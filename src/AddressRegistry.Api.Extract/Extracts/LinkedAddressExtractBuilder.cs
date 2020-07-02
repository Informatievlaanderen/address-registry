namespace AddressRegistry.Api.Extract.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Microsoft.Data.SqlClient;
    using Projections.Syndication;
    using Projections.Syndication.AddressLink;
    using Projections.Syndication.Parcel;

    public class LinkedAddressExtractBuilder
    {
        private readonly SyndicationContext _syndicationContext;
        private readonly string _connectionString;

        public LinkedAddressExtractBuilder(SyndicationContext syndicationContext, string connectionString)
        {
            _syndicationContext = syndicationContext ?? throw new ArgumentNullException(nameof(syndicationContext));
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public ExtractFile CreateLinkedBuildingUnitAddressFiles()
        {
            var extractItems =
                from extractItem in _syndicationContext.AddressBuildingUnitLinkExtract
                join buildingUnit in _syndicationContext.BuildingUnitAddressMatchLatestItems
                    on new { extractItem.AddressId, extractItem.BuildingUnitId } equals new { buildingUnit.AddressId, buildingUnit.BuildingUnitId }
                where extractItem.AddressComplete && !buildingUnit.IsRemoved && buildingUnit.IsComplete && buildingUnit.IsBuildingComplete
                select extractItem.DbaseRecord;

            return ExtractBuilder.CreateDbfFile<AddressLinkDbaseRecord>(
                ExtractFileNames.BuildingUnitLinks,
                new AddressLinkDbaseSchema(),
                extractItems,
                extractItems.Count);
        }

        public async Task<ExtractFile> CreateLinkedParcelAddressFiles(CancellationToken cancellationToken)
        {
            string BuildCommandText(string select)
            {
                return $"SELECT {select} FROM [{Schema.Syndication}].[{AddressParcelLinkExtractItemConfiguration.TableName}] [apl] " +
                    $"INNER JOIN [{Schema.Syndication}].[{ParcelAddressLatestItemConfiguration.TableName}] [pal] on [pal].ParcelId = [apl].ParcelId and [pal].AddressId = [apl].AddressId " +
                    "WHERE [apl].AddressComplete = 1 AND [pal].IsRemoved = 0";
            }

            IEnumerable<byte[]> GetDbaseRecordBytes()
            {
                using (var dbaseRecordConnection = new SqlConnection(_connectionString))
                using (var dbaseRecordsCommand = new SqlCommand(BuildCommandText("DbaseRecord"), dbaseRecordConnection))
                {
                    dbaseRecordConnection.Open();

                    var reader = dbaseRecordsCommand.ExecuteReader(CommandBehavior.SequentialAccess);

                    if (reader.HasRows)
                        while (reader.Read())
                            yield return reader.GetSqlBytes(0).Value;
                }
            }

            int count;

            using (var countConnection = new SqlConnection(_connectionString))
            {
                await countConnection.OpenAsync(cancellationToken);

                using (var countCommand = new SqlCommand(BuildCommandText("COUNT(1)"), countConnection))
                    count = (int)await countCommand.ExecuteScalarAsync(cancellationToken);
            }

            return ExtractBuilder.CreateDbfFile<AddressLinkDbaseRecord>(
                ExtractFileNames.ParcelLinks,
                new AddressLinkDbaseSchema(),
                GetDbaseRecordBytes(),
                () => count);
        }
    }
}
