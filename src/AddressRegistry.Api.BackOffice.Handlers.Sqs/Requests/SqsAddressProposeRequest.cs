namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;

    public sealed class SqsAddressProposeRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeProposeRequest>
    {
        public AddressBackOfficeProposeRequest Request { get; set; }
    }
}
