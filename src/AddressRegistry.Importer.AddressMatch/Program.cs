namespace AddressRegistry.Importer.AddressMatch
{
    using CsvHelper;
    using CsvHelper.Configuration;
    using Dapper;
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.IO;
    using System.IO.Compression;
    using System.Linq;
    using System.Text;

    class Program
    {
        private static string _crabConnectionString;
        private static readonly Configuration CsvConfiguration = new Configuration { Encoding = Encoding.UTF8 };
        private static readonly string RRStreetNamesPath = "RRStreetNames.csv";
        private static readonly string KadStreetNamesPath = "KadStreetNames.csv";
        private static readonly string RRAddressesPath = "RRAddresses.csv";
        private static readonly string ImportAddressMatchZipPath = "importAddressMatch.zip";

        static void Main(string[] args)
        {
            Console.WriteLine("Starting AddressRegistry.Importer.AddressMatch");

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args ?? new string[0])
                .Build();

            _crabConnectionString = configuration.GetConnectionString("CRAB");

            WriteCsvFile(ExtractRRStreetNames(), RRStreetNamesPath);
            WriteCsvFile(ExtractKadStreetNames(), KadStreetNamesPath);
            WriteCsvFile(ExtractRRAddress(), RRAddressesPath);

            CreateZip(new[] { RRStreetNamesPath, KadStreetNamesPath, RRAddressesPath });

            Console.WriteLine($"Zip created, sending Zip");

            //SendZip

            CleanUpFiles(new[] { RRStreetNamesPath, KadStreetNamesPath, RRAddressesPath, ImportAddressMatchZipPath });
        }

        private static void CreateZip(IEnumerable<string> filesToInclude)
        {
            using (FileStream zipToOpen = new FileStream(ImportAddressMatchZipPath, FileMode.OpenOrCreate))
            {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Create))
                {
                    foreach (var file in filesToInclude)
                    {
                        var entry = archive.CreateEntry(file);
                        using (var zipStream = entry.Open())
                            zipStream.Write(File.ReadAllBytes(file));
                    }
                }
            }
        }

        private static void CleanUpFiles(IEnumerable<string> filesToDelete)
        {
            foreach (var file in filesToDelete)
                File.Delete(file);
        }

        private static void WriteCsvFile<T>(IList<T> records, string filePath)
        {
            Console.WriteLine($"Writing {records.Count()} records to {filePath}");
            using (var streamWriter = new StreamWriter(filePath))
            using (var csvWriter = new CsvWriter(streamWriter, CsvConfiguration))
            {
                csvWriter.WriteRecords(records);
            }
        }

        static IList<RRStreetName> ExtractRRStreetNames()
        {
            using (var connection = new SqlConnection(_crabConnectionString))
            {
                var streetNames = connection.Query<RRStreetName>(@"
                    select s.StraatnaamId as StreetNameId, s.straatNaam as StreetName, ss.straatCode as StreetCode, pk.postKantonCode as PostalCode from odb.tblStraatNaam s
                    inner join odb.tblSubStraat_straatNaam ss_s on s.straatNaamId = ss_s.straatNaamId
                    inner join odb.tblSubStraat ss on ss.subStraatId = ss_s.subStraatId
                    inner join odb.tblSubKanton sk on sk.subKantonId = ss.subKantonId
                    inner join odb.tblPostKanton pk on sk.postKantonId = pk.postKantonId
                    where ss_s.eindDatum is null AND s.eindDatum is null AND ss.eindDatum is null AND sk.eindDatum is null and pk.eindDatum is null");

                return streetNames.ToList();
            }
        }

        static IList<KadStreetName> ExtractKadStreetNames()
        {
            using (var connection = new SqlConnection(_crabConnectionString))
            {
                var streetNames = connection.Query<KadStreetName>(@"
                    select s.StraatnaamId as StreetNameId, ksc.kadStraatNaamCode as KadStreetNameCode, g.nisGemeenteCode as NisCode from odb.tblStraatNaam s
                    inner join odb.tblStraatNaam_kadStraatNaam s_ks on s_ks.straatNaamId = s.straatNaamId
                    inner join odb.tblKadStraatNaam ks on ks.kadStraatNaamId = s_ks.kadStraatNaamId
                    inner join odb.tblKadStraatNaamCode ksc on ksc.kadStraatNaamCodeId = ks.kadStraatNaamCodeId
                    inner join odb.tblGemeente g on g.gemeenteId = ksc.gemeenteId
                    where s_ks.eindDatum is null and s.eindDatum is null and ks.eindDatum is null and ksc.eindDatum is null and g.eindDatum is null");

                return streetNames.ToList();
            }
        }

        static IList<RRAddress> ExtractRRAddress()
        {
            using (var connection = new SqlConnection(_crabConnectionString))
            {
                var streetNames = connection.Query<RRAddress>(@"
                    select arra.adresid as AddressId, arra.aardadres as AddressType, rra.rrHuisNummer as RRHouseNumber, rra.rrIndex as RRIndex, ss.straatCode as StreetCode, pk.postKantonCode as PostalCode from odb.tblRrAdres rra
                    inner join odb.tblAdres_RrAdres arra on arra.rrAdresId = rra.rrAdresId
                    inner join odb.tblSubStraat ss on ss.subStraatId = rra.subStraatId
                    inner join odb.tblSubKanton sk on sk.subKantonId = ss.subKantonId
                    inner join odb.tblPostKanton pk on sk.postKantonId = pk.postKantonId
                    where rra.eindDatum is null and arra.eindDatum is null and ss.eindDatum is null and sk.eindDatum is null and pk.eindDatum is null");

                return streetNames.ToList();
            }
        }
    }

    class RRStreetName
    {
        public int StreetNameId { get; set; }
        public string StreetName { get; set; }
        public string StreetCode { get; set; }
        public string PostalCode { get; set; }
    }

    class KadStreetName
    {
        public int StreetNameId { get; set; }
        public string KadStreetNameCode { get; set; }
        public string NisCode { get; set; }
    }

    class RRAddress
    {
        public int AddressId { get; set; }
        public string AddressType { get; set; }
        public string RRHouseNumber { get; set; }
        public string RRIndex { get; set; }
        public string StreetCode { get; set; }
        public string PostalCode { get; set; }
    }
}
