namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using System.Collections.Generic;
    using Address;
    using Requests;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ProposeAddressSqsRequest : SqsRequest, IHasBackOfficeRequest<ProposeAddressRequest>
    {
        public int PersistentLocalId { get; set; }
        public ProposeAddressRequest Request { get; init; }
    }

    public sealed class ProposeAddressSqsRequestFactory
    {
        private readonly IPersistentLocalIdGenerator _persistentLocalIdGenerator;

        public ProposeAddressSqsRequestFactory(IPersistentLocalIdGenerator persistentLocalIdGenerator)
        {
            _persistentLocalIdGenerator = persistentLocalIdGenerator;
        }

        public ProposeAddressSqsRequest Create(ProposeAddressRequest request, IDictionary<string, object?> metadata, ProvenanceData provenanceData)
        {
            return new ProposeAddressSqsRequest
            {
                PersistentLocalId = _persistentLocalIdGenerator.GenerateNextPersistentLocalId(),
                Request = request,
                Metadata = metadata,
                ProvenanceData = provenanceData
            };
        }
    }
}
