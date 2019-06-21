namespace AddressRegistry.Api.Legacy.Address.Responses
{
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
    using Query;
    using Swashbuckle.AspNetCore.Filters;
    using System;
    using System.Globalization;
    using System.Net.Mime;
    using System.Runtime.Serialization;
    using System.Threading.Tasks;
    using System.Xml;
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
                Published = address.RecordCreatedAt.ToDateTimeOffset(),
                LastUpdated = address.LastChangedOn.ToDateTimeOffset(),
                Description = BuildDescription(address, responseOptions.Value.Naamruimte)
            };

            // TODO: Hier moet prolly version nog ergens in
            item.AddLink(
                new SyndicationLink(
                    new Uri($"{responseOptions.Value.Naamruimte}/{address.OsloId}"),
                    AtomLinkTypes.Related));

            item.AddLink(
                new SyndicationLink(
                    new Uri(string.Format(responseOptions.Value.DetailUrl, address.OsloId)),
                    AtomLinkTypes.Self));

            item.AddLink(
                new SyndicationLink(
                        new Uri(string.Format($"{responseOptions.Value.DetailUrl}.xml", address.OsloId)),
                        AtomLinkTypes.Alternate)
                { MediaType = MediaTypeNames.Application.Xml });

            item.AddLink(
                new SyndicationLink(
                        new Uri(string.Format($"{responseOptions.Value.DetailUrl}.json", address.OsloId)),
                        AtomLinkTypes.Alternate)
                { MediaType = MediaTypeNames.Application.Json });

            item.AddCategory(
                new SyndicationCategory(category));

            item.AddContributor(
                new SyndicationPerson(
                    "agentschap Informatie Vlaanderen",
                    "informatie.vlaanderen@vlaanderen.be",
                    AtomContributorTypes.Author));

            await writer.Write(item);
        }

        private static string BuildDescription(AddressSyndicationQueryResult address, string naamruimte)
        {
            if (!address.ContainsEvent && !address.ContainsObject)
                return "No data embedded";

            var content = new SyndicationContent();
            if (address.ContainsObject)
                content.Object = new AddressSyndicationContent(address.AddressId,
                    naamruimte,
                    address.StreetNameId,
                    address.OsloId,
                    address.HouseNumber,
                    address.BoxNumber,
                    address.PostalCode,
                    address.PointPosition == null ? null : AddressMapper.GetAddressPoint(address.PointPosition),
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
        public Guid AddressId { get; set; }

        /// <summary>
        /// De identificator van het adres.
        /// </summary>
        [DataMember(Name = "Identificator", Order = 2)]
        public Identificator Identificator { get; set; }

        /// <summary>
        /// De id van de straatnaam.
        /// </summary>
        [DataMember(Name = "StraatnaamId", Order = 3)]
        public Guid? SteetnameId { get; set; }

        /// <summary>
        /// De id van de postinfo.
        /// </summary>
        [DataMember(Name = "PostCode", Order = 4)]
        public string PostalCode { get; set; }

        /// <summary>
        /// Het huisnummer.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 5)]
        public string HouseNumber { get; set; }

        /// <summary>
        /// Het nummer van de bus.
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
        /// False wanneer het bestaan van het adres niet geweten is ten tijde van administratieve procedures, maar pas na plaatselijke observatie.
        /// </summary>
        [DataMember(Name = "OfficieelToegekend", Order = 12)]
        public bool IsOfficiallyAssigned { get; set; }

        /// <summary>
        /// Creatie data ivm het item.
        /// </summary>
        [DataMember(Name = "Creatie", Order = 13)]
        public Provenance Provenance { get; set; }

        public AddressSyndicationContent(
            Guid addressId,
            string naamruimte,
            Guid? streetNameId,
            int? osloId,
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
            Identificator = new Identificator(naamruimte, osloId.HasValue ? osloId.ToString() : string.Empty, version);
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

            Provenance = new Provenance(organisation, new Reason(reason));
        }
    }

    public class AddressSyndicationResponseExamples : IExamplesProvider
    {
        private SyndicationContent ContentExample =>
            new SyndicationContent
            {
                Object = new AddressSyndicationContent(
                    Guid.NewGuid(),
                    _responseOptions.Naamruimte,
                    Guid.NewGuid(),
                    13023,
                    "70",
                    null,
                    "9000",
                    new Point { XmlPoint = new GmlPoint { Pos = "188473.52 193390.22" } },
                    PositieGeometrieMethode.AfgeleidVanObject,
                    PositieSpecificatie.Gebouweenheid,
                    AdresStatus.InGebruik,
                    DateTimeOffset.Now,
                    true,
                    true,
                    Organisation.Agiv,
                    Reason.CentralManagementCrab)
            };

        private readonly ResponseOptions _responseOptions;

        public AddressSyndicationResponseExamples(IOptions<ResponseOptions> responseOptionsProvider)
            => _responseOptions = responseOptionsProvider.Value;

        public object GetExamples()
        {
            return $@"<?xml version=""1.0"" encoding=""utf-8""?>
<feed xmlns=""http://www.w3.org/2005/Atom"">
  <id>https://basisregisters.vlaanderen/syndication/feed/address.atom</id>
  <title>Basisregisters Vlaanderen - Adresregister</title>
  <subtitle>Basisregisters Vlaanderen stelt u in staat om alles te weten te komen rond: de Belgische gemeenten; de Belgische postcodes; de Vlaamse straatnamen; de Vlaamse adressen; de Vlaamse gebouwen en gebouweenheden; de Vlaamse percelen; de Vlaamse organisaties en organen; de Vlaamse dienstverlening.</subtitle>
  <generator uri=""https://basisregisters.vlaanderen"" version=""2.0.0.0"">Basisregisters Vlaanderen</generator>
  <rights>Copyright (c) 2017-2018, Informatie Vlaanderen</rights>
  <updated>2018-10-05T14:06:53Z</updated>
  <author>
    <name>agentschap Informatie Vlaanderen</name>
    <email>informatie.vlaanderen@vlaanderen.be</email>
  </author>
  <link href=""https://basisregisters.vlaanderen/syndication/feed/address.atom"" rel=""self"" />
  <link href=""https://legacy.staging-basisregisters.vlaanderen/"" rel=""related"" />
  <link href=""https://legacy.staging-basisregisters.vlaanderen/v1/feeds/adressen.atom?offset=100&limit=100"" rel=""next""/>
  <entry>
    <id>4</id>
    <title>AddressWasRegistered-4</title>
    <updated>2018-10-04T13:12:17Z</updated>
    <published>2018-10-04T13:12:17Z</published>
    <link href=""{_responseOptions.Naamruimte}/13023"" rel=""related"" />
    <link href=""https://basisregisters.vlaanderen.be/api/v1/adressen/13023"" rel=""self"" />
    <link href=""https://basisregisters.vlaanderen.be/api/v1/adressen/13023.xml"" rel=""alternate"" type=""application/xml"" />
    <link href=""https://basisregisters.vlaanderen.be/api/v1/adressen/13023.json"" rel=""alternate"" type=""application/json"" />
    <author>
      <name>agentschap Informatie Vlaanderen</name>
      <email>informatie.vlaanderen@vlaanderen.be</email>
    </author>
    <category term=""https://data.vlaanderen.be/ns/adres"" />
    <content><![CDATA[{ContentExample.ToXml()}]]></content>
  </entry>
</feed>";
        }
    }
}
