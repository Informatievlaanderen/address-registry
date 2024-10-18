namespace AddressRegistry.Api.Extract.Extracts
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Asp.Versioning;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Api.Extract;
    using Consumer.Read.Municipality;
    using Consumer.Read.StreetName;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Data.Sqlite;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using Projections.Extract;
    using Responses;
    using Swashbuckle.AspNetCore.Filters;
    using ProblemDetails = Be.Vlaanderen.Basisregisters.BasicApiProblem.ProblemDetails;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("extract")]
    [ApiExplorerSettings(GroupName = "Extract")]
    public class ExtractController : ApiController
    {
        /// <summary>
        /// Vraag een dump van het volledige register op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="municipalityConsumerContext"></param>
        /// <param name="streetNameConsumerContext"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als adresregister kan gedownload worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressRegistryResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromServices] ExtractContext context,
            [FromServices] MunicipalityConsumerContext municipalityConsumerContext,
            [FromServices] StreetNameConsumerContext streetNameConsumerContext,
            CancellationToken cancellationToken = default)
        {
            return new IsolationExtractArchive(ExtractFileNames.GetAddressZip(), context)
                {
                    AddressRegistryExtractBuilder.CreateAddressFilesV2(context, streetNameConsumerContext, municipalityConsumerContext)
                }
                .CreateFileCallbackResult(cancellationToken);
        }

        /// <summary>
        /// Vraag een dump van de crab links in het register op.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als crab-adresregister kan gedownload worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("crab")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressRegistryResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> Get(
            [FromServices] ExtractContext context,
            CancellationToken cancellationToken = default)
        {
            return new IsolationExtractArchive(ExtractFileNames.GetAddressCrabZip(), context)
                {
                    AddressCrabHouseNumberIdExtractBuilder.CreateAddressCrabHouseNumberIdFile(context),
                    AddressCrabSubaddressIdExtractBuilder.CreateAddressSubaddressIdFile(context)
                }
                .CreateFileCallbackResult(cancellationToken);
        }

        /// <summary>
        /// Vraag een dump van alle postcode-straatnaam koppelingen op.
        /// </summary>
        /// <param name="configuration"></param>
        /// <param name="cancellationToken"></param>
        /// <response code="200">Als postcode-straatnaam koppelingen kan gedownload worden.</response>
        /// <response code="500">Als er een interne fout is opgetreden.</response>
        [HttpGet("postcode-straatnamen")]
        [ProducesResponseType(typeof(FileStreamResult), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerResponseExample(StatusCodes.Status200OK, typeof(AddressRegistryResponseExample))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        public async Task<IActionResult> GetPostalCodeStreetNameLinks(
            [FromServices] IConfiguration configuration,
            CancellationToken cancellationToken = default)
        {
            var extractBuilder = new PostalCodeStreetNameExtractBuilder(configuration.GetConnectionString("ExtractProjections"));

            return new ExtractArchive(ExtractFileNames.GetPostalCodeStreetNameLinksZip())
                {
                    await extractBuilder.CreateLinkedPostalCodeStreetNameFile(cancellationToken)
                }
                .CreateFileCallbackResult(cancellationToken);
        }

        [HttpGet("geopackage")]
        public async Task<IActionResult> GetGeopackage(
            [FromServices] ExtractContext context,
            [FromServices] MunicipalityConsumerContext municipalityConsumerContext,
            [FromServices] StreetNameConsumerContext streetNameConsumerContext,
            CancellationToken cancellationToken = default)
        {
            return new ExtractArchive("geopackage.zip")
                {
                    await CreateGeopackage(context, cancellationToken)
                }
                .CreateFileCallbackResult(cancellationToken);
        }

        public async Task<ExtractFile> CreateGeopackage(ExtractContext context, CancellationToken cancellationToken)
        {
            return new ExtractFile(new ExtractFileName("geopackage", "gpkg"), (stream, token) =>
            {
                var geopackage = new GeopackageWriter(stream);
                geopackage.CreateTables(context, token).GetAwaiter().GetResult();
            });
        }
    }

    public class GeopackageWriter(Stream stream): IAsyncDisposable
    {
        public async Task CreateTables(ExtractContext context, CancellationToken token)
        {
            // Path to save the database file
            string filePath = "geopackage.gpkg";

            if(File.Exists(filePath))
                File.Delete(filePath);

            await Task.Delay(50, token);

            await using (var connection = new SqliteConnection("Data Source=:memory:"))
            {
                await connection.OpenAsync(token);
                var setApplicationId = "PRAGMA application_id = 1196437808;";
                ExecuteNonQuery(connection, setApplicationId);

                CreateSpatialRefSysTable(connection);
                CreateContentsTable(connection);
                CreateGeometryColumnsTable(connection);
                CreateExtensionsTable(connection);
                CreateAddressTable(connection);

                InsertAddressData(context, connection);

                await using (var fileConnection = new SqliteConnection($"Data Source={filePath}"))
                {
                    await fileConnection.OpenAsync(token);
                    connection.BackupDatabase(fileConnection);
                    fileConnection.Close();
                }

                await connection.CloseAsync();
            }

            SqliteConnection.ClearAllPools();

            GC.Collect();
            GC.WaitForPendingFinalizers();

            while(IsFileLocked(filePath))
            {
                await Task.Delay(50, token);
            }

            await using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                await fileStream.CopyToAsync(stream, token);
            }
        }

        private void InsertAddressData(ExtractContext context, SqliteConnection connection)
        {
            context.AddressExtractV2
                //.Take(100)
                .AsNoTracking()
                .Select(address => new
                {
                    address.AddressPersistentLocalId,
                    address.StreetNamePersistentLocalId,
                    address.MinimumX,
                    address.MaximumY
                })
                .ToList()
                .ForEach(address =>
                {
                    var geom = new Point(address.MinimumX, address.MaximumY)
                    {
                        SRID = 31370
                    };
                    var spatialite = new GeoPackageGeoWriter(){HandleOrdinates = Ordinates.XY}.Write(geom);

                    var insertAddress = @"
                        INSERT INTO address (
                            streetname_id,
                            geometry
                        ) VALUES (
                            @streetname_id,
                            @geometry
                        );";
                    using var command = new SqliteCommand(insertAddress, connection);
                    command.Parameters.AddWithValue("@id", address.AddressPersistentLocalId);
                    command.Parameters.AddWithValue("@streetname_id", address.StreetNamePersistentLocalId);
                    command.Parameters.AddWithValue("@geometry", spatialite);
                    command.ExecuteNonQuery();
                });
        }

        public static bool IsFileLocked(string filePath)
        {
            try
            {
                using (var handle = File.OpenHandle(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    // If we get a handle, it means the file is not locked
                    return false;
                }
            }
            catch (IOException)
            {
                // If an IOException is thrown, the file is locked
                return true;
            }
        }

        static void CreateAddressTable(SqliteConnection sqliteConnection)
{
    string createAddressTable = @"
        CREATE TABLE address (
            id INTEGER PRIMARY KEY,
            streetname_id INTEGER NOT NULL,
            geometry POINT NOT NULL
        );";
    ExecuteNonQuery(sqliteConnection, createAddressTable);

     string insertAddress = @"
        INSERT INTO gpkg_contents (
            table_name,
            data_type,
            identifier,
            description,
            min_x,
            min_y,
            max_x,
            max_y,
            srs_id
        ) VALUES (
            'address',
            'features',
            'address',
            'Address table',
            -180,
            -90,
            180,
            90,
            31370
        );";
    ExecuteNonQuery(sqliteConnection, insertAddress);

    var insertGeometryColumns = @"
        INSERT INTO gpkg_geometry_columns (
            table_name,
            column_name,
            geometry_type_name,
            srs_id,
            z,
            m
        ) VALUES (
            'address',
            'geometry',
            'POINT',
            31370,
            0,
            0
        );";

    ExecuteNonQuery(sqliteConnection, insertGeometryColumns);

    // var createSpatialIndex = @"
    //     CREATE VIRTUAL TABLE rtree_address_geometry
    //     USING rtree(
    //         id,
    //         minx, maxx,
    //         miny, maxy
    //     );";
    //
    // ExecuteNonQuery(sqliteConnection, createSpatialIndex);
    //
    // var insertSpatialIndex = @"
    //     INSERT INTO rtree_address_geometry (
    //         id,
    //         minx, maxx,
    //         miny, maxy
    //     ) SELECT
    //         id,
    //         4.3847,
    //         50.8466,
    //         4.3847,
    //         50.8466
    //     FROM address;";
    //
    // ExecuteNonQuery(sqliteConnection, insertSpatialIndex);
    //
    // var addRTreeExtension = @"
    // INSERT INTO gpkg_extensions (
    //     table_name,
    //     column_name,
    //     extension_name,
    //     definition,
    //     scope
    // ) VALUES (
    //     'address',
    //     'geometry',
    //     'gpkg_rtree_index',
    //     'http://www.geopackage.org/spec/#extension_rtree',
    //     'write-only'
    // );";
    //
    // ExecuteNonQuery(sqliteConnection, addRTreeExtension);
}


static void CreateSpatialRefSysTable(SqliteConnection connection)
{
    string createSrsTable = @"
        CREATE TABLE gpkg_spatial_ref_sys (
            srs_name TEXT NOT NULL,
            srs_id INTEGER PRIMARY KEY,
            organization TEXT NOT NULL,
            organization_coordsys_id INTEGER NOT NULL,
            definition TEXT NOT NULL,
            description TEXT
        );";
    ExecuteNonQuery(connection, createSrsTable);

    string insertLambert72 = @"
        INSERT INTO gpkg_spatial_ref_sys (
            srs_name,
            srs_id,
            organization,
            organization_coordsys_id,
            definition
        ) VALUES (
            'Lambert 72',
            31370,
            'EPSG',
            31370,
            'PROJCS[""Belge 1972 / Belgian Lambert 72"",GEOGCS[""Belge 1972"",DATUM[""Reseau_National_Belge_1972"",SPHEROID[""International 1924"",6378388,297,AUTHORITY[""EPSG"",""7022""]],TOWGS84[106.869,-52.2978,103.724,-0.33657,0.456955,-1.84218,1],AUTHORITY[""EPSG"",""6313""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4313""]],PROJECTION[""Lambert_Conformal_Conic_2SP""],PARAMETER[""standard_parallel_1"",51.1666672333333],PARAMETER[""standard_parallel_2"",49.8333339],PARAMETER[""latitude_of_origin"",90],PARAMETER[""central_meridian"",4.36748666666667],PARAMETER[""false_easting"",150000.013],PARAMETER[""false_northing"",5400088.438],UNIT[""metre"",1,AUTHORITY[""EPSG"",""9001""]],AXIS[""X"",EAST],AXIS[""Y"",NORTH],AUTHORITY[""EPSG"",""31370""]]'
        );";
    ExecuteNonQuery(connection, insertLambert72);

    string insertWGS84 = @"
        INSERT INTO gpkg_spatial_ref_sys (
            srs_name,
            srs_id,
            organization,
            organization_coordsys_id,
            definition
        ) VALUES (
            'WGS 84',
            4326,
            'EPSG',
            4326,
            'GEOGCS[""WGS 84"",DATUM[""WGS_1984"",SPHEROID[""WGS 84"",6378137,298.257223563,AUTHORITY[""EPSG"",""7030""]],AUTHORITY[""EPSG"",""6326""]],PRIMEM[""Greenwich"",0,AUTHORITY[""EPSG"",""8901""]],UNIT[""degree"",0.0174532925199433,AUTHORITY[""EPSG"",""9122""]],AUTHORITY[""EPSG"",""4326""]]'
        );";
    ExecuteNonQuery(connection, insertWGS84);

    string insertUndefinedCartesian = @"
        INSERT INTO gpkg_spatial_ref_sys (
            srs_name,
            srs_id,
            organization,
            organization_coordsys_id,
            definition
        ) VALUES (
            'Undefined Cartesian',
            -1,
            'NONE',
            -1,
            'undefined'
        );";
    ExecuteNonQuery(connection, insertUndefinedCartesian);

    string insertUndefinedGeographic = @"
        INSERT INTO gpkg_spatial_ref_sys (
            srs_name,
            srs_id,
            organization,
            organization_coordsys_id,
            definition
        ) VALUES (
            'Undefined Geographic',
            0,
            'NONE',
            0,
            'undefined'
        );";
    ExecuteNonQuery(connection, insertUndefinedGeographic);
}

static void CreateContentsTable(SqliteConnection connection)
{
    string createContentsTable = @"
            CREATE TABLE gpkg_contents (
                table_name TEXT NOT NULL PRIMARY KEY,
                data_type TEXT NOT NULL,
                identifier TEXT UNIQUE,
                description TEXT DEFAULT '',
                last_change DATETIME NOT NULL DEFAULT (strftime('%Y-%m-%dT%H:%M:%fZ', 'now')),
                min_x DOUBLE,
                min_y DOUBLE,
                max_x DOUBLE,
                max_y DOUBLE,
                srs_id INTEGER,
                FOREIGN KEY (srs_id) REFERENCES gpkg_spatial_ref_sys(srs_id)
            );";
    ExecuteNonQuery(connection, createContentsTable);
}

static void CreateGeometryColumnsTable(SqliteConnection connection)
{
    string createGeometryColumnsTable = @"
            CREATE TABLE gpkg_geometry_columns (
                table_name TEXT NOT NULL,
                column_name TEXT NOT NULL,
                geometry_type_name TEXT NOT NULL,
                srs_id INTEGER NOT NULL,
                z TINYINT NOT NULL,
                m TINYINT NOT NULL,
                PRIMARY KEY (table_name, column_name),
                FOREIGN KEY (srs_id) REFERENCES gpkg_spatial_ref_sys(srs_id),
                FOREIGN KEY (table_name) REFERENCES gpkg_contents(table_name)
            );";
    ExecuteNonQuery(connection, createGeometryColumnsTable);
}

static void CreateExtensionsTable(SqliteConnection connection)
{
    string createExtensionsTable = @"
        CREATE TABLE gpkg_extensions (
            table_name TEXT,
            column_name TEXT,
            extension_name TEXT NOT NULL,
            definition TEXT NOT NULL,
            scope TEXT NOT NULL,
            CONSTRAINT ge_tce UNIQUE (table_name, column_name, extension_name)
        );";
    ExecuteNonQuery(connection, createExtensionsTable);
}

        public async ValueTask DisposeAsync()
        {
            await stream.DisposeAsync();
        }

        private static void ExecuteNonQuery(SqliteConnection connection, string sql)
        {
            using var command = new SqliteCommand(sql, connection);
            command.ExecuteNonQuery();
        }
    }
}
