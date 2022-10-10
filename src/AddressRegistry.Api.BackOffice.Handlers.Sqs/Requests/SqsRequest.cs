namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using MediatR;

    public class SqsRequest : IRequest<LocationResult>
    {
        public Guid TicketId { get; set; }
        public string? IfMatchHeaderValue { get; set; }
        public ProvenanceData? ProvenanceData { get; set; }
        public IDictionary<string, object?>? Metadata { get; set; }
    }

    public record LocationResult(Uri Location);
}
