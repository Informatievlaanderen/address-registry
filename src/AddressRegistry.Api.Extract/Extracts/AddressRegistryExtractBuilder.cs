namespace AddressRegistry.Api.Extract.Extracts
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Consumer.Read.Municipality;
    using Consumer.Read.StreetName;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Projections.Extract;
    using Projections.Extract.AddressExtract;
    using MunicipalityLanguage = Consumer.Read.Municipality.Projections.MunicipalityLanguage;

    public static class AddressRegistryExtractBuilder
    {
        public static async IAsyncEnumerable<ExtractFile> CreateAddressFilesV2(
            ExtractContext context,
            StreetNameConsumerContext streetNameConsumerContext,
            MunicipalityConsumerContext municipalityConsumerContext)
        {
            var extractItems = context
                .AddressExtractV2
                .AsNoTracking()
                .OrderBy(m => m.AddressPersistentLocalId);

            var feedPosition = await GetFeedPosition(context);
            var extractMetadata = new Dictionary<string, string>
            {
                { ExtractMetadataKeys.LatestEventId, feedPosition.ToString() }
            };

            var cachedMunicipalities = municipalityConsumerContext.MunicipalityLatestItems.AsNoTracking().ToList();
            var cachedStreetNames = streetNameConsumerContext
                .StreetNameLatestItems
                .AsNoTracking()
                .ToList()
                .Select(x => new
                {
                    PersistentLocalId = x.PersistentLocalId,
                    PersistentLocalIdAsString = x.PersistentLocalId.ToString(),
                    x.NameDutch,
                    x.NameFrench,
                    x.NameGerman,
                    x.NameEnglish,
                    x.NisCode
                })
                .ToList();

            byte[] TransformRecord(AddressExtractItemV2 r)
            {
                var item = new AddressDbaseRecordV2();
                item.FromBytes(r.DbaseRecord, DbfFileWriter<AddressDbaseRecordV2>.Encoding);

                // update streetname, municipality
                var streetName = cachedStreetNames.First(x => x.PersistentLocalId == r.StreetNamePersistentLocalId);
                var municipality = cachedMunicipalities.First(x => x.NisCode == streetName.NisCode);

                item.straatnmid.Value = streetName.PersistentLocalIdAsString;
                item.gemeenteid.Value = municipality.NisCode;

                switch (municipality.PrimaryLanguage)
                {
                    case MunicipalityLanguage.French:
                        item.gemeentenm.Value = municipality.NameFrench;
                        item.straatnm.Value = streetName.NameFrench;
                        break;

                    case MunicipalityLanguage.German:
                        item.gemeentenm.Value = municipality.NameGerman;
                        item.straatnm.Value = streetName.NameGerman;
                        break;

                    case MunicipalityLanguage.English:
                        item.gemeentenm.Value = municipality.NameEnglish;
                        item.straatnm.Value = streetName.NameEnglish;
                        break;

                    default:
                        item.gemeentenm.Value = municipality.NameDutch;
                        item.straatnm.Value = streetName.NameDutch;
                        break;
                }

                item.voladres.Value = new VolledigAdres(
                        item.straatnm.Value,
                        item.huisnr.Value,
                        item.busnr.Value,
                        item.postcode.Value,
                        item.gemeentenm.Value,
                        Taal.NL) //irrelevant cause we use spelling
                    .GeografischeNaam
                    .Spelling;

                return item.ToBytes(DbfFileWriter<AddressDbaseRecordV2>.Encoding);
            }

            yield return ExtractBuilder.CreateDbfFile<AddressExtractItemV2, AddressDbaseRecordV2>(
                ExtractFileNames.Address,
                new AddressDbaseSchemaV2(),
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
                extractItems.Select(x => x.ShapeRecordContent!),
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

        private static async Task<long> GetFeedPosition(ExtractContext context)
        {
            var addressProjectionState = context
                .ProjectionStates
                .AsNoTracking()
                .Single(m => m.Name == typeof(AddressExtractProjectionsV2).FullName);
            var extractPosition = addressProjectionState.Position;

            await using var connection = new SqlConnection(context.Database.GetConnectionString());

            var query = $"""
                         SELECT MAX(FeedPosition)
                         FROM [{Schema.Legacy}].[AddressSyndication]
                         WHERE Position = {extractPosition}
                         """;
            await using var getFeedPositionByExtractPosition = new SqlCommand(query, connection);
            await connection.OpenAsync();

            var feedPosition = await getFeedPositionByExtractPosition.ExecuteScalarAsync() as long?;

            if (feedPosition.HasValue)
            {
                return feedPosition.Value;
            }

            // If feed projection lags behind, then we'll make an estimation of what the position will be once caught up.
            query = $"""
                     SELECT MAX(Position), MAX(FeedPosition)
                     FROM [{Schema.Legacy}].[AddressSyndication]
                     """;
            await using var getMaxFeedPositions = new SqlCommand(query, connection);
            var reader = await getMaxFeedPositions.ExecuteReaderAsync();
            await reader.ReadAsync();

            var maxPosition = reader.GetInt64(0);
            var maxFeedPosition = reader.GetInt64(1);

            var diff = extractPosition - maxPosition;
            return maxFeedPosition + diff;
        }
    }
}
