namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using MediatR;
    using Newtonsoft.Json;

    public class AddressDeregulateRequest : AddressBackOfficeDeregulateRequest, IRequest<ETagResponse>
    {
        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }
    }
}
