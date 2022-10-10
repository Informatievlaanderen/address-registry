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

    [DataContract(Name = "CorrigerenAfkeuringAdres", Namespace = "")]
    public class CorrectAddressFromRejectedToProposedRequest : AddressBackOfficeCorrectAddressFromRejectedRequest, IRequest<ETagResponse>
    {
        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        /// <summary>
        /// Map to CorrectRejection command
        /// </summary>
        /// <returns>CorrectRejection.</returns>
        public CorrectAddressRejection ToCommand(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            Provenance provenance)
        {
            return new CorrectAddressRejection(
                streetNamePersistentLocalId,
                new AddressPersistentLocalId(PersistentLocalId),
                provenance);
        }
    }
}
