namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RegularizeSqsRequest : SqsRequest, IHasBackOfficeRequest<RegularizeBackOfficeRequest>
    {
        public RegularizeBackOfficeRequest Request { get; set; }
    }
}
