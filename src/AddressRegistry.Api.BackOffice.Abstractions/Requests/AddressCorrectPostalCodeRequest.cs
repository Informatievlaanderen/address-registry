namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using MediatR;
    using Newtonsoft.Json;
    using Responses;
    using StreetName;
    using StreetName.Commands;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "CorrigerenPostcodeAdres", Namespace = "")]
    public class AddressCorrectPostalCodeRequest : IRequest<ETagResponse>
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [JsonIgnore]
        public int PersistentLocalId { get; set; }

        /// <summary>
        /// De unieke en persistente identificator van de postcode van het adres.
        /// </summary>
        [DataMember(Name = "PostinfoId", Order = 0)]
        [JsonProperty(Required = Required.Always)]
        public string PostInfoId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Map to CorrectAddressPostalCode command
        /// </summary>
        /// <returns>CorrectAddressPostalCode.</returns>
        public CorrectAddressPostalCode ToCommand(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            PostalCode postalCode,
            MunicipalityId postalCodeMunicipalityId,
            Provenance provenance)
        {
            return new CorrectAddressPostalCode(
                streetNamePersistentLocalId,
                new AddressPersistentLocalId(PersistentLocalId),
                postalCode,
                postalCodeMunicipalityId,
                provenance);
        }
    }

    public class AddressCorrectPostalCodeRequestExamples : IExamplesProvider<AddressCorrectPostalCodeRequest>
    {
        public AddressCorrectPostalCodeRequest GetExamples()
        {
            return new AddressCorrectPostalCodeRequest
            {
                PostInfoId = "https://data.vlaanderen.be/id/postinfo/9000"
            };
        }
    }
}
