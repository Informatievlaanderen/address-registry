namespace AddressRegistry.Console.Extract
{
    using Microsoft.Extensions.Configuration;
    using Serilog;
    using System;
    using System.IO;
    using System.IO.Compression;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.Extract.Extracts;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Infrastructure;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Projections.Extract;
    using Projections.Syndication;

    public class Program
    {
        private static readonly AutoResetEvent Closing = new AutoResetEvent(false);
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            var ct = CancellationTokenSource.Token;

            ct.Register(() => Closing.Set());
            Console.CancelKeyPress += (sender, eventArgs) => CancellationTokenSource.Cancel();

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

            var container = ConfigureServices(configuration);

            Log.Information("Starting AddressRegistry.Console.Extract");

            try
            {
                var extractBuilder = new LinkedAddressExtractBuilder(container.GetService<ExtractContext>(), container.GetService<SyndicationContext>());
                var sourceDirectoryName = "addresslinks";

                Directory.CreateDirectory(sourceDirectoryName);
                var buildingTask =Task.Run(() =>
                {
                    using (var file = File.Create(Path.Combine(sourceDirectoryName, ExtractController.FileNameLinksBuildingUnit + ".dbf")))
                        extractBuilder.CreateLinkedBuildingUnitAddressFiles().WriteTo(file, ct);
                });

                var parcelTask = Task.Run(() =>
                {
                    using (var file = File.Create(Path.Combine(sourceDirectoryName, ExtractController.FileNameLinksParcel + ".dbf")))
                        extractBuilder.CreateLinkedParcelAddressFiles().WriteTo(file, ct);
                });

                Task.WaitAll(buildingTask, parcelTask);

                ZipFile.CreateFromDirectory(sourceDirectoryName, ExtractController.ZipNameLinks + ".zip");
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Extract console stopped working");
                throw;
            }


            Log.Information("Stopping AddressRegistry.Console.Extract");
            Closing.Close();
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule(configuration, services));

            var tempProvider = services.BuildServiceProvider();
            builder.RegisterModule(new SyndicationModule(configuration, services, tempProvider.GetService<ILoggerFactory>()));
            builder.RegisterModule(new ExtractModule(configuration, services, tempProvider.GetService<ILoggerFactory>()));

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
