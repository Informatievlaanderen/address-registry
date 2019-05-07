namespace AddressRegistry.Api.CrabImport.CrabImport
{
    using AddressRegistry.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Api;
    using CsvHelper.Configuration;
    using Dapper;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("addressmatch")]
    [ApiExplorerSettings(GroupName = "AddressMatch")]
    public class AddressMatchController : ApiController
    {
        private readonly string _filePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", DateTime.Today.ToString("yyyyMMdd"));
        private const string RrStreetNamesFileName = "RRStreetNames.csv";
        private const string KadStreetNamesFileName = "KadStreetNames.csv";
        private const string RrAddressesFileName = "RRAddresses.csv";
        private readonly Configuration _csvConfiguration = new Configuration { Encoding = Encoding.UTF8, Delimiter = ";" };

        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Post(
            [FromServices] ILogger<AddressMatchController> logger,
            [FromServices] IConfiguration configuration,
            [FromForm] IFormFile file,
            CancellationToken cancellationToken = default)
        {
            await SaveFile(file, cancellationToken);

            ZipFile.ExtractToDirectory(Path.Combine(_filePath, file.FileName), _filePath, true);

            var connectionString = configuration.GetConnectionString("AddressMatch");
            await CreateTmpTables(connectionString);

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    ImportRrStreetNames(connection, transaction);
                    ImportKadStreetNames(connection, transaction);
                    ImportRrAddresses(connection, transaction);
                    transaction.Commit();
                }
                catch
                {
                    transaction.Rollback();
                }
            }

            //Switch tables

            return Ok();
        }

        private void ImportRrStreetNames(SqlConnection connection, SqlTransaction transaction)
        {
            var destinationTableName = $"{Schema.Legacy}.tmpRRStreetNames";
            var dataTable = new DataTable();
            dataTable.Columns.Add("StreetNameId", typeof(int));
            dataTable.Columns.Add("StreetName", typeof(string));
            dataTable.Columns.Add("StreetCode", typeof(string));
            dataTable.Columns.Add("PostalCode", typeof(string));

            ExtractAndTransform(RrStreetNamesFileName, dataTable);
            Load(connection, transaction, destinationTableName, dataTable);
        }

        private void ImportKadStreetNames(SqlConnection connection, SqlTransaction transaction)
        {
            var destinationTableName = $"{Schema.Legacy}.tmpKadStreetNames";
            var dataTable = new DataTable();
            dataTable.Columns.Add("StreetNameId", typeof(int));
            dataTable.Columns.Add("KadStreetNameCode", typeof(string));
            dataTable.Columns.Add("NisCode", typeof(string));

            ExtractAndTransform(KadStreetNamesFileName, dataTable);
            Load(connection, transaction, destinationTableName, dataTable);
        }

        private void ImportRrAddresses(SqlConnection connection, SqlTransaction transaction)
        {
            var destinationTableName = $"{Schema.Legacy}.tmpRRAddresses";
            var dataTable = new DataTable();
            dataTable.Columns.Add("AddressId", typeof(int));
            dataTable.Columns.Add("AddressType", typeof(string));
            dataTable.Columns.Add("RRHouseNumber", typeof(string));
            dataTable.Columns.Add("RRIndex", typeof(string));
            dataTable.Columns.Add("StreetCode", typeof(string));
            dataTable.Columns.Add("PostalCode", typeof(string));

            ExtractAndTransform(RrAddressesFileName, dataTable);
            Load(connection, transaction, destinationTableName, dataTable);
        }

        private static void Load(
            SqlConnection connection,
            SqlTransaction transaction,
            string destinationTableName,
            DataTable dataTable)
        {
            using (var bulkCopy = new SqlBulkCopy(connection, SqlBulkCopyOptions.Default, transaction))
            {
                bulkCopy.BulkCopyTimeout = 600;
                bulkCopy.DestinationTableName = destinationTableName;
                bulkCopy.WriteToServer(dataTable);
            }
        }

        private void ExtractAndTransform(string fileName, DataTable dataTable)
        {
            using (var stream = new StreamReader(Path.Combine(_filePath, fileName)))
            using (var reader = new CsvHelper.CsvReader(stream, _csvConfiguration))
            {
                reader.Read();
                reader.ReadHeader();

                while (reader.Read())
                {
                    var row = dataTable.NewRow();
                    foreach (DataColumn column in dataTable.Columns)
                    {
                        row[column.ColumnName] = reader.GetField(column.DataType, column.ColumnName);
                    }

                    dataTable.Rows.Add(row);
                }
            }
        }

        private async Task SaveFile(IFormFile file, CancellationToken cancellationToken)
        {
            var path = Path.Combine(_filePath, file.FileName);
            Directory.CreateDirectory(_filePath);

            using (var fileStream = new FileStream(path, FileMode.Create))
                await file.CopyToAsync(fileStream, cancellationToken);
        }

        private async Task CreateTmpTables(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                await connection.ExecuteAsync($@"
                    IF NOT EXISTS (
                    SELECT  schema_name
                    FROM    information_schema.schemata
                    WHERE   schema_name = '{Schema.Legacy}')

                    BEGIN
                    EXEC sp_executesql N'CREATE SCHEMA {Schema.Legacy}'
                    END");

                await connection.ExecuteAsync($@"
                    DROP TABLE IF EXISTS {Schema.Legacy}.tmpRRStreetNames;
                    DROP TABLE IF EXISTS {Schema.Legacy}.tmpKadStreetNames;
                    DROP TABLE IF EXISTS {Schema.Legacy}.tmpRRAddresses");

                await connection.ExecuteAsync($@"
                    CREATE TABLE {Schema.Legacy}.tmpRRStreetNames(
                        StreetNameId [int] NOT NULL,
                        StreetName [nvarchar](max) NULL,
                        StreetCode [nvarchar](4) NULL,
                        PostalCode [nvarchar](4) NULL)
                    ");

                await connection.ExecuteAsync($@"
                    CREATE TABLE {Schema.Legacy}.tmpKadStreetNames(
                        StreetNameId [int] NOT NULL,
                        KadStreetNameCode [nvarchar](5) NULL,
                        NisCode [nvarchar](5) NULL)
                    ");

                await connection.ExecuteAsync($@"
                    CREATE TABLE {Schema.Legacy}.tmpRRAddresses(
                        AddressId [int] NOT NULL,
                        AddressType [nvarchar](1) NOT NULL,
                        RRHouseNumber [nvarchar](11) NULL,
                        RRIndex [nvarchar](4) NULL,
                        StreetCode [nvarchar](4) NULL,
                        PostalCode [nvarchar](4) NULL)
                    ");
            }
        }
    }
}
