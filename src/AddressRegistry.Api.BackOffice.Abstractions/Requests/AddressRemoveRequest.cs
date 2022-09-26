namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using MediatR;
    using Newtonsoft.Json;
    using Responses;

    public class AddressRemoveRequest : AddressBackOfficeRemoveRequest, IRequest<ETagResponse>
    {
        [JsonIgnore]
        public IDictionary<string, object?> Metadata { get; set; }
    }
}
