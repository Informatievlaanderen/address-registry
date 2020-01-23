namespace AddressRegistry.Projections.Syndication
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Autofac.Features.OwnedInstances;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Municipality;
    using Serilog;
    using StreetName;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using BuildingUnit;
    using Parcel;
    using PostalInfo;

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

            Log.Information("Starting AddressRegistry.Projections.Syndication");

            try
            {
                await DistributedLock<Program>.RunAsync(
                    async () =>
                    {
                        try
                        {
                            await MigrationsHelper.RunAsync(
                                configuration.GetConnectionString("SyndicationProjectionsAdmin"),
                                container.GetService<ILoggerFactory>(),
                                ct);

                            await Task.WhenAll(StartRunners(configuration, container, ct));

                            Log.Information("Running... Press CTRL + C to exit.");
                            Closing.WaitOne();
                        }
                        catch (Exception e)
                        {
                            Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                            throw;
                        }
                    },
                    DistributedLockOptions.LoadFromConfiguration(configuration) ?? DistributedLockOptions.Defaults,
                    container.GetService<ILogger<Program>>());
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                Thread.Sleep(1000);
                throw;
            }

            Log.Information("Stopping...");
            Closing.Close();
        }

        private static IEnumerable<Task> StartRunners(IConfiguration configuration, IServiceProvider container, CancellationToken ct)
        {
            var municipalityRunner = new FeedProjectionRunner<MunicipalityEvent, SyndicationItem<Municipality.Municipality>, SyndicationContext>(
                "municipality",
                configuration.GetValue<Uri>("SyndicationFeeds:Municipality"),
                configuration.GetValue<string>("SyndicationFeeds:MunicipalityAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:MunicipalityAuthPassword"),
                configuration.GetValue<int>("SyndicationFeeds:MunicipalityPollingInMilliseconds"),
                false,
                true,
                container.GetService<ILogger<Program>>(),
                container.GetService<IRegistryAtomFeedReader>(),
                new MunicipalitySyndiciationItemProjections(),
                new MunicipalityLatestProjections(),
                new MunicipalityBosaProjections());

            var postalInfoRunner = new FeedProjectionRunner<PostalInfoEvent, SyndicationItem<PostalInfo.PostalInfo>, SyndicationContext>(
                "postalInfo",
                configuration.GetValue<Uri>("SyndicationFeeds:PostalInfo"),
                configuration.GetValue<string>("SyndicationFeeds:PostalInfoAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:PostalInfoAuthPassword"),
                configuration.GetValue<int>("SyndicationFeeds:PostalInfoPollingInMilliseconds"),
                false,
                true,
                container.GetService<ILogger<Program>>(),
                container.GetService<IRegistryAtomFeedReader>(),
                new PostalInfoLatestProjections());

            var streetNameRunner = new FeedProjectionRunner<StreetNameEvent, SyndicationItem<StreetName.StreetName>, SyndicationContext>(
                "streetName",
                configuration.GetValue<Uri>("SyndicationFeeds:StreetName"),
                configuration.GetValue<string>("SyndicationFeeds:StreetNameAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:StreetNameAuthPassword"),
                configuration.GetValue<int>("SyndicationFeeds:StreetNamePollingInMilliseconds"),
                false,
                true,
                container.GetService<ILogger<Program>>(),
                container.GetService<IRegistryAtomFeedReader>(),
                new StreetNameSyndicationItemProjections(),
                new StreetNameLatestProjections(),
                new StreetNameBosaProjections());

            var parcelRunner = new FeedProjectionRunner<ParcelEvent, SyndicationItem<Parcel.Parcel>, SyndicationContext>(
                "parcel",
                configuration.GetValue<Uri>("SyndicationFeeds:Parcel"),
                configuration.GetValue<string>("SyndicationFeeds:ParcelAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:ParcelAuthPassword"),
                configuration.GetValue<int>("SyndicationFeeds:ParcelPollingInMilliseconds"),
                false,
                true,
                container.GetService<ILogger<Program>>(),
                container.GetService<IRegistryAtomFeedReader>(),
                new ParcelAddressMatchProjections());

            var buildingUnitRunner = new FeedProjectionRunner<BuildingEvent, SyndicationItem<Building>, SyndicationContext>(
                "buildingUnit",
                configuration.GetValue<Uri>("SyndicationFeeds:Building"),
                configuration.GetValue<string>("SyndicationFeeds:BuildingAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:BuildingAuthPassword"),
                configuration.GetValue<int>("SyndicationFeeds:BuildingPollingInMilliseconds"),
                true,
                true,
                container.GetService<ILogger<Program>>(),
                container.GetService<IRegistryAtomFeedReader>(),
                new BuildingUnitAddressMatchProjections());

            yield return municipalityRunner.CatchUpAsync(
                container.GetService<Func<Owned<SyndicationContext>>>(),
                ct);

            yield return postalInfoRunner.CatchUpAsync(
                container.GetService<Func<Owned<SyndicationContext>>>(),
                ct);

            yield return streetNameRunner.CatchUpAsync(
                container.GetService<Func<Owned<SyndicationContext>>>(),
                ct);

            yield return parcelRunner.CatchUpAsync(
                container.GetService<Func<Owned<SyndicationContext>>>(),
                ct);

            yield return buildingUnitRunner.CatchUpAsync(
                container.GetService<Func<Owned<SyndicationContext>>>(),
                ct);
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule(configuration, services));

            var tempProvider = services.BuildServiceProvider();
            builder.RegisterModule(new SyndicationModule(configuration, services, tempProvider.GetService<ILoggerFactory>()));

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
