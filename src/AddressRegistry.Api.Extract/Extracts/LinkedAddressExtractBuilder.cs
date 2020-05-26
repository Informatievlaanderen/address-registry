namespace AddressRegistry.Api.Extract.Extracts
{
    using Projections.Extract.AddressExtract;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Projections.Syndication;
    using Projections.Syndication.AddressLink;

    public class LinkedAddressExtractBuilder
    {
        private readonly SyndicationContext _syndicationContext;

        public LinkedAddressExtractBuilder(SyndicationContext syndicationContext)
        {
            _syndicationContext = syndicationContext;
        }

        public ExtractFile CreateLinkedBuildingUnitAddressFiles()
        {
            var extractItems =
                from extractItem in _syndicationContext.AddressBuildingUnitLinkExtract
                join buildingUnit in _syndicationContext.BuildingUnitAddressMatchLatestItems
                    on extractItem.BuildingUnitId equals buildingUnit.BuildingUnitId
                where extractItem.AddressComplete && !buildingUnit.IsRemoved && buildingUnit.IsComplete && buildingUnit.IsBuildingComplete
                select extractItem.DbaseRecord;

            return ExtractBuilder.CreateDbfFile<AddressLinkDbaseRecord>(
                ExtractController.FileNameLinksBuildingUnit,
                new AddressDbaseSchema(),
                extractItems,
                extractItems.Count);
        }

        public ExtractFile CreateLinkedParcelAddressFiles()
        {
            var extractItems =
                from extractItem in _syndicationContext.AddressParcelLinkExtract
                join parcel in _syndicationContext.ParcelAddressMatchLatestItems
                    on extractItem.ParcelId equals parcel.ParcelId
                where extractItem.AddressComplete && !parcel.IsRemoved
                select extractItem.DbaseRecord;

            return ExtractBuilder.CreateDbfFile<AddressLinkDbaseRecord>(
                ExtractController.FileNameLinksParcel,
                new AddressDbaseSchema(),
                extractItems,
                extractItems.Count);
        }
    }
}
