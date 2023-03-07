namespace AddressRegistry.Projections.Syndication.BuildingUnit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressLink;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;

    public class AddressBuildingUnitLinkProjections : AtomEntryProjectionHandlerModule<BuildingEvent,
        SyndicationItem<Building>, SyndicationContext>
    {
        private readonly Encoding _encoding;
        private readonly ILogger _logger;

        public AddressBuildingUnitLinkProjections(Encoding encoding, ILogger logger)
        {
            _encoding = encoding;
            _logger = logger;

            When(BuildingEvent.BuildingUnitAddressWasAttached, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitAddressWasDetached, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitBecameComplete, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitBecameIncomplete, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitPersistentLocalIdentifierWasAssigned, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitWasAdded, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitWasAddedToRetiredBuilding, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitWasReaddedByOtherUnitRemoval, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitWasReaddressed, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitWasRemoved, AddSyndicationItemEntry);
            When(BuildingEvent.CommonBuildingUnitWasAdded, AddSyndicationItemEntry);

            When(BuildingEvent.BuildingWasRemoved, RemoveBuilding);
            When(BuildingEvent.BuildingBecameComplete,
                (entry, context, ct) => CompleteBuilding(entry, context, true, ct));
            When(BuildingEvent.BuildingBecameIncomplete,
                (entry, context, ct) => CompleteBuilding(entry, context, false, ct));
            When(BuildingEvent.BuildingWasRetired, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingWasCorrectedToRetired, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingWasNotRealized, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingWasCorrectedToNotRealized, AddSyndicationItemEntry);

            //TODO: Remove after fix building unit readdressing
            When(BuildingEvent.BuildingUnitPositionWasAppointedByAdministrator, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitPositionWasDerivedFromObject, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitPositionWasCorrectedToAppointedByAdministrator, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitPositionWasCorrectedToDerivedFromObject, AddSyndicationItemEntry);
        }

        private async Task RemoveBuilding(AtomEntry<SyndicationItem<Building>> entry, SyndicationContext context,
            CancellationToken ct)
        {
            var addressBuildingUnitLinkExtractItems = GetBuildingUnitItemsByBuilding(entry, context);
            context.AddressBuildingUnitLinkExtract.RemoveRange(addressBuildingUnitLinkExtractItems);
        }

        private async Task CompleteBuilding(AtomEntry<SyndicationItem<Building>> entry, SyndicationContext context,
            bool isComplete, CancellationToken ct)
        {
            ct.ThrowIfCancellationRequested();
            
            var addressBuildingUnitLinkExtractItems = GetBuildingUnitItemsByBuilding(entry, context);
            addressBuildingUnitLinkExtractItems.ForEach(buildingUnitItem =>
                buildingUnitItem.IsBuildingComplete = isComplete);
        }

        private async Task AddSyndicationItemEntry(AtomEntry<SyndicationItem<Building>> entry,
            SyndicationContext context, CancellationToken ct)
        {
            var addressBuildingUnitLinkExtractItems = GetBuildingUnitItemsByBuilding(entry, context);

            //Remove buildingunits which are not present in the feed but present in the DB
            foreach (var buildingUnitAddressMatchLatestItem in addressBuildingUnitLinkExtractItems)
            {
                if (!entry.Content.Object.BuildingUnits
                        .Select(x => Guid.Parse(x.BuildingUnitId))
                        .Contains(buildingUnitAddressMatchLatestItem.BuildingUnitId))
                {
                    buildingUnitAddressMatchLatestItem.IsBuildingUnitRemoved = true;
                }
            }

            foreach (var buildingUnit in entry.Content.Object.BuildingUnits)
            {
                var unitItems = addressBuildingUnitLinkExtractItems
                    .Where(x => x.BuildingUnitId == Guid.Parse(buildingUnit.BuildingUnitId)).ToList();

                unitItems
                    .Where(x => !buildingUnit.Addresses.Select(Guid.Parse).Contains(x.AddressId))
                    .ToList()
                    .ForEach(x => { x.IsAddressLinkRemoved = true; });

                foreach (var addressId in buildingUnit.Addresses.Select(Guid.Parse))
                {
                    var addressItem = unitItems.FirstOrDefault(x => x.AddressId == addressId);
                    if (addressItem == null)
                    {
                        await context.AddressBuildingUnitLinkExtract.AddAsync(
                            CreateAddressBuildingUnitLinkExtractItem(entry, addressId, buildingUnit, context), ct);
                    }
                    else
                    {
                        addressItem.IsBuildingUnitRemoved = false;
                        addressItem.IsAddressLinkRemoved = false;
                        addressItem.IsBuildingUnitComplete = buildingUnit.IsComplete;

                        if (string.Equals(addressItem.BuildingUnitPersistentLocalId,
                                buildingUnit.Identificator.ObjectId))
                        {
                            continue;
                        }

                        addressItem.BuildingUnitPersistentLocalId = buildingUnit.Identificator.ObjectId;
                        UpdateDbaseRecordField(addressItem,
                            record => record.adresobjid.Value = buildingUnit.Identificator.ObjectId);
                    }
                }
            }
        }

        private AddressBuildingUnitLinkExtractItem CreateAddressBuildingUnitLinkExtractItem(
            AtomEntry<SyndicationItem<Building>> entry, Guid addressId, BuildingUnitSyndicationContent buildingUnit,
            SyndicationContext context)
        {
            var address = context.AddressLinkAddresses.Find(addressId);

            var dbaseRecord = CreateDbaseRecord(buildingUnit, address, context);
            return new AddressBuildingUnitLinkExtractItem
            {
                AddressId = addressId,
                BuildingId = Guid.Parse(entry.Content.Object.Id),
                BuildingUnitPersistentLocalId = buildingUnit.Identificator.ObjectId,
                BuildingUnitId = Guid.Parse(buildingUnit.BuildingUnitId),
                DbaseRecord = dbaseRecord, //Add address info
                AddressPersistentLocalId = address?.PersistentLocalId,
                IsAddressLinkRemoved = false,
                IsBuildingUnitComplete = buildingUnit.IsComplete,
                IsBuildingUnitRemoved = false,
                IsBuildingComplete = entry.Content.Object.IsComplete,
            };
        }

        private static List<AddressBuildingUnitLinkExtractItem> GetBuildingUnitItemsByBuilding(
            AtomEntry<SyndicationItem<Building>> entry, SyndicationContext context)
        {
            var objectId = Guid.Parse(entry.Content.Object.Id);
            var addressBuildingUnitLinkExtractItems =
                context
                    .AddressBuildingUnitLinkExtract
                    .Where(x => x.BuildingId == objectId)
                    .AsEnumerable()
                    .Concat(context.AddressBuildingUnitLinkExtract.Local.Where(x =>
                        x.BuildingId == objectId))
                    .ToList();
            return addressBuildingUnitLinkExtractItems;
        }

        private byte[] CreateDbaseRecord(BuildingUnitSyndicationContent buildingUnit,
            AddressLinkSyndicationItem address, SyndicationContext context)
        {
            var record = new AddressLinkDbaseRecord
            {
                objecttype = { Value = "Gebouweenheid" },
                adresobjid =
                {
                    Value = string.IsNullOrEmpty(buildingUnit.Identificator.ObjectId)
                        ? ""
                        : buildingUnit.Identificator.ObjectId
                },
            };

            if (address != null)
            {
                if (!string.IsNullOrEmpty(address.PersistentLocalId))
                {
                    record.adresid.Value = Convert.ToInt32(address.PersistentLocalId);
                }

                record.voladres.Value = CreateCompleteAddress(address, context);
            }

            return record.ToBytes(_encoding);
        }

        private void UpdateDbaseRecordField(AddressBuildingUnitLinkExtractItem item,
            Action<AddressLinkDbaseRecord> update)
        {
            var record = new AddressLinkDbaseRecord();
            record.FromBytes(item.DbaseRecord, _encoding);
            update(record);
            item.DbaseRecord = record.ToBytes(_encoding);
        }

        internal static string CreateCompleteAddress(AddressLinkSyndicationItem address, SyndicationContext context)
        {
            // update streetname, municipality
            var streetName = context.StreetNameLatestItems.AsNoTracking()
                .First(x => x.StreetNameId == address.StreetNameId);
            var municipality = context.MunicipalityLatestItems.AsNoTracking()
                .First(x => x.NisCode == streetName.NisCode);

            string municipalityName;
            string streetNameName;

            switch (municipality.PrimaryLanguage)
            {
                default:
                    municipalityName = municipality.NameDutch;
                    streetNameName = streetName.NameDutch;
                    break;

                case Taal.FR:
                    municipalityName = municipality.NameFrench;
                    streetNameName = streetName.NameFrench;
                    break;

                case Taal.DE:
                    municipalityName = municipality.NameGerman;
                    streetNameName = streetName.NameGerman;
                    break;

                case Taal.EN:
                    municipalityName = municipality.NameEnglish;
                    streetNameName = streetName.NameEnglish;
                    break;
            }

            return
                new VolledigAdres(
                        streetNameName,
                        address.HouseNumber,
                        address.BoxNumber,
                        address.PostalCode,
                        municipalityName,
                        Taal.NL)
                    .GeografischeNaam
                    .Spelling;
        }
    }
}
