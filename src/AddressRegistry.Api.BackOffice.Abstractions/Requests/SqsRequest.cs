namespace AddressRegistry.Api.BackOffice.Abstractions.Requests
{
    using System;
    using System.Collections.Generic;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Newtonsoft.Json;

    public class SqsRequest : IRequest<IResult>
    {
        [JsonIgnore]
        public IDictionary<string, object> Metadata { get; set; }

        [JsonIgnore]
        public string? MessageGroupId { get; set; }

        [JsonIgnore]
        public Guid TicketId { get; set; }
    }
}
