namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RetireSqsRequest : SqsRequest, IHasBackOfficeRequest<RetireBackOfficeRequest>
    {
        public RetireBackOfficeRequest Request { get; set; }
    }
}
