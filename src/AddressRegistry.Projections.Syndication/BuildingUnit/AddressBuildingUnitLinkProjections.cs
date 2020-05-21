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
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;

    public class AddressBuildingUnitLinkProjections : AtomEntryProjectionHandlerModule<BuildingEvent, SyndicationItem<Building>, SyndicationContext>
    {
        private readonly Encoding _encoding;

        public AddressBuildingUnitLinkProjections(Encoding encoding)
        {
            _encoding = encoding;

            When(BuildingEvent.BuildingUnitAddressWasAttached, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitAddressWasDetached, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitBecameComplete, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitBecameIncomplete, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitPersistentLocalIdentifierWasAssigned, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitWasAdded, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitWasAddedToRetiredBuilding, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitWasReaddedByOtherUnitRemoval, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitWasReaddressed, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingUnitWasRemoved, RemoveBuildingUnit);
            When(BuildingEvent.CommonBuildingUnitWasAdded, AddSyndicationItemEntry);
        }

        private static async Task RemoveBuildingUnit(AtomEntry<SyndicationItem<Building>> entry, SyndicationContext context, CancellationToken ct)
        {
            var addressBuildingUnitLinkExtractItems =
                context
                    .AddressBulidingUnitLinkExtract
                    .Where(x => x.BuildingId == entry.Content.Object.Id)
                    .AsEnumerable()
                    .Concat(context.AddressBulidingUnitLinkExtract.Local.Where(x => x.BuildingId == entry.Content.Object.Id))
                    .ToList();

            context.AddressBulidingUnitLinkExtract.RemoveRange(addressBuildingUnitLinkExtractItems);
        }

        private async Task AddSyndicationItemEntry(AtomEntry<SyndicationItem<Building>> entry, SyndicationContext context, CancellationToken ct)
        {
            var addressBuildingUnitLinkExtractItems =
                context
                    .AddressBulidingUnitLinkExtract
                    .Where(x => x.BuildingId == entry.Content.Object.Id)
                    .AsEnumerable()
                    .Concat(context.AddressBulidingUnitLinkExtract.Local.Where(x => x.BuildingId == entry.Content.Object.Id))
                    .Distinct()
                    .ToList();

            var itemsToRemove = new List<AddressBuildingUnitLinkExtractItem>();
            foreach (var buildingUnitAddressMatchLatestItem in addressBuildingUnitLinkExtractItems)
            {
                if (!entry.Content.Object.BuildingUnits.Select(x => x.BuildingUnitId).Contains(buildingUnitAddressMatchLatestItem.BuildingUnitId))
                    itemsToRemove.Add(buildingUnitAddressMatchLatestItem);
            }

            context.AddressBulidingUnitLinkExtract.RemoveRange(itemsToRemove);

            foreach (var buildingUnit in entry.Content.Object.BuildingUnits)
            {
                var unitItems = addressBuildingUnitLinkExtractItems.Where(x => x.BuildingUnitId == buildingUnit.BuildingUnitId).ToList();
                var addressItemsToRemove = unitItems.Where(x => !buildingUnit.Addresses.Contains(x.AddressId));
                foreach (var addressId in buildingUnit.Addresses)
                {
                    var addressItem = unitItems.FirstOrDefault(x => x.AddressId == addressId);
                    if (addressItem == null)
                    {
                        await context.AddressBulidingUnitLinkExtract.AddAsync(await
                            CreateAddressBuildingUnitLinkExtractItem(entry, addressId, buildingUnit, context), ct);
                    }
                    else
                    {
                        addressItem.BuildingUnitPersistentLocalId = buildingUnit.Identificator.ObjectId;
                        UpdateDbaseRecordField(addressItem, record => record.adresobjid.Value = buildingUnit.Identificator.ObjectId);
                    }
                }

                context.AddressBulidingUnitLinkExtract.RemoveRange(addressItemsToRemove);
            }
        }

        private async Task<AddressBuildingUnitLinkExtractItem> CreateAddressBuildingUnitLinkExtractItem(AtomEntry<SyndicationItem<Building>> entry, Guid addressId, BuildingUnitSyndicationContent buildingUnit, SyndicationContext context)
        {
            var address = await context.AddressLinkAddresses.FindAsync(addressId);

            var dbaseRecord = CreateDbaseRecord(buildingUnit, address, context);
            return new AddressBuildingUnitLinkExtractItem
            {
                AddressId = addressId,
                BuildingId = entry.Content.Object.Id,
                BuildingUnitPersistentLocalId = buildingUnit.Identificator.ObjectId,
                BuildingUnitId = buildingUnit.BuildingUnitId,
                DbaseRecord = dbaseRecord, //Add address info
                AddressComplete = address?.IsComplete ?? false,
                AddressPersistentLocalId = address?.PersistentLocalId,
            };
        }

        private byte[] CreateDbaseRecord(BuildingUnitSyndicationContent buildingUnit, AddressLinkSyndicationItem address, SyndicationContext context)
        {
            var record = new AddressLinkDbaseRecord
            {
                objecttype = { Value = "Gebouweenheid" },
                adresobjid = { Value = string.IsNullOrEmpty(buildingUnit.Identificator.ObjectId) ? "" : buildingUnit.Identificator.ObjectId },
            };

            if (address != null)
            {
                if (!string.IsNullOrEmpty(address.PersistentLocalId))
                    record.adresid.Value = Convert.ToInt32(address.PersistentLocalId);

                record.voladres.Value = CreateCompleteAddress(address, context);
            }

            return record.ToBytes(_encoding);
        }

        private void UpdateDbaseRecordField(AddressBuildingUnitLinkExtractItem item, Action<AddressLinkDbaseRecord> update)
        {
            var record = new AddressLinkDbaseRecord();
            record.FromBytes(item.DbaseRecord, _encoding);
            update(record);
            item.DbaseRecord = record.ToBytes(_encoding);
        }

        private static string CreateCompleteAddress(AddressLinkSyndicationItem address, SyndicationContext context)
        {
            // update streetname, municipality
            var streetName = context.StreetNameLatestItems.First(x => x.StreetNameId == address.StreetNameId);
            var municipality = context.MunicipalityLatestItems.First(x => x.NisCode == streetName.NisCode);

            var municipalityName = string.Empty;
            var streetNameName = string.Empty;

            switch (municipality.PrimaryLanguage)
            {
                case null:
                case Taal.NL:
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

            return string.IsNullOrEmpty(address.BoxNumber)
                ? $"{streetNameName} {address.HouseNumber}, {address.PostalCode}, {municipalityName}"
                : $"{streetNameName} {address.HouseNumber} bus {address.BoxNumber}, {address.PostalCode}, {municipalityName}";
        }
    }
}
