namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressCorrectRetirementRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeCorrectRetirementRequest>
    {
        public AddressBackOfficeCorrectRetirementRequest Request { get; set; }
    }
}
