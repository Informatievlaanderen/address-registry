namespace AddressRegistry.Api.BackOffice.Abstractions.SqsRequests
{
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;

    public sealed class ProposeAddressesForMunicipalityMergerSqsRequest : SqsRequest
    {
        public string NisCode { get; set; }

        public List<ProposeAddressesForMunicipalityMergerSqsRequestItem> Addresses { get; set; }

        public ProposeAddressesForMunicipalityMergerSqsRequest()
        {
        }

        public ProposeAddressesForMunicipalityMergerSqsRequest(
            string nisCode,
            List<ProposeAddressesForMunicipalityMergerSqsRequestItem> addresses,
            ProvenanceData provenanceData)
        {
            NisCode = nisCode;
            Addresses = addresses;
            ProvenanceData = provenanceData;
        }
    }

    public sealed class ProposeAddressesForMunicipalityMergerSqsRequestItem
    {
        public int StreetNamePersistentLocalId { get; set; }
        public string PostalCode { get; set; }
        public int NewAddressPersistentLocalId { get; set; }
        public string HouseNumber { get; set; }
        public string? BoxNumber { get; set; }

        public int MergedAddressPersistentLocalId { get; set; }

        public ProposeAddressesForMunicipalityMergerSqsRequestItem(
            int streetNamePersistentLocalId,
            string postalCode, int newAddressPersistentLocalId,
            string houseNumber,
            string? boxNumber,
            int mergedAddressPersistentLocalId)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            PostalCode = postalCode;
            NewAddressPersistentLocalId = newAddressPersistentLocalId;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
            MergedAddressPersistentLocalId = mergedAddressPersistentLocalId;
        }
    }
}
