namespace AddressRegistry.Api.Extract.Extracts
{
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract.AddressExtract;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Projections.Syndication;
    using Projections.Syndication.AddressLink;

    public class LinkedAddressExtractBuilder2
    {
        private readonly SyndicationContext _syndicationContext;

        public LinkedAddressExtractBuilder2(SyndicationContext syndicationContext)
        {
            _syndicationContext = syndicationContext;
        }

        public ExtractFile CreateLinkedBuildingUnitAddressFiles()
        {
            var extractItems =
                from extractItem in _syndicationContext.AddressBuildingUnitLinkExtract
                join buildingUnit in _syndicationContext.BuildingUnitAddressMatchLatestItems
                    on extractItem.BuildingUnitId equals buildingUnit.BuildingUnitId
                where !buildingUnit.IsRemoved && buildingUnit.IsComplete && buildingUnit.IsBuildingComplete
                select extractItem.DbaseRecord;

            return ExtractBuilder.CreateDbfFile<AddressLinkDbaseRecord>(
                ExtractController.FileNameLinksBuildingUnit,
                new AddressDbaseSchema(),
                extractItems,
                extractItems.Count);
        }
    }
}
