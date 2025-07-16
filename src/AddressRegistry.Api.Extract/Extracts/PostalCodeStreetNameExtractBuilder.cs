namespace AddressRegistry.Api.Extract.Extracts
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Be.Vlaanderen.Basisregisters.GrAr.Extracts;
    using Microsoft.Data.SqlClient;
    using Projections.Extract.PostalCodeStreetNameExtract;
    using Projections.Legacy.AddressDetailV2WithParent;

    public class PostalCodeStreetNameExtractBuilder
    {
        private readonly string _connectionString;

        public PostalCodeStreetNameExtractBuilder(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public async Task<ExtractFile> CreateLinkedPostalCodeStreetNameFile(CancellationToken cancellationToken)
        {
            var query = $"""
                         SELECT PostalCode, StreetNamePersistentLocalId
                         FROM [{Schema.Legacy}].[{AddressDetailItemV2WithParentConfiguration.TableName}]
                         WHERE Removed = 0 AND PostalCode IS NOT NULL
                         GROUP BY PostalCode, StreetNamePersistentLocalId
                         ORDER BY PostalCode, StreetNamePersistentLocalId
                         """;
            await using var dbaseRecordConnection = new SqlConnection(_connectionString);
            await using var dbaseRecordsCommand = new SqlCommand(query, dbaseRecordConnection);
            await dbaseRecordConnection.OpenAsync(cancellationToken);

            var reader = await dbaseRecordsCommand.ExecuteReaderAsync(cancellationToken);

            var dbaseRecords = new List<byte[]>();
            if (reader.HasRows)
            {
                while (await reader.ReadAsync(cancellationToken))
                {
                    var record = new PostalCodeStreetNameExtractDbaseRecord
                    {
                        postcode =
                        {
                            Value = reader.GetString(0)
                        },
                        straatnmid =
                        {
                            Value = reader.GetInt32(1).ToString()
                        }
                    };

                    dbaseRecords.Add(record.ToBytes(DbfFileWriter<PostalCodeStreetNameExtractDbaseRecord>.Encoding));
                }
            }

            return ExtractBuilder.CreateDbfFile<PostalCodeStreetNameExtractDbaseRecord>(
                ExtractFileNames.PostalCodeStreetNameLinks,
                new PostalCodeStreetNameExtractDbaseSchema(),
                dbaseRecords,
                () => dbaseRecords.Count);
        }
    }
}
