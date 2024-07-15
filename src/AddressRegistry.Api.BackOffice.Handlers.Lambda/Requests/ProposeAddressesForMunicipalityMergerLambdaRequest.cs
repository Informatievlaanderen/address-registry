namespace AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests
{
    using Abstractions.SqsRequests;

    public sealed record ProposeAddressesForMunicipalityMergerLambdaRequest : AddressLambdaRequest
    {
        private readonly IEnumerable<ProposeAddressesForMunicipalityMergerSqsRequestItem> _addresses;
        public string NisCode { get; }

        public ProposeAddressesForMunicipalityMergerLambdaRequest(
            string groupId,
            ProposeAddressesForMunicipalityMergerSqsRequest sqsRequest)
            : base(
                groupId,
                sqsRequest.TicketId,
                sqsRequest.IfMatchHeaderValue,
                sqsRequest.ProvenanceData.ToProvenance(),
                sqsRequest.Metadata)
        {
            _addresses = sqsRequest.Addresses;
            NisCode = sqsRequest.NisCode;
        }
    }
}
