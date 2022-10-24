namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class RegularizeSqsRequest : SqsRequest, IHasBackOfficeRequest<BackOfficeRegularizeRequest>
    {
        public BackOfficeRegularizeRequest Request { get; set; }
    }
}
