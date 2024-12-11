namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using Requests;

    public sealed class CreateStreetNameSnapshotSqsRequest : SqsRequest
    {
        public CreateStreetNameSnapshotRequest Request { get; init; }
    }
}
