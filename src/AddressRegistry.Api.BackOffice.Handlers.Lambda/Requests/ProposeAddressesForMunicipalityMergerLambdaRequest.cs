namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.SqsRequests;

    public sealed record ProposeAddressesForMunicipalityMergerLambdaRequest : AddressLambdaRequest
    {
        public IEnumerable<ProposeAddressesForMunicipalityMergerSqsRequestItem> Addresses { get; }

        public ProposeAddressesForMunicipalityMergerLambdaRequest(
            string groupId,
            ProposeAddressesForMunicipalityMergerSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                null,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            Addresses = sqsRequest.Addresses;
        }
    }
}
