namespace AddressRegistry.Api.Extract.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using Projections.Extract.AddressLinkExtract;
    using Projections.Syndication;

    public static class LinkedAddressExtractBuilder
    {
        private const string BuildingUnitObjectType = "Gebouweenheid";
        private const string ParcelObjectType = "Perceel";

        public static IEnumerable<ExtractFile> CreateLinkedAddressFiles(ExtractContext context, SyndicationContext syndicationContext)
        {
            var extractItems = context
                .AddressLinkExtract
                .AsNoTracking()
                .Where(m => m.Complete)
                .OrderBy(m => m.PersistentLocalId);

            var cachedMunicipalities = syndicationContext.MunicipalityLatestItems.AsNoTracking().ToList();
            var cachedStreetNames = syndicationContext.StreetNameLatestItems.AsNoTracking().ToList();

            AddressLink TransformRecord(AddressLinkExtractItem r)
            {
                var parcels = syndicationContext.ParcelAddressMatchLatestItems.Where(x => x.AddressId == r.AddressId);
                var buildingUnits = syndicationContext.BuildingUnitAddressMatchLatestItems.Where(x => x.AddressId == r.AddressId);

                if (!parcels.Any() && !buildingUnits.Any())
                    return new AddressLink();

                var addressLink = new AddressLink();

                // update streetname, municipality
                var streetName = cachedStreetNames.First(x => x.StreetNameId == r.StreetNameId);
                var municipality = cachedMunicipalities.First(x => x.NisCode == streetName.NisCode);

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

                var completeAddress = string.IsNullOrEmpty(r.BoxNumber)
                    ? $"{streetNameName} {r.HouseNumber}, {r.PostalCode}, {municipalityName}"
                    : $"{streetNameName} {r.HouseNumber} bus {r.BoxNumber}, {r.PostalCode}, {municipalityName}";

                foreach (var parcel in parcels)
                {
                    var item = new AddressLinkDbaseRecord();
                    item.FromBytes(r.DbaseRecord, DbfFileWriter<AddressLinkDbaseRecord>.Encoding);

                    item.objecttype.Value = ParcelObjectType;
                    item.adresobjid.Value = parcel.ParcelPersistentLocalId;
                    item.voladres.Value = completeAddress;

                    addressLink.DbaseRecords.Add(item.ToBytes(DbfFileWriter<AddressLinkDbaseRecord>.Encoding));
                }

                foreach (var buildingUnit in buildingUnits)
                {
                    var item = new AddressLinkDbaseRecord();
                    item.FromBytes(r.DbaseRecord, DbfFileWriter<AddressLinkDbaseRecord>.Encoding);

                    item.objecttype.Value = BuildingUnitObjectType;
                    item.adresobjid.Value = buildingUnit.BuildingUnitPersistentLocalId;
                    item.voladres.Value = completeAddress;

                    addressLink.DbaseRecords.Add(item.ToBytes(DbfFileWriter<AddressLinkDbaseRecord>.Encoding));
                }

                return addressLink;
            }

            yield return CreateDbfFile<AddressLinkExtractItem, AddressLinkDbaseRecord>(
                ExtractController.ZipNameLinks,
                new AddressLinkDbaseSchema(),
                extractItems,
                () => (syndicationContext.BuildingUnitAddressMatchLatestItems.Count() + syndicationContext.ParcelAddressMatchLatestItems.Count()),
                TransformRecord);
        }

        public static ExtractFile CreateDbfFile<T, TDbaseRecord>(
            string fileName,
            DbaseSchema schema,
            IEnumerable<T> records,
            Func<int> getRecordCount,
            Func<T, AddressLink> buildRecordFunc) where TDbaseRecord : DbaseRecord, new()
            => new ExtractFile(
                new DbfFileName(fileName),
                (stream, token) =>
                {
                    var dbfFile = new DbfFileWriter<TDbaseRecord>(new DbaseFileHeader(DateTime.Now, DbaseCodePage.Western_European_ANSI, new DbaseRecordCount(getRecordCount()), schema), stream);

                    foreach (var record in records)
                    {
                        if (token.IsCancellationRequested)
                            return;

                        foreach (var dbaseRecord in buildRecordFunc(record).DbaseRecords)
                            dbfFile.WriteBytesAs<TDbaseRecord>(dbaseRecord);
                    }

                    dbfFile.WriteEndOfFile();
                });

        public class AddressLink
        {
            public List<byte[]> DbaseRecords { get; } = new List<byte[]>();
        }
    }
}
