namespace AddressRegistry.Api.Extract.Extracts
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using Projections.Extract.AddressExtract;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Projections.Syndication;

    public class AddressRegistryExtractBuilder
    {
        public static IEnumerable<ExtractFile> CreateAddressFiles(ExtractContext context, SyndicationContext syndicationContext)
        {
            var extractItems = context
                .AddressExtract
                .AsNoTracking()
                .Where(m => m.Complete)
                .OrderBy(m => m.AddressPersistentLocalId);

            var addressProjectionState = context
                .ProjectionStates
                .AsNoTracking()
                .Single(m => m.Name == typeof(AddressExtractProjection).FullName);
            var extractMetadata = new Dictionary<string,string>
            {
                { ExtractMetadataKeys.LatestEventId, addressProjectionState.Position.ToString()}
            };

            var cachedMunicipalities = syndicationContext.MunicipalityLatestItems.AsNoTracking().ToList();
            var cachedStreetNames = syndicationContext.StreetNameLatestItems.AsNoTracking().ToList();

            byte[] TransformRecord(AddressExtractItem r)
            {
                var item = new AddressDbaseRecord();
                item.FromBytes(r.DbaseRecord, DbfFileWriter<AddressDbaseRecord>.Encoding);

                // update streetname, municipality
                var streetName = cachedStreetNames.First(x => x.StreetNameId == r.StreetNameId);
                var municipality = cachedMunicipalities.First(x => x.NisCode == streetName.NisCode);

                item.straatnmid.Value = streetName.PersistentLocalId;

                switch (municipality.PrimaryLanguage)
                {
                    default:
                        item.gemeentenm.Value = municipality.NameDutch;
                        item.straatnm.Value = streetName.NameDutch;
                        break;

                    case Taal.FR:
                        item.gemeentenm.Value = municipality.NameFrench;
                        item.straatnm.Value = streetName.NameFrench;
                        break;

                    case Taal.DE:
                        item.gemeentenm.Value = municipality.NameGerman;
                        item.straatnm.Value = streetName.NameGerman;
                        break;

                    case Taal.EN:
                        item.gemeentenm.Value = municipality.NameEnglish;
                        item.straatnm.Value = streetName.NameEnglish;
                        break;
                }

                return item.ToBytes(DbfFileWriter<AddressDbaseRecord>.Encoding);
            }

            yield return ExtractBuilder.CreateDbfFile<AddressExtractItem, AddressDbaseRecord>(
                ExtractFileNames.Address,
                new AddressDbaseSchema(),
                extractItems,
                extractItems.Count,
                TransformRecord);

            yield return ExtractBuilder.CreateMetadataDbfFile(
                ExtractFileNames.Address,
                extractMetadata);

            var boundingBox = new BoundingBox3D(
                extractItems.Where(x => x.MinimumX > 0).Min(record => record.MinimumX),
                extractItems.Where(x => x.MinimumY > 0).Min(record => record.MinimumY),
                extractItems.Where(x => x.MaximumX > 0).Max(record => record.MaximumX),
                extractItems.Where(x => x.MaximumY > 0).Max(record => record.MaximumY),
                0,
                0,
                double.NegativeInfinity,
                double.PositiveInfinity);

            yield return ExtractBuilder.CreateShapeFile<PointShapeContent>(
                ExtractFileNames.Address,
                ShapeType.Point,
                extractItems.Select(x => x.ShapeRecordContent),
                ShapeContent.Read,
                extractItems.Select(x => x.ShapeRecordContentLength),
                boundingBox);

            yield return ExtractBuilder.CreateShapeIndexFile(
                ExtractFileNames.Address,
                ShapeType.Point,
                extractItems.Select(x => x.ShapeRecordContentLength),
                extractItems.Count,
                boundingBox);

            yield return ExtractBuilder.CreateProjectedCoordinateSystemFile(
                ExtractFileNames.Address,
                ProjectedCoordinateSystem.Belge_Lambert_1972);
        }
        public static IEnumerable<ExtractFile> CreateAddressFilesV2(ExtractContext context, SyndicationContext syndicationContext)
        {
            var extractItems = context
                .AddressExtractV2
                .AsNoTracking()
                .Where(m => m.Complete)
                .OrderBy(m => m.AddressPersistentLocalId);

            var addressProjectionState = context
                .ProjectionStates
                .AsNoTracking()
                .Single(m => m.Name == typeof(AddressExtractProjectionsV2).FullName);

            var extractMetadata = new Dictionary<string, string>
            {
                { ExtractMetadataKeys.LatestEventId, addressProjectionState.Position.ToString()}
            };

            var cachedMunicipalities = syndicationContext.MunicipalityLatestItems.AsNoTracking().ToList();
            var cachedStreetNames = syndicationContext.StreetNameLatestItems.AsNoTracking().ToList();

            byte[] TransformRecord(AddressExtractItemV2 r)
            {
                var item = new AddressDbaseRecord();
                item.FromBytes(r.DbaseRecord, DbfFileWriter<AddressDbaseRecord>.Encoding);

                // update streetname, municipality
                var streetName = cachedStreetNames.First(x => x.PersistentLocalId == r.StreetNamePersistentLocalId.ToString());
                var municipality = cachedMunicipalities.First(x => x.NisCode == streetName.NisCode);

                item.straatnmid.Value = streetName.PersistentLocalId;

                switch (municipality.PrimaryLanguage)
                {
                    default:
                        item.gemeentenm.Value = municipality.NameDutch;
                        item.straatnm.Value = streetName.NameDutch;
                        break;

                    case Taal.FR:
                        item.gemeentenm.Value = municipality.NameFrench;
                        item.straatnm.Value = streetName.NameFrench;
                        break;

                    case Taal.DE:
                        item.gemeentenm.Value = municipality.NameGerman;
                        item.straatnm.Value = streetName.NameGerman;
                        break;

                    case Taal.EN:
                        item.gemeentenm.Value = municipality.NameEnglish;
                        item.straatnm.Value = streetName.NameEnglish;
                        break;
                }

                return item.ToBytes(DbfFileWriter<AddressDbaseRecord>.Encoding);
            }

            yield return ExtractBuilder.CreateDbfFile<AddressExtractItemV2, AddressDbaseRecord>(
                ExtractFileNames.Address,
                new AddressDbaseSchema(),
                extractItems,
                extractItems.Count,
                TransformRecord);

            yield return ExtractBuilder.CreateMetadataDbfFile(
                ExtractFileNames.Address,
                extractMetadata);

            var boundingBox = new BoundingBox3D(
                extractItems.Where(x => x.MinimumX > 0).Min(record => record.MinimumX),
                extractItems.Where(x => x.MinimumY > 0).Min(record => record.MinimumY),
                extractItems.Where(x => x.MaximumX > 0).Max(record => record.MaximumX),
                extractItems.Where(x => x.MaximumY > 0).Max(record => record.MaximumY),
                0,
                0,
                double.NegativeInfinity,
                double.PositiveInfinity);

            yield return ExtractBuilder.CreateShapeFile<PointShapeContent>(
                ExtractFileNames.Address,
                ShapeType.Point,
                extractItems.Select(x => x.ShapeRecordContent),
                ShapeContent.Read,
                extractItems.Select(x => x.ShapeRecordContentLength),
                boundingBox);

            yield return ExtractBuilder.CreateShapeIndexFile(
                ExtractFileNames.Address,
                ShapeType.Point,
                extractItems.Select(x => x.ShapeRecordContentLength),
                extractItems.Count,
                boundingBox);

            yield return ExtractBuilder.CreateProjectedCoordinateSystemFile(
                ExtractFileNames.Address,
                ProjectedCoordinateSystem.Belge_Lambert_1972);
        }
    }
}
