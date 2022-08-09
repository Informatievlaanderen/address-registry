namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;
    using Responses;
    using StreetName;
    using StreetName.Commands;
    using Swashbuckle.AspNetCore.Filters;

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
                new HouseNumber(Huisnummer),
                string.IsNullOrWhiteSpace(Busnummer) ? null : new BoxNumber(Busnummer),
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
