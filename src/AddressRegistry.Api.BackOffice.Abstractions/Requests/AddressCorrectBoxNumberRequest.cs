namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using MediatR;
    using Newtonsoft.Json;
    using StreetName;
    using StreetName.Commands;
    using Swashbuckle.AspNetCore.Filters;

    [DataContract(Name = "CorrigerenBusnummerAdres", Namespace = "")]
    public class AddressCorrectBoxNumberRequest : CorrectBoxNumberBackOfficeRequest, IRequest<ETagResponse>
    {
        /// <summary>
        /// De unieke en persistente identificator van het adres.
        /// </summary>
        [JsonIgnore]
        public int PersistentLocalId { get; set; }

        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Map to CorrectAddressBoxNumber command
        /// </summary>
        /// <returns>CorrectAddressBoxNumber.</returns>
        public CorrectAddressBoxNumber ToCommand(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            Provenance provenance)
        {
            return new CorrectAddressBoxNumber(
                streetNamePersistentLocalId,
                new AddressPersistentLocalId(PersistentLocalId),
                BoxNumber.Create(Busnummer),
                provenance);
        }
    }

    public class AddressCorrectBoxNumberRequestExamples : IExamplesProvider<AddressCorrectBoxNumberRequest>
    {
        public AddressCorrectBoxNumberRequest GetExamples()
        {
            return new AddressCorrectBoxNumberRequest
            {
                Busnummer = "1A",
            };
        }
    }
}
