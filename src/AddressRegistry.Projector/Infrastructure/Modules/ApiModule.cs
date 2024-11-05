namespace AddressRegistry.Projector.Infrastructure.Modules
{
    using AddressRegistry.Infrastructure;
    using AddressRegistry.Projections.AddressMatch;
    using AddressRegistry.Projections.Elastic;
    using AddressRegistry.Projections.Elastic.AddressSearch;
    using AddressRegistry.Projections.Elastic.Infrastructure;
    using AddressRegistry.Projections.Extract;
    using AddressRegistry.Projections.Extract.AddressCrabHouseNumberIdExtract;
    using AddressRegistry.Projections.Extract.AddressCrabSubaddressIdExtract;
    using AddressRegistry.Projections.Extract.AddressExtract;
    using AddressRegistry.Projections.Integration;
    using AddressRegistry.Projections.Integration.Infrastructure;
    using AddressRegistry.Projections.Integration.LatestItem;
    using AddressRegistry.Projections.Integration.Merger;
    using AddressRegistry.Projections.Integration.Version;
    using AddressRegistry.Projections.LastChangedList;
    using AddressRegistry.Projections.Legacy;
    using AddressRegistry.Projections.Legacy.AddressDetailV2WithParent;
    using AddressRegistry.Projections.Legacy.AddressListV2;
    using AddressRegistry.Projections.Legacy.AddressSyndication;
    using AddressRegistry.Projections.Wfs;
    using AddressRegistry.Projections.Wfs.AddressWfs;
    using AddressRegistry.Projections.Wms;
    using AddressRegistry.Projections.Wms.AddressWmsItemV2;
    using AddressRegistry.Projections.Wms.AddressWmsItemV3;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Consumer.Read.Municipality;
    using Consumer.Read.Municipality.Infrastructure.Modules;
    using Consumer.Read.Postal;
    using Consumer.Read.Postal.Infrastructure.Modules;
    using Consumer.Read.StreetName;
    using Consumer.Read.StreetName.Infrastructure.Modules;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NetTopologySuite.IO;
    using SqlStreamStore;

    public class ApiModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public ApiModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
        }

        protected override void Load(ContainerBuilder builder)
        {
            RegisterProjectionSetup(builder);

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.Populate(_services);
        }

        private void RegisterProjectionSetup(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new EventHandlingModule(
                    typeof(DomainAssemblyMarker).Assembly,
                    EventsJsonSerializerSettingsProvider.CreateSerializerSettings()
                )
            );

            builder.RegisterModule<EnvelopeModule>();
            builder.RegisterEventstreamModule(_configuration);
            builder.RegisterModule(new ProjectorModule(_configuration));

            RegisterLastChangedProjections(builder);

            RegisterExtractProjectionsV2(builder);
            RegisterLegacyProjectionsV2(builder);
            RegisterWfsProjectionsV2(builder);
            RegisterWmsProjectionsV2(builder);
            RegisterAddressMatchProjections(builder);

            if(_configuration.GetSection("Integration").GetValue("Enabled", false))
                RegisterIntegrationProjections(builder);

            RegisterElasticProjections(builder);
        }

        private void RegisterIntegrationProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new IntegrationModule(
                        _configuration,
                        _services,
                        _loggerFactory));
            builder
                .RegisterProjectionMigrator<IntegrationContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<AddressVersionProjections, IntegrationContext>(
                    context => new AddressVersionProjections(context.Resolve<IOptions<IntegrationOptions>>(),
                        context.Resolve<IEventsRepository>()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<AddressLatestItemProjections, IntegrationContext>(
                    context => new AddressLatestItemProjections(context.Resolve<IOptions<IntegrationOptions>>()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<AddressIdAddressPersistentLocalIdRelationProjections, IntegrationContext>(
                    _ => new AddressIdAddressPersistentLocalIdRelationProjections(),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<AddressMergerItemProjections, IntegrationContext>(ConnectedProjectionSettings.Default);
        }

        private void RegisterExtractProjectionsV2(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new ExtractModule(
                    _configuration,
                    _services,
                    _loggerFactory));

            builder
                .RegisterProjectionMigrator<ExtractContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<AddressExtractProjectionsV2, ExtractContext>(
                    context => new AddressExtractProjectionsV2(
                        context.Resolve<IReadonlyStreamStore>(),
                        context.Resolve<EventDeserializer>(),
                        context.Resolve<IOptions<ExtractConfig>>(),
                        DbaseCodePage.Western_European_ANSI.ToEncoding(),
                        new WKBReader()),
                    ConnectedProjectionSettings.Default);
        }

        private void RegisterLastChangedProjections(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new AddressLastChangedListModule(
                    _configuration.GetConnectionString("LastChangedList"),
                    _services,
                    _loggerFactory));
        }

        private void RegisterLegacyProjectionsV2(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new LegacyModule(
                        _configuration,
                        _services,
                        _loggerFactory));
            builder
                .RegisterProjectionMigrator<LegacyContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<AddressDetailProjectionsV2WithParent, LegacyContext>(
                    () => new AddressDetailProjectionsV2WithParent(),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<AddressListProjectionsV2, LegacyContext>(ConnectedProjectionSettings.Default)
                .RegisterProjections<AddressSyndicationProjections, LegacyContext>(
                    () => new AddressSyndicationProjections(),
                    ConnectedProjectionSettings.Default);
        }

        private void RegisterAddressMatchProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new AddressMatchModule(
                        _configuration,
                        _services,
                        _loggerFactory));
            builder
                .RegisterProjectionMigrator<AddressMatchContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<AddressRegistry.Projections.AddressMatch.AddressDetailV2WithParent.AddressDetailProjectionsV2WithParent, AddressMatchContext>(
                    () => new AddressRegistry.Projections.AddressMatch.AddressDetailV2WithParent.AddressDetailProjectionsV2WithParent(),
                    ConnectedProjectionSettings.Default);
        }

        private void RegisterWfsProjectionsV2(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new WfsModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var wfsProjectionSettings = ConnectedProjectionSettings
                .Configure(settings =>
                    settings.ConfigureLinearBackoff<SqlException>(_configuration, "Wfs"));

            builder
                .RegisterProjectionMigrator<WfsContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<AddressWfsProjections, WfsContext>(() =>
                        new AddressWfsProjections(WKBReaderFactory.CreateForLegacy()),
                    wfsProjectionSettings);
        }

        private void RegisterWmsProjectionsV2(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new WmsModule(
                        _configuration,
                        _services,
                        _loggerFactory));

            var wmsProjectionSettings = ConnectedProjectionSettings
                .Configure(settings =>
                    settings.ConfigureLinearBackoff<SqlException>(_configuration, "Wms"));

            builder
                .RegisterProjectionMigrator<WmsContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<AddressWmsItemV2Projections, WmsContext>(() =>
                        new AddressWmsItemV2Projections(WKBReaderFactory.CreateForLegacy()),
                    wmsProjectionSettings)
                .RegisterProjections<AddressWmsItemV3Projections, WmsContext>(() =>
                    new AddressWmsItemV3Projections(WKBReaderFactory.CreateForLegacy()),
                    wmsProjectionSettings);
        }

        private void RegisterElasticProjections(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new ElasticRunnerModule(
                        _configuration,
                        _services,
                        _loggerFactory))
                .RegisterModule(new AddressRegistry.Projections.Elastic.Infrastructure.ElasticModule(_configuration))
                .RegisterModule(new StreetNameConsumerModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new PostalConsumerModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new MunicipalityConsumerModule(_configuration, _services, _loggerFactory));

            _services.AddDbContextFactory<StreetNameConsumerContext>();
            _services.AddDbContextFactory<PostalConsumerContext>();
            _services.AddDbContextFactory<MunicipalityConsumerContext>();

            builder
                .RegisterProjectionMigrator<ElasticRunnerContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<AddressSearchProjections, ElasticRunnerContext>((c) =>
                        new AddressSearchProjections(c.Resolve<IAddressElasticsearchClient>(),
                            c.Resolve<IDbContextFactory<MunicipalityConsumerContext>>(),
                            c.Resolve<IDbContextFactory<PostalConsumerContext>>(),
                            c.Resolve<IDbContextFactory<StreetNameConsumerContext>>()),
                    ConnectedProjectionSettings.Configure(x =>
                    {
                        x.ConfigureCatchUpUpdatePositionMessageInterval(1);
                    }));
        }
    }
}
