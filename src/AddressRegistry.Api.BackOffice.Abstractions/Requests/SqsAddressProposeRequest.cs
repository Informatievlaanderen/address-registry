namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Converters;
    using Newtonsoft.Json;
    using StreetName;
    using StreetName.Commands;

    [DataContract(Name = "VoorstelAdres", Namespace = "")]
    public class SqsAddressProposeRequest : SqsRequest
    {
        /// <summary>
        /// De unieke en persistente identificator van de postcode van het adres.
        /// </summary>
        [DataMember(Name = "PostinfoId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string PostInfoId { get; set; }

        /// <summary>
        /// De unieke en persistente identificator van de straatnaam van het adres.
        /// </summary>
        [DataMember(Name = "StraatnaamId", Order = 1)]
        [JsonProperty(Required = Required.Always)]
        public string StraatNaamId { get; set; }

        /// <summary>
        /// Het huisnummer van het adres.
        /// </summary>
        [DataMember(Name = "Huisnummer", Order = 2)]
        [JsonProperty(Required = Required.Always)]
        public string Huisnummer { get; set; }

        /// <summary>
        /// Het busnummer van het adres (optioneel).
        /// </summary>
        [DataMember(Name = "Busnummer", Order = 3)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public string? Busnummer { get; set; }

        /// <summary>
        /// De geometriemethode van het adres.
        /// </summary>
        [DataMember(Name= "PositieGeometriemethode", Order = 4)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PositieGeometrieMethode? PositieGeometrieMethode { get; set; }

        /// <summary>
        /// De specificatie van de adrespositie (optioneel).
        /// </summary>
        [DataMember(Name = "PositieSpecificatie", Order = 5)]
        [JsonProperty(Required = Required.Default, DefaultValueHandling = DefaultValueHandling.Ignore)]
        public PositieSpecificatie? PositieSpecificatie { get; set; }

        /// <summary>
        /// Puntgeometrie van het adres in GML-3 formaat met Lambert 72 referentie systeem.
        /// </summary>
        [DataMember(Name = "Positie", Order = 6)]
        [JsonProperty(Required = Required.Default)]
        public string? Positie { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Map to ProposeAddress command
        /// </summary>
        /// <returns>ProposeAddress.</returns>
        public ProposeAddress ToCommand(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            PostalCode postalCode,
            MunicipalityId postalCodeMunicipalityId,
            AddressPersistentLocalId addressPersistentLocalId,
            Provenance provenance)
        {
            return new ProposeAddress(
                streetNamePersistentLocalId,
                postalCode,
                postalCodeMunicipalityId,
                addressPersistentLocalId,
                HouseNumber.Create(Huisnummer),
                string.IsNullOrWhiteSpace(Busnummer) ? null : new BoxNumber(Busnummer),
                PositieGeometrieMethode.Map(),
                PositieSpecificatie.Map(),
                string.IsNullOrWhiteSpace(Positie) ? null : Positie.ToExtendedWkbGeometry(),
                provenance);
        }
    }

    //public class AddressProposeRequestExamples : IExamplesProvider<AddressProposeRequest>
    //{
    //    public AddressProposeRequest GetExamples()
    //    {
    //        return new AddressProposeRequest
    //        {
    //            StraatNaamId = "https://data.vlaanderen.be/id/straatnaam/45041",
    //            PostInfoId = "https://data.vlaanderen.be/id/postinfo/9000",
    //            Huisnummer = "11",
    //            Busnummer = "3A"
    //        };
    //    }
    //}
}
