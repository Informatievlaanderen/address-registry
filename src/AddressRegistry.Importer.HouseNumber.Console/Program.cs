namespace AddressRegistry.Importer.HouseNumber.Console
{
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.CommandLine;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Serilog;
    using Crab;
    using Serilog;
    using Serilog.Events;
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Microsoft.Extensions.Configuration;

    internal class Program
    {
        private static Stopwatch _stopwatch;
        private static int _commandCounter;

        private static void Main(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args ?? new string[0])
                .Build();

            var mailSettings = configuration.GetSection("ApplicationSettings").GetSection("SerilogMail");

            Log.Logger = new LoggerConfiguration()
                .WriteTo.File("tracing.log", LogEventLevel.Verbose)
                .WriteTo.Console(LogEventLevel.Information)
                .WriteTo.SendGridSmtp(
                    mailSettings["apiKey"],
                    mailSettings["subject"],
                    mailSettings["fromEmail"],
                    mailSettings["toEmail"],
                    (LogEventLevel)Enum.Parse(typeof(LogEventLevel), mailSettings["restrictedToMinimumLevel"], true))
                .ReadFrom.Configuration(configuration)
                .CreateLogger();

            var crabConnectionString = configuration.GetConnectionString("CRABEntities");
            Func<CRABEntities> crabEntitiesFactory = () =>
            {
                var factory = new CRABEntities(crabConnectionString);
                factory.Database.CommandTimeout = 60 * 60;
                return factory;
            };

            var settings = new SettingsBasedConfig(configuration.GetSection("ApplicationSettings"));
            try
            {
                var options = new ImportOptions(
                    args,
                    errors => WaitForExit(settings.WaitForUserInput, "Could not parse commandline options."));

                var generator = new HouseNumberCommandGenerator(configuration.GetConnectionString("Crab2Vbr"), crabEntitiesFactory);

                MapLogging.Log = s => _commandCounter++;

                var commandProcessor = new CommandProcessorBuilder<int>(generator)
                    .WithCommandLineOptions(options.ImportArguments)
                    .UseSerilog(cfg => cfg
                        .WriteTo.File(
                            "tracing.log",
                            LogEventLevel.Verbose,
                            retainedFileCountLimit: 20,
                            fileSizeLimitBytes: 104857600,
                            rollOnFileSizeLimit: true,
                            rollingInterval: RollingInterval.Day)
                        .WriteTo.Console(LogEventLevel.Information))
                    .UseHttpApiProxyConfig(settings)
                    .UseCommandProcessorConfig(settings)
                    .UseDefaultSerializerSettingsForCrabImports()
                    .UseImportFeed(new ImportFeed { Name = settings.FeedName })
                    .Build();

                WaitForStart(settings.WaitForUserInput);

                commandProcessor.Run(options, settings);

                WaitForExit(settings.WaitForUserInput);
            }
            catch (Exception exception)
            {
                WaitForExit(settings.WaitForUserInput, "General error occurred", exception);
            }
        }

        private static void WaitForExit(bool waitForUserInput, string errorMessage = null, Exception exception = null)
        {
            if (!string.IsNullOrEmpty(errorMessage) || exception != null)
                Log.Fatal(exception, errorMessage);

            Console.WriteLine();

            if (_stopwatch != null)
            {
                var avg = _commandCounter / _stopwatch.Elapsed.TotalSeconds;
                var summary = $"Report: generated {_commandCounter} commands in {_stopwatch.Elapsed}ms (={avg}/second).";
                Console.WriteLine(summary);
            }

            if (waitForUserInput)
            {
                Console.WriteLine("Done! Press ENTER key to exit...");
                ConsoleExtensions.WaitFor(ConsoleKey.Enter);
            }

            if (!string.IsNullOrEmpty(errorMessage))
                Environment.Exit(1);

            Environment.Exit(0);
        }

        private static void WaitForStart(bool waitForUserInput)
        {
            if (waitForUserInput)
            {
                Console.WriteLine("Press ENTER key to start the CRAB Import...");
                ConsoleExtensions.WaitFor(ConsoleKey.Enter);
            }
            else
                Console.WriteLine("Starting CRAB Import...");

            _stopwatch = Stopwatch.StartNew();
        }
    }
}
