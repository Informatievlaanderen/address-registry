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
    using System.Globalization;
    using Microsoft.Data.SqlClient;
    using System.IO;
    using System.IO.Compression;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using Projections.Legacy.AddressMatch;

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
        private readonly CsvConfiguration _csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture) { Encoding = Encoding.UTF8, Delimiter = ";" };

        [HttpPost, DisableRequestSizeLimit]
        public async Task<IActionResult> Post(
            [FromServices] ILogger<AddressMatchController> logger,
            [FromServices] IConfiguration configuration,
            [FromForm] IFormFile file,
            CancellationToken cancellationToken = default)
        {
            await SaveFile(file, cancellationToken);

            ZipFile.ExtractToDirectory(Path.Combine(_filePath, file.FileName), _filePath, true);

            var connectionString = configuration.GetConnectionString("LegacyProjectionsAdmin");

            using (var connection = new SqlConnection(connectionString))
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                try
                {
                    await connection.ExecuteAsync($@"
                        TRUNCATE TABLE [{Schema.Legacy}].[{RRStreetName.TableName}];
                        TRUNCATE TABLE [{Schema.Legacy}].[{KadStreetName.TableName}];
                        TRUNCATE TABLE [{Schema.Legacy}].[{RRAddress.TableName}];",
                        transaction: transaction);

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

            return Ok();
        }

        private void ImportRrStreetNames(SqlConnection connection, SqlTransaction transaction)
        {
            var destinationTableName = $"{Schema.Legacy}.{RRStreetName.TableName}";
            var dataTable = new DataTable();
            dataTable.Columns.Add(nameof(RRStreetName.StreetNameId), typeof(int));
            dataTable.Columns.Add(nameof(RRStreetName.StreetName), typeof(string));
            dataTable.Columns[nameof(RRStreetName.StreetName)].AllowDBNull = true;

            dataTable.Columns.Add(nameof(RRStreetName.StreetCode), typeof(string));
            dataTable.Columns[nameof(RRStreetName.StreetCode)].AllowDBNull = true;

            dataTable.Columns.Add(nameof(RRStreetName.PostalCode), typeof(string));
            dataTable.Columns[nameof(RRStreetName.PostalCode)].AllowDBNull = true;

            ExtractAndTransform(RrStreetNamesFileName, dataTable);
            Load(connection, transaction, destinationTableName, dataTable);
        }

        private void ImportKadStreetNames(SqlConnection connection, SqlTransaction transaction)
        {
            var destinationTableName = $"{Schema.Legacy}.{KadStreetName.TableName}";
            var dataTable = new DataTable();
            dataTable.Columns.Add(nameof(KadStreetName.StreetNameId), typeof(int));

            dataTable.Columns.Add(nameof(KadStreetName.KadStreetNameCode), typeof(string));
            dataTable.Columns[nameof(KadStreetName.KadStreetNameCode)].AllowDBNull = true;

            dataTable.Columns.Add(nameof(KadStreetName.NisCode), typeof(string));
            dataTable.Columns[nameof(KadStreetName.NisCode)].AllowDBNull = true;

            ExtractAndTransform(KadStreetNamesFileName, dataTable);
            Load(connection, transaction, destinationTableName, dataTable);
        }

        private void ImportRrAddresses(SqlConnection connection, SqlTransaction transaction)
        {
            var destinationTableName = $"{Schema.Legacy}.{RRAddress.TableName}";
            var dataTable = new DataTable();
            dataTable.Columns.Add(nameof(RRAddress.AddressId), typeof(int));
            dataTable.Columns.Add(nameof(RRAddress.AddressType), typeof(string));

            dataTable.Columns.Add(nameof(RRAddress.RRHouseNumber), typeof(string));
            dataTable.Columns[nameof(RRAddress.RRHouseNumber)].AllowDBNull = true;

            dataTable.Columns.Add(nameof(RRAddress.RRIndex), typeof(string));
            dataTable.Columns[nameof(RRAddress.RRIndex)].AllowDBNull = true;

            dataTable.Columns.Add(nameof(RRAddress.StreetCode), typeof(string));
            dataTable.Columns[nameof(RRAddress.StreetCode)].AllowDBNull = true;

            dataTable.Columns.Add(nameof(RRAddress.PostalCode), typeof(string));
            dataTable.Columns[nameof(RRAddress.PostalCode)].AllowDBNull = true;

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
                        var value = reader.GetField(column.DataType, column.ColumnName);

                        if (value is string stringValue && stringValue == string.Empty)
                            row[column.ColumnName] = null;
                        else
                            row[column.ColumnName] = value;
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
    }
}
