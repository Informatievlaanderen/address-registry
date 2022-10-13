namespace AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests
{
    using Abstractions.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class SqsAddressProposeRequest : SqsRequest, IHasBackOfficeRequest<AddressBackOfficeProposeRequest>
    {
        public AddressBackOfficeProposeRequest Request { get; set; }
    }
}
