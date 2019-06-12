namespace AddressRegistry.Importer.Subaddress
{
    using System;
    using System.Configuration;
    using System.Diagnostics;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.CommandLine;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Serilog;
    using HouseNumber.Crab;
    using Serilog;
    using Serilog.Events;

    internal class Program
    {
        private static Stopwatch _stopwatch;
        private static int _commandCounter;

        private static void Main(string[] args)
        {
            try
            {
                var options = new ImportOptions(
                    args,
                    errors => WaitForExit("Could not parse commandline options."));
                var settings = new SettingsBasedConfig();

                var generator = new SubaddressCommandGenerator(ConfigurationManager.ConnectionStrings["Crab2Vbr"].ConnectionString);

                MapLogging.Log = s => _commandCounter++;

                var builder = new CommandProcessorBuilder<int>(generator)
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
                    .Build();

                WaitForStart();

                builder.Run(options, settings);

                WaitForExit();
            }
            catch (Exception exception)
            {
                WaitForExit("General error occurred", exception);
            }
        }

        private static void WaitForExit(string errorMessage = null, Exception exception = null)
        {
            if (!string.IsNullOrEmpty(errorMessage))
                Console.Error.WriteLine(errorMessage);

            if (exception != null)
                Console.Error.WriteLine(exception);

            Console.WriteLine();
            if (_stopwatch != null)
            {
                var avg = _commandCounter / _stopwatch.Elapsed.TotalSeconds;
                var summary = $"Report: generated {_commandCounter} commands in {_stopwatch.Elapsed}ms (={avg}/second).";
                Console.WriteLine(summary);
            }

            Console.WriteLine("Done! Press ENTER key to exit...");
            ConsoleExtensions.WaitFor(ConsoleKey.Enter);

            if (!string.IsNullOrEmpty(errorMessage))
                Environment.Exit(1);

            Environment.Exit(0);
        }

        private static void WaitForStart()
        {
            Console.WriteLine("Press ENTER key to start the CRAB Import...");
            ConsoleExtensions.WaitFor(ConsoleKey.Enter);
            _stopwatch = Stopwatch.StartNew();
        }
    }
}
