namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using MediatR;
    using Newtonsoft.Json;
    using Responses;

    [DataContract(Name = "RegulariserenAdres", Namespace = "")]
    public class AddressRegularizeRequest : AddressBackOfficeRegularizeRequest, IRequest<ETagResponse>
    {
        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }
    }
}
