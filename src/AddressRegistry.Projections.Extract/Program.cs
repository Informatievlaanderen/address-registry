namespace AddressRegistry.Projections.Extract
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Autofac.Features.OwnedInstances;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Serilog;
    using SqlStreamStore;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressExtract;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using Municipality;
    using StreetName;

    internal class Program
    {
        private static readonly AutoResetEvent Closing = new AutoResetEvent(false);
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        public static async Task Main(string[] args)
        {
            var ct = CancellationTokenSource.Token;

            ct.Register(() => Closing.Set());
            Console.CancelKeyPress += (sender, eventArgs) => CancellationTokenSource.Cancel();

            Console.WriteLine("Starting AddressRegistry.Projections.Extract");

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", false, false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", true, false)
                .AddEnvironmentVariables()
                .AddCommandLine(args ?? new string[0])
                .Build();

            var container = ConfigureServices(configuration);
            var logger = container.GetService<ILogger<Program>>();

            try
            {
                await MigrationsHelper.RunAsync(
                    configuration.GetConnectionString("ExtractProjectionsAdmin"),
                    container.GetService<ILoggerFactory>(),
                    ct);

                await Task.WhenAll(StartRunners(configuration, container, ct));

                Console.WriteLine("Running... Press CTRL + C to exit.");
                Closing.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                Thread.Sleep(1000);
                throw;
            }

            Console.WriteLine("Stopping...");
            Closing.Close();
        }

        private static IEnumerable<Task> StartRunners(IConfiguration configuration, IServiceProvider container, CancellationToken ct)
        {
            var runner = container.GetService<AddressExtractRunner>();

            yield return runner.StartAsync(
                container.GetService<IStreamStore>(),
                container.GetService<Func<Owned<ExtractContext>>>(),
                ct);

            var municipalityRunner = new FeedProjectionRunner<MunicipalityEvent, Municipality.Municipality, ExtractContext>(
                "municipality",
                configuration.GetValue<Uri>("SyndicationFeeds:Municipality"),
                configuration.GetValue<string>("SyndicationFeeds:MunicipalityAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:MunicipalityAuthPassword"),
                configuration.GetValue<int>("SyndicationFeeds:MunicipalityPollingInMilliseconds"),
                container.GetService<ILogger<Program>>(),
                container.GetService<IRegistryAtomFeedReader>(),
                new AddressExtractMunicipalityProjection());

            var streetNameRunner = new FeedProjectionRunner<StreetNameEvent, StreetName.StreetName, ExtractContext>(
                "streetname",
                configuration.GetValue<Uri>("SyndicationFeeds:StreetName"),
                configuration.GetValue<string>("SyndicationFeeds:StreetNameAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:StreetNameAuthPassword"),
                configuration.GetValue<int>("SyndicationFeeds:StreetNamePollingInMilliseconds"),
                container.GetService<ILogger<Program>>(),
                container.GetService<IRegistryAtomFeedReader>(),
                new AddressExtractStreetNameProjection());

            yield return municipalityRunner.CatchUpAsync(
                container.GetService<Func<Owned<ExtractContext>>>(),
                ct);

            yield return streetNameRunner.CatchUpAsync(
                container.GetService<Func<Owned<ExtractContext>>>(),
                ct);
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            services.AddHttpClient();

            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule(configuration, services));

            var tempProvider = services.BuildServiceProvider();
            builder.RegisterModule(new ExtractModule(configuration, services, tempProvider.GetService<ILoggerFactory>()));

            builder.RegisterModule(new ProjectionsModule(configuration));

            builder
                .RegisterType<AddressExtractRunner>()
                .SingleInstance();

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
