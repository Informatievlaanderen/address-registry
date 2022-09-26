namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using MediatR;
    using StreetName;

    public class SqsLambdaRequest : IRequest
    {
        public Guid TicketId { get; set; }
        public string MessageGroupId { get; set; }
        public string? IfMatchHeaderValue { get; set; }
        public Provenance Provenance { get; set; }
        public IDictionary<string, object> Metadata { get; set; }

        public StreetNamePersistentLocalId StreetNamePersistentLocalId =>
            new StreetNamePersistentLocalId(Convert.ToInt32(MessageGroupId));
    }
}
