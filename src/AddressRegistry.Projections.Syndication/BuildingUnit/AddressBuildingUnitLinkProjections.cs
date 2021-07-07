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
            When(BuildingEvent.BuildingUnitWasRemoved, AddSyndicationItemEntry);
            When(BuildingEvent.CommonBuildingUnitWasAdded, AddSyndicationItemEntry);

            When(BuildingEvent.BuildingWasRemoved, RemoveBuilding);
            When(BuildingEvent.BuildingWasRetired, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingWasCorrectedToRetired, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingWasNotRealized, AddSyndicationItemEntry);
            When(BuildingEvent.BuildingWasCorrectedToNotRealized, AddSyndicationItemEntry);
        }

        private static async Task RemoveBuilding(AtomEntry<SyndicationItem<Building>> entry, SyndicationContext context, CancellationToken ct)
        {
            var addressBuildingUnitLinkExtractItems =
                context
                    .AddressBuildingUnitLinkExtract
                    .Where(x => x.BuildingId == entry.Content.Object.Id)
                    .AsEnumerable()
                    .Concat(context.AddressBuildingUnitLinkExtract.Local.Where(x => x.BuildingId == entry.Content.Object.Id))
                    .ToList();

            context.AddressBuildingUnitLinkExtract.RemoveRange(addressBuildingUnitLinkExtractItems);
        }

        private async Task AddSyndicationItemEntry(AtomEntry<SyndicationItem<Building>> entry, SyndicationContext context, CancellationToken ct)
        {
            var addressBuildingUnitLinkExtractItems =
                context
                    .AddressBuildingUnitLinkExtract
                    .Where(x => x.BuildingId == entry.Content.Object.Id)
                    .AsEnumerable()
                    .Concat(context.AddressBuildingUnitLinkExtract.Local.Where(x => x.BuildingId == entry.Content.Object.Id))
                    .Distinct()
                    .ToList();

            var itemsToRemove = new List<AddressBuildingUnitLinkExtractItem>();
            foreach (var buildingUnitAddressMatchLatestItem in addressBuildingUnitLinkExtractItems)
            {
                if (!entry.Content.Object.BuildingUnits.Select(x => x.BuildingUnitId).Contains(buildingUnitAddressMatchLatestItem.BuildingUnitId))
                    itemsToRemove.Add(buildingUnitAddressMatchLatestItem);
            }

            context.AddressBuildingUnitLinkExtract.RemoveRange(itemsToRemove);

            foreach (var buildingUnit in entry.Content.Object.BuildingUnits)
            {
                var unitItems = addressBuildingUnitLinkExtractItems.Where(x => x.BuildingUnitId == buildingUnit.BuildingUnitId).ToList();
                var addressItemsToRemove = unitItems.Where(x => !buildingUnit.Addresses.Contains(x.AddressId));
                foreach (var addressId in buildingUnit.Addresses)
                {
                    var addressItem = unitItems.FirstOrDefault(x => x.AddressId == addressId);
                    if (addressItem == null)
                    {
                        await context.AddressBuildingUnitLinkExtract.AddAsync(CreateAddressBuildingUnitLinkExtractItem(entry, addressId, buildingUnit, context), ct);
                    }
                    else
                    {
                        addressItem.BuildingUnitPersistentLocalId = buildingUnit.Identificator.ObjectId;
                        UpdateDbaseRecordField(addressItem, record => record.adresobjid.Value = buildingUnit.Identificator.ObjectId);
                    }
                }

                context.AddressBuildingUnitLinkExtract.RemoveRange(addressItemsToRemove);
            }
        }

        private AddressBuildingUnitLinkExtractItem CreateAddressBuildingUnitLinkExtractItem(AtomEntry<SyndicationItem<Building>> entry, Guid addressId, BuildingUnitSyndicationContent buildingUnit, SyndicationContext context)
        {
            var address = context.AddressLinkAddresses.Find(addressId);

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

        internal static string CreateCompleteAddress(AddressLinkSyndicationItem address, SyndicationContext context)
        {
            // update streetname, municipality
            var streetName = context.StreetNameLatestItems.AsNoTracking().First(x => x.StreetNameId == address.StreetNameId);
            var municipality = context.MunicipalityLatestItems.AsNoTracking().First(x => x.NisCode == streetName.NisCode);

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
