namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ProposeAddressesForMunicipalityMergerSqsRequest : SqsRequest
    {
        public int StreetNamePersistentLocalId { get; set; }
        public List<ProposeAddressesForMunicipalityMergerSqsRequestItem> Addresses { get; set; }

        public ProposeAddressesForMunicipalityMergerSqsRequest() { }

        public ProposeAddressesForMunicipalityMergerSqsRequest(
            int streetNamePersistentLocalId,
            List<ProposeAddressesForMunicipalityMergerSqsRequestItem> addresses,
            ProvenanceData provenanceData)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            Addresses = addresses;
            ProvenanceData = provenanceData;
        }
    }

    public sealed class ProposeAddressesForMunicipalityMergerSqsRequestItem
    {
        public string PostalCode { get; set; }
        public int AddressPersistentLocalId { get; set; }
        public string HouseNumber { get; set; }
        public string? BoxNumber { get; set; }

        public int MergedStreetNamePersistentLocalId { get; set; }
        public int MergedAddressPersistentLocalId { get; set; }

        public ProposeAddressesForMunicipalityMergerSqsRequestItem()
        { }

        public ProposeAddressesForMunicipalityMergerSqsRequestItem(
            string postalCode,
            int addressPersistentLocalId,
            string houseNumber,
            string? boxNumber,
            int mergedStreetNamePersistentLocalId,
            int mergedAddressPersistentLocalId)
        {
            PostalCode = postalCode;
            AddressPersistentLocalId = addressPersistentLocalId;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            MergedStreetNamePersistentLocalId = mergedStreetNamePersistentLocalId;
            MergedAddressPersistentLocalId = mergedAddressPersistentLocalId;
        }
    }
}
