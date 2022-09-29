namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressCorrectRejectionRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeCorrectAddressFromRejectedRequest>
    {
        public AddressBackOfficeCorrectAddressFromRejectedRequest Request { get; set; }
    }
}
