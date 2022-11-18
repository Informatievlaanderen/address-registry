namespace AddressRegistry.Projections.Syndication
{
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Municipality;
    using Serilog;
    using StreetName;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressLink;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using BuildingUnit;
    using Infrastructure.Modules;
    using Parcel;
    using PostalInfo;

    public class Program
    {
        private static readonly AutoResetEvent Closing = new AutoResetEvent(false);
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        protected Program()
        { }

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
                .AddCommandLine(args)
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
                                container.GetRequiredService<ILoggerFactory>(),
                                ct);

                            await container
                                .GetRequiredService<FeedProjector<SyndicationContext>>()
                                .Register(BuildProjectionRunners(configuration, container))
                                .Start(ct);
                        }
                        catch (Exception e)
                        {
                            Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                            throw;
                        }
                    },
                    DistributedLockOptions.LoadFromConfiguration(configuration),
                    container.GetRequiredService<ILogger<Program>>());
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

        private static IEnumerable<IFeedProjectionRunner<SyndicationContext>> BuildProjectionRunners(IConfiguration configuration, IServiceProvider container)
        {
            ILogger<Program> CreateLogger() => container
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger<Program>();

            var municipalityRunner = new FeedProjectionRunner<MunicipalityEvent, SyndicationItem<Municipality.Municipality>, SyndicationContext>(
                "municipality",
                configuration.GetValue<Uri>("SyndicationFeeds:Municipality"),
                configuration.GetValue<string>("SyndicationFeeds:MunicipalityAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:MunicipalityAuthPassword"),
                configuration.GetValue<int>("SyndicationFeeds:MunicipalityPollingInMilliseconds"),
                false,
                true,
                CreateLogger(),
                container.GetRequiredService<IRegistryAtomFeedReader>(),
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
                CreateLogger(),
                container.GetRequiredService<IRegistryAtomFeedReader>(),
                new PostalInfoLatestProjections());

            var streetNameRunner = new FeedProjectionRunner<StreetNameEvent, SyndicationItem<StreetName.StreetName>, SyndicationContext>(
                "streetName",
                configuration.GetValue<Uri>("SyndicationFeeds:StreetName"),
                configuration.GetValue<string>("SyndicationFeeds:StreetNameAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:StreetNameAuthPassword"),
                configuration.GetValue<int>("SyndicationFeeds:StreetNamePollingInMilliseconds"),
                false,
                true,
                CreateLogger(),
                container.GetRequiredService<IRegistryAtomFeedReader>(),
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
                CreateLogger(),
                container.GetRequiredService<IRegistryAtomFeedReader>(),
                new ParcelAddressMatchProjections());

            var buildingUnitRunner = new FeedProjectionRunner<BuildingEvent, SyndicationItem<Building>, SyndicationContext>(
                "buildingUnit",
                configuration.GetValue<Uri>("SyndicationFeeds:Building"),
                configuration.GetValue<string>("SyndicationFeeds:BuildingAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:BuildingAuthPassword"),
                configuration.GetValue<int>("SyndicationFeeds:BuildingPollingInMilliseconds"),
                true,
                true,
                CreateLogger(),
                container.GetRequiredService<IRegistryAtomFeedReader>(),
                new BuildingUnitAddressMatchProjections());

            var buildingUnitAddressRunner = new LinkedFeedProjectionRunner<BuildingEvent, SyndicationItem<Building>, SyndicationContext>(
                "buildingUnitAddressLink",
                configuration.GetValue<Uri>("SyndicationFeeds:Building"),
                configuration.GetValue<string>("SyndicationFeeds:BuildingAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:BuildingAuthPassword"),
                false,
                true,
                configuration.GetValue<int>("LinkedFeedRetryPolicy:NumberOfRetryAttempts"),
                configuration.GetValue<int>("LinkedFeedRetryPolicy:JittererMinSeconds"),
                configuration.GetValue<int>("LinkedFeedRetryPolicy:JittererMaxSeconds"),
                CreateLogger(),
                container.GetRequiredService<IRegistryAtomFeedReader>(),
                new AddressBuildingUnitLinkProjections(DbaseCodePage.Western_European_ANSI.ToEncoding(), CreateLogger()));

            var parcelAddressRunner = new LinkedFeedProjectionRunner<ParcelEvent, SyndicationItem<Parcel.Parcel>, SyndicationContext>(
                "parcelAddressLink",
                configuration.GetValue<Uri>("SyndicationFeeds:Parcel"),
                configuration.GetValue<string>("SyndicationFeeds:ParcelAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:ParcelAuthPassword"),
                false,
                true,
                configuration.GetValue<int>("LinkedFeedRetryPolicy:NumberOfRetryAttempts"),
                configuration.GetValue<int>("LinkedFeedRetryPolicy:JittererMinSeconds"),
                configuration.GetValue<int>("LinkedFeedRetryPolicy:JittererMaxSeconds"),
                CreateLogger(),
                container.GetRequiredService<IRegistryAtomFeedReader>(),
                new AddressParcelLinkProjections(DbaseCodePage.Western_European_ANSI.ToEncoding()));

            var addressRunner = new LinkedFeedProjectionRunner<AddressEvent, SyndicationItem<Address>, SyndicationContext>(
                "address",
                configuration.GetValue<Uri>("SyndicationFeeds:Address"),
                configuration.GetValue<string>("SyndicationFeeds:AddressAuthUserName"),
                configuration.GetValue<string>("SyndicationFeeds:AddressAuthPassword"),
                false,
                true,
                configuration.GetValue<int>("LinkedFeedRetryPolicy:NumberOfRetryAttempts"),
                configuration.GetValue<int>("LinkedFeedRetryPolicy:JittererMinSeconds"),
                configuration.GetValue<int>("LinkedFeedRetryPolicy:JittererMaxSeconds"),
                CreateLogger(),
                container.GetRequiredService<IRegistryAtomFeedReader>(),
                new AddressLinkSyndicationProjections(DbaseCodePage.Western_European_ANSI.ToEncoding()));

            var linkedFeedManager = new LinkedFeedProjectionManager<SyndicationContext>(
                new List<ILinkedFeedProjectionRunner<SyndicationContext>>
                {
                   addressRunner,
                   buildingUnitAddressRunner,
                   parcelAddressRunner
                });

            return new IFeedProjectionRunner<SyndicationContext>[]
            {
                municipalityRunner,
                postalInfoRunner,
                streetNameRunner,
                parcelRunner,
                buildingUnitRunner,
                linkedFeedManager
            };
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule(configuration, services));

            var tempProvider = services.BuildServiceProvider();
            builder.RegisterModule(new SyndicationModule(configuration, services, tempProvider.GetRequiredService<ILoggerFactory>()));

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
