namespace AddressRegistry.Api.Oslo.Address.Sync
{
    using System;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Syndication;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Convertors;
    using Infrastructure.Options;
    using Microsoft.Extensions.Options;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;
    using Swashbuckle.AspNetCore.Filters;
    using Provenance = Be.Vlaanderen.Basisregisters.GrAr.Provenance.Syndication.Provenance;

    public static class AddressSyndicationResponse
    {
        public static async Task WriteAddress(
            this ISyndicationFeedWriter writer,
            IOptions<ResponseOptions> responseOptions,
            AtomFormatter formatter,
            string category,
            AddressSyndicationQueryResult address)
        {
            var item = new SyndicationItem
            {
                Id = address.Position.ToString(CultureInfo.InvariantCulture),
                Title = $"{address.ChangeType}-{address.Position}",
                Published = address.RecordCreatedAt.ToBelgianDateTimeOffset(),
                LastUpdated = address.LastChangedOn.ToBelgianDateTimeOffset(),
                Description = BuildDescription(address, responseOptions.Value.Naamruimte)
            };

            if (address.PersistentLocalId.HasValue)
            {
                item.AddLink(
                    new SyndicationLink(
                        new Uri($"{responseOptions.Value.Naamruimte}/{address.PersistentLocalId}"),
                        AtomLinkTypes.Related));

                //item.AddLink(
                //    new SyndicationLink(
                //        new Uri(string.Format(responseOptions.Value.DetailUrl, address.PersistentLocalId)),
                //        AtomLinkTypes.Self));

                //item.AddLink(
                //    new SyndicationLink(
                //            new Uri(string.Format($"{responseOptions.Value.DetailUrl}.xml", address.PersistentLocalId)), AtomLinkTypes.Alternate)
                //    { MediaType = MediaTypeNames.Application.Xml });

                //item.AddLink(
                //    new SyndicationLink(
                //            new Uri(string.Format($"{responseOptions.Value.DetailUrl}.json",
                //                address.PersistentLocalId)),
                //            AtomLinkTypes.Alternate)
                //    { MediaType = MediaTypeNames.Application.Json });
            }

            item.AddCategory(
                new SyndicationCategory(category));

            item.AddContributor(
                new SyndicationPerson(
                    address.Organisation == null ? Organisation.Unknown.ToName() : address.Organisation.Value.ToName(),
                    string.Empty,
                    AtomContributorTypes.Author));

            await writer.Write(item);
        }

        private static string BuildDescription(AddressSyndicationQueryResult address, string naamruimte)
        {
            if (!address.ContainsEvent && !address.ContainsObject)
                return "No data embedded";

            var content = new SyndicationContent();
            if (address.ContainsObject)
                content.Object = new AddressSyndicationContent(
                    address.AddressId.HasValue ? address.AddressId.Value.ToString("D") : address.PersistentLocalId.Value.ToString(),
                    naamruimte,
                    address.StreetNameId.HasValue ? address.StreetNameId.Value.ToString("D") : address.StreetNamePersistentLocalId.ToString(),
                    address.PersistentLocalId,
                    address.HouseNumber,
                    address.BoxNumber,
                    address.PostalCode,
                    address.PointPosition == null ? (Point?)null : AddressMapper.GetAddressPoint(address.PointPosition),
                    address.GeometryMethod == null ? (PositieGeometrieMethode?)null : AddressMapper.ConvertFromGeometryMethod(address.GeometryMethod.Value),
                    address.GeometrySpecification == null ? (PositieSpecificatie?)null : AddressMapper.ConvertFromGeometrySpecification(address.GeometrySpecification.Value),
                    address.Status.ConvertFromAddressStatus(),
                    address.LastChangedOn.ToBelgianDateTimeOffset(),
                    address.IsComplete,
                    address.IsOfficiallyAssigned,
                    address.Organisation,
                    address.Reason);

            if (address.ContainsEvent)
            {
                var doc = new XmlDocument();
                doc.LoadXml(address.EventDataAsXml);
                content.Event = doc.DocumentElement;
            }

            return content.ToXml();
        }
    }

    [DataContract(Name = "Content", Namespace = "")]
    public class SyndicationContent : SyndicationContentBase
    {
        [DataMember(Name = "Event")]
        public XmlElement Event { get; set; }

        [DataMember(Name = "Object")]
        public AddressSyndicationContent Object { get; set; }
    }

    [DataContract(Name = "Adres", Namespace = "")]
    public class AddressSyndicationContent
    {
        /// <summary>
        /// De technische id van het adres.
        /// </summary>
        [DataMember(Name = "Id", Order = 1)]
        public string AddressId { get; set; }

        /// <summary>
        /// De identificator van het adres.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        public AdresIdentificator Identificator { get; set; }

        /// <summary>
        /// De id van de straatnaam.
        /// </summary>
        [DataMember(Name = "StraatnaamId", Order = 3)]
        public string? SteetnameId { get; set; }

        /// <summary>
        /// De id van de postinfo.
        /// </summary>
        [DataMember(Name = "PostCode", Order = 4)]
        public string PostalCode { get; set; }

        /// <summary>
        /// Het huisnummer van het adres.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 5)]
        public string HouseNumber { get; set; }

        /// <summary>
        /// Het busnummer van het adres.
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 6)]
        public string BoxNumber { get; set; }

        /// <summary>
        /// De fase in het leven van het adres.
        /// </summary>
        [DataMember(Name = "AdresStatus", Order = 7)]
        public AdresStatus? AdressStatus { get; set; }

        /// <summary>
        /// De positie van het adres.
        /// </summary>
        [DataMember(Name = "AdresPositie", Order = 8)]
        public SyndicationPoint Point { get; set; }

        /// <summary>
        /// De gebruikte methode om de positie te bepalen.
        /// </summary>
        [DataMember(Name = "PositieGeometrieMethode", Order = 9)]
        public PositieGeometrieMethode? GeometryMethod { get; set; }

        /// <summary>
        /// De specificatie van het object, voorgesteld door de positie.
        /// </summary>
        [DataMember(Name = "PositieSpecificatie", Order = 10)]
        public PositieSpecificatie? PositionSpecification { get; set; }

        /// <summary>
        /// Duidt aan of het item compleet is.
        /// </summary>
        [DataMember(Name = "IsCompleet", Order = 11)]
        public bool IsComplete { get; set; }

        /// <summary>
        /// False wanneer het bestaan van het adres niet geweten is ten tijde van administratieve procedures, maar pas na observatie op het terrein.
        /// </summary>
        [DataMember(Name = "OfficieelToegekend", Order = 12)]
        public bool IsOfficiallyAssigned { get; set; }

        /// <summary>
        /// Creatie data ivm het item.
        /// </summary>
        [DataMember(Name = "Creatie", Order = 13)]
        public Provenance Provenance { get; set; }

        public AddressSyndicationContent(
            string addressId,
            string naamruimte,
            string? streetNameId,
            int? persistentLocalId,
            string houseNumber,
            string boxNumber,
            string postalCode,
            Point point,
            PositieGeometrieMethode? geometryMethod,
            PositieSpecificatie? positionSpecification,
            AdresStatus? status,
            DateTimeOffset version,
            bool isComplete,
            bool isOfficiallyAssigned,
            Organisation? organisation,
            string reason)
        {
            AddressId = addressId;
            Identificator = new AdresIdentificator(naamruimte, persistentLocalId?.ToString(CultureInfo.InvariantCulture), version);
            SteetnameId = streetNameId;
            PostalCode = postalCode;
            Point = point == null ? null : new SyndicationPoint { XmlPoint = point.XmlPoint };
            GeometryMethod = geometryMethod;
            PositionSpecification = positionSpecification;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            AdressStatus = status;
            IsComplete = isComplete;
            IsOfficiallyAssigned = isOfficiallyAssigned;

            Provenance = new Provenance(version, organisation, new Reason(reason));
        }
    }

    public class AddressSyndicationResponseExamples : IExamplesProvider<XmlElement>
    {
        private const string RawXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"">
    <id>https://api.basisregisters.vlaanderen.be/v2/feeds/adressen.atom</id>
    <title>Basisregisters Vlaanderen - feed 'adressen'</title>
    <subtitle>Deze Atom feed geeft leestoegang tot events op de resource 'adressen'.</subtitle>
    <generator uri=""https://basisregisters.vlaanderen.be"" version=""2.2.23.4"">Basisregisters Vlaanderen</generator>
    <rights>Gratis hergebruik volgens https://overheid.vlaanderen.be/sites/default/files/documenten/ict-egov/licenties/hergebruik/modellicentie_gratis_hergebruik_v1_0.html</rights>
    <updated>2020-11-07T01:22:17Z</updated>
    <author>
        <name>Digitaal Vlaanderen</name>
        <email>digitaal.vlaanderen@vlaanderen.be</email>
    </author>
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/adressen"" rel=""self"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/adressen.atom"" rel=""alternate"" type=""application/atom+xml"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/adressen.xml"" rel=""alternate"" type=""application/xml"" />
    <link href=""https://docs.basisregisters.vlaanderen.be/"" rel=""related"" />
    <link href=""https://api.basisregisters.vlaanderen.be/v2/feeds/adressen?from=3&amp;limit=100&amp;embed=event,object"" rel=""next"" />
    <entry>
        <id>0</id>
        <title>AddressWasRegistered-0</title>
        <updated>2003-12-07T04:19:50+01:00</updated>
        <published>2003-12-07T04:19:50+01:00</published>
        <author>
            <name>Federale Overheidsdienst Financiën (Algemene Administratie van de Patrimoniumdocumentatie)</name>
        </author>
        <category term=""adressen"" />
        <content>
            <![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Event><AddressWasRegistered><AddressId>e45d2dea-56ed-5009-bea2-218d3f10b7a3</AddressId><StreetNameId>a8a7a581-20d4-5331-9271-c4c6198b2909</StreetNameId><HouseNumber>16</HouseNumber><Provenance><Timestamp>2003-12-07T03:19:50Z</Timestamp><Organisation>Akred</Organisation><Reason>Centrale bijhouding CRAB</Reason></Provenance>
    </AddressWasRegistered>
  </Event><Object><Id>e45d2dea-56ed-5009-bea2-218d3f10b7a3</Id><Identificator><Id>https://data.vlaanderen.be/id/adres/</Id><Naamruimte>https://data.vlaanderen.be/id/adres</Naamruimte><ObjectId i:nil=""true"" /><VersieId>2003-12-07T04:19:50+01:00</VersieId></Identificator><StraatnaamId>a8a7a581-20d4-5331-9271-c4c6198b2909</StraatnaamId><PostCode i:nil=""true"" /><Huisnummer>16</Huisnummer><Busnummer i:nil=""true"" /><AdresStatus i:nil=""true"" /><AdresPositie i:nil=""true"" /><PositieGeometrieMethode i:nil=""true"" /><PositieSpecificatie i:nil=""true"" /><IsCompleet>false</IsCompleet><OfficieelToegekend>false</OfficieelToegekend><Creatie><Tijdstip>2003-12-07T04:19:50+01:00</Tijdstip><Organisatie>Federale Overheidsdienst Financiën (Algemene Administratie van de Patrimoniumdocumentatie)</Organisatie><Reden>Centrale bijhouding CRAB</Reden></Creatie>
  </Object></Content>]]>
</content>
</entry>
<entry>
    <id>2</id>
    <title>AddressPostalCodeWasChanged-2</title>
    <updated>2003-12-08T09:11:49+01:00</updated>
    <published>2003-12-07T04:19:50+01:00</published>
    <author>
        <name>Federale Overheidsdienst Financiën (Algemene Administratie van de Patrimoniumdocumentatie)</name>
    </author>
    <category term=""adressen"" />
    <content>
        <![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance""><Event><AddressPostalCodeWasChanged><AddressId>e45d2dea-56ed-5009-bea2-218d3f10b7a3</AddressId><PostalCode>8730</PostalCode><Provenance><Timestamp>2003-12-08T08:11:49Z</Timestamp><Organisation>Akred</Organisation><Reason>Centrale bijhouding CRAB</Reason></Provenance>
    </AddressPostalCodeWasChanged>
  </Event><Object><Id>e45d2dea-56ed-5009-bea2-218d3f10b7a3</Id><Identificator><Id>https://data.vlaanderen.be/id/adres/</Id><Naamruimte>https://data.vlaanderen.be/id/adres</Naamruimte><ObjectId i:nil=""true"" /><VersieId>2003-12-08T09:11:49+01:00</VersieId></Identificator><StraatnaamId>a8a7a581-20d4-5331-9271-c4c6198b2909</StraatnaamId><PostCode>8730</PostCode><Huisnummer>16</Huisnummer><Busnummer i:nil=""true"" /><AdresStatus i:nil=""true"" /><AdresPositie i:nil=""true"" /><PositieGeometrieMethode i:nil=""true"" /><PositieSpecificatie i:nil=""true"" /><IsCompleet>false</IsCompleet><OfficieelToegekend>false</OfficieelToegekend><Creatie><Tijdstip>2003-12-08T09:11:49+01:00</Tijdstip><Organisatie>Federale Overheidsdienst Financiën (Algemene Administratie van de Patrimoniumdocumentatie)</Organisatie><Reden>Centrale bijhouding CRAB</Reden></Creatie>
  </Object></Content>]]>
</content>
</entry>
<entry>
    <id>132217759</id>
    <title>AddressWasMigratedToStreetName-132217759</title>
    <updated>2023-11-01T11:44:38+01:00</updated>
    <published>2023-11-01T11:44:38+01:00</published>
    <link href=""https://data.vlaanderen.be/id/adres/3064179"" rel=""related"" />
    <author>
      <name>Digitaal Vlaanderen</name>
    </author>
    <category term=""adressen"" />
    <content><![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
  <Event>
    <AddressWasMigratedToStreetName>
      <StreetNamePersistentLocalId>44935</StreetNamePersistentLocalId>
      <AddressId>996b2463-0ce2-541e-95a0-0f51f8478d0e</AddressId>
      <StreetNameId>b4b0e993-cc85-53b6-836b-7ec08e7c6910</StreetNameId>
      <AddressPersistentLocalId>3064179</AddressPersistentLocalId>
      <Status>Current</Status>
      <HouseNumber>174B</HouseNumber>
      <BoxNumber />
      <GeometryMethod>DerivedFromObject</GeometryMethod>
      <GeometrySpecification>BuildingUnit</GeometrySpecification>
      <ExtendedWkbGeometry>01010000208A7A0000EC51B81E6108F2403D0AD7A394D50941</ExtendedWkbGeometry>
      <OfficiallyAssigned>true</OfficiallyAssigned>
      <PostalCode>8310</PostalCode>
      <IsCompleted>true</IsCompleted>
      <IsRemoved>false</IsRemoved>
      <ParentPersistentLocalId />
      <Provenance>
        <Timestamp>2023-11-01T10:44:38Z</Timestamp>
        <Organisation>DigitaalVlaanderen</Organisation>
        <Reason>Migrate Address aggregate to StreetName.</Reason>
      </Provenance>
    </AddressWasMigratedToStreetName>
  </Event>
  <Object>
    <Id>3064179</Id>
    <Identificator>
      <Id>https://data.vlaanderen.be/id/adres/3064179</Id>
      <Naamruimte>https://data.vlaanderen.be/id/adres</Naamruimte>
      <ObjectId>3064179</ObjectId>
      <VersieId>2023-11-01T11:44:38+01:00</VersieId>
    </Identificator>
    <StraatnaamId>44935</StraatnaamId>
    <PostCode>8310</PostCode>
    <Huisnummer>174B</Huisnummer>
    <Busnummer i:nil=""true"" />
    <AdresStatus>InGebruik</AdresStatus>
    <AdresPositie>
      <point>
        <pos>73862.07 211634.58</pos>
      </point>
    </AdresPositie>
    <PositieGeometrieMethode>AfgeleidVanObject</PositieGeometrieMethode>
    <PositieSpecificatie>Gebouweenheid</PositieSpecificatie>
    <IsCompleet>true</IsCompleet>
    <OfficieelToegekend>true</OfficieelToegekend>
    <Creatie>
      <Tijdstip>2023-11-01T11:44:38+01:00</Tijdstip>
      <Organisatie>Digitaal Vlaanderen</Organisatie>
      <Reden>Migrate Address aggregate to StreetName.</Reden>
    </Creatie>
  </Object>
</Content>]]></content>
</entry>
<entry>
    <id>140807758</id>
    <title>AddressWasProposedV2-140807758</title>
    <updated>2024-11-21T10:14:32+01:00</updated>
    <published>2024-11-21T10:14:32+01:00</published>
    <link href=""https://data.vlaanderen.be/id/adres/30408210"" rel=""related"" />
    <author>
      <name>Andere</name>
    </author>
    <category term=""adressen"" />
    <content><![CDATA[<Content xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">
  <Event>
    <AddressWasProposedV2>
      <StreetNamePersistentLocalId>13813</StreetNamePersistentLocalId>
      <AddressPersistentLocalId>30408210</AddressPersistentLocalId>
      <ParentPersistentLocalId />
      <PostalCode>2440</PostalCode>
      <HouseNumber>28</HouseNumber>
      <BoxNumber />
      <GeometryMethod>DerivedFromObject</GeometryMethod>
      <GeometrySpecification>BuildingUnit</GeometrySpecification>
      <ExtendedWkbGeometry>010100000048E17A148485074166666666FE1B0941</ExtendedWkbGeometry>
      <Provenance>
        <Timestamp>2024-11-21T09:14:32Z</Timestamp>
        <Organisation>Other</Organisation>
        <Reason>
        </Reason>
      </Provenance>
    </AddressWasProposedV2>
  </Event>
  <Object>
    <Id>30408210</Id>
    <Identificator>
      <Id>https://data.vlaanderen.be/id/adres/30408210</Id>
      <Naamruimte>https://data.vlaanderen.be/id/adres</Naamruimte>
      <ObjectId>30408210</ObjectId>
      <VersieId>2024-11-21T10:14:32+01:00</VersieId>
    </Identificator>
    <StraatnaamId>13813</StraatnaamId>
    <PostCode>2440</PostCode>
    <Huisnummer>28</Huisnummer>
    <Busnummer i:nil=""true"" />
    <AdresStatus>Voorgesteld</AdresStatus>
    <AdresPositie>
      <point>
        <pos>192688.51 205695.80</pos>
      </point>
    </AdresPositie>
    <PositieGeometrieMethode>AfgeleidVanObject</PositieGeometrieMethode>
    <PositieSpecificatie>Gebouweenheid</PositieSpecificatie>
    <IsCompleet>true</IsCompleet>
    <OfficieelToegekend>true</OfficieelToegekend>
    <Creatie>
      <Tijdstip>2024-11-21T10:14:32+01:00</Tijdstip>
      <Organisatie>Andere</Organisatie>
      <Reden />
    </Creatie>
  </Object>
</Content>]]></content>
</entry>
</feed>";

        public XmlElement GetExamples()
        {
            var example = new XmlDocument();
            example.LoadXml(RawXml);
            return example.DocumentElement;
        }
    }
}
