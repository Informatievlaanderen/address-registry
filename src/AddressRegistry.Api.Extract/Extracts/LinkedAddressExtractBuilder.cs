namespace AddressRegistry.Api.Extract.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using Projections.Extract.AddressLinkExtract;
    using Projections.Syndication;
    using Projections.Syndication.Municipality;
    using Projections.Syndication.StreetName;

    public class LinkedAddressExtractBuilder
    {
        private readonly ExtractContext _context;
        private readonly SyndicationContext _syndicationContext;
        private readonly List<MunicipalityLatestItem> _cachedMunicipalities;
        private readonly List<StreetNameLatestItem> _cachedStreetNames;
        private const string BuildingUnitObjectType = "Gebouweenheid";
        private const string ParcelObjectType = "Perceel";

        public LinkedAddressExtractBuilder(ExtractContext context, SyndicationContext syndicationContext)
        {
            _context = context;
            _syndicationContext = syndicationContext;
            _cachedMunicipalities = syndicationContext.MunicipalityLatestItems.AsNoTracking().ToList();
            _cachedStreetNames = syndicationContext.StreetNameLatestItems.AsNoTracking().ToList();
        }

        public ExtractFile CreateLinkedBuildingUnitAddressFiles()
        {
            var extractItems = _context
                .AddressLinkExtract
                .AsNoTracking()
                .Where(m => m.Complete)
                .OrderBy(m => m.PersistentLocalId);

            var buildingUnitsIdsWithRemoved = _syndicationContext.BuildingUnitAddressMatchLatestItems.Where(x => !x.IsRemoved).Select(x => new { x.BuildingUnitPersistentLocalId, x.AddressId });

            AddressLink TransformRecord(AddressLinkExtractItem r)
            {
                var buildingUnitIds = buildingUnitsIdsWithRemoved.Where(x => x.AddressId == r.AddressId).Select(x => x.BuildingUnitPersistentLocalId);

                return CreateAddressLink(buildingUnitIds, r, BuildingUnitObjectType);
            }

            return CreateDbfFile<AddressLinkExtractItem, AddressLinkDbaseRecord>(
                ExtractController.FileNameLinksBuildingUnit,
                new AddressLinkDbaseSchema(),
                extractItems,
                () => _syndicationContext.BuildingUnitAddressMatchLatestItems.Count(x => !x.IsRemoved),
                TransformRecord);
        }

        public ExtractFile CreateLinkedParcelAddressFiles()
        {
            var extractItems = _context
                .AddressLinkExtract
                .AsNoTracking()
                .Where(m => m.Complete)
                .OrderBy(m => m.PersistentLocalId);

            var parcelIdsWithRemoved = _syndicationContext.ParcelAddressMatchLatestItems.Where(x => !x.IsRemoved).Select(x => new { x.ParcelPersistentLocalId, x.AddressId });

            AddressLink TransformRecord(AddressLinkExtractItem r)
            {
                var parcelIds = parcelIdsWithRemoved.Where(x => x.AddressId == r.AddressId).Select(x => x.ParcelPersistentLocalId);

                return CreateAddressLink(parcelIds, r, ParcelObjectType);
            }

            return CreateDbfFile<AddressLinkExtractItem, AddressLinkDbaseRecord>(
                ExtractController.FileNameLinksParcel,
                new AddressLinkDbaseSchema(),
                extractItems,
                () => _syndicationContext.ParcelAddressMatchLatestItems.Count(x => !x.IsRemoved),
                TransformRecord);
        }

        private AddressLink CreateAddressLink(
            IQueryable<string> linkIds,
            AddressLinkExtractItem linkExtractItem,
            string linkType)
        {
            if (!linkIds.Any())
                return new AddressLink();

            var addressLink = new AddressLink();

            // update streetname, municipality
            var streetName = _cachedStreetNames.First(x => x.StreetNameId == linkExtractItem.StreetNameId);
            var municipality = _cachedMunicipalities.First(x => x.NisCode == streetName.NisCode);

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

            var completeAddress = string.IsNullOrEmpty(linkExtractItem.BoxNumber)
                ? $"{streetNameName} {linkExtractItem.HouseNumber}, {linkExtractItem.PostalCode}, {municipalityName}"
                : $"{streetNameName} {linkExtractItem.HouseNumber} bus {linkExtractItem.BoxNumber}, {linkExtractItem.PostalCode}, {municipalityName}";

            Parallel.ForEach(linkIds, linkId =>
            {
                var item = new AddressLinkDbaseRecord();
                item.FromBytes(linkExtractItem.DbaseRecord, DbfFileWriter<AddressLinkDbaseRecord>.Encoding);

                item.objecttype.Value = linkType;
                item.adresobjid.Value = linkId;
                item.voladres.Value = completeAddress;

                addressLink.DbaseRecords.Add(item.ToBytes(DbfFileWriter<AddressLinkDbaseRecord>.Encoding));
            });

            //foreach (var linkId in linkIds)
            //{
            //    var item = new AddressLinkDbaseRecord();
            //    item.FromBytes(linkExtractItem.DbaseRecord, DbfFileWriter<AddressLinkDbaseRecord>.Encoding);

            //    item.objecttype.Value = linkType;
            //    item.adresobjid.Value = linkId;
            //    item.voladres.Value = completeAddress;

            //    addressLink.DbaseRecords.Add(item.ToBytes(DbfFileWriter<AddressLinkDbaseRecord>.Encoding));
            //}

            return addressLink;
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
