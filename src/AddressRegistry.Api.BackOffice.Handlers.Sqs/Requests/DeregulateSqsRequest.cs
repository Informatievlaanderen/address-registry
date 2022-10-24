namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class DeregulateSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeDeregulateRequest>
    {
        public BackOfficeDeregulateRequest Request { get; set; }
    }
}
