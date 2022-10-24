namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class DeregulateSqsRequest : SqsRequest, IHasBackOfficeRequest<DeregulateBackOfficeRequest>
    {
        public DeregulateBackOfficeRequest Request { get; set; }
    }
}
