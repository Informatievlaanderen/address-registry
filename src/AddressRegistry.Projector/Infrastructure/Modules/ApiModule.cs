namespace AddressRegistry.Projector.Infrastructure.Modules
{
    using System;
    using AddressRegistry.Infrastructure;
    using AddressRegistry.Projections.Extract;
    using AddressRegistry.Projections.Extract.AddressCrabHouseNumberIdExtract;
    using AddressRegistry.Projections.Extract.AddressCrabSubaddressIdExtract;
    using AddressRegistry.Projections.Extract.AddressExtract;
    using AddressRegistry.Projections.LastChangedList;
    using AddressRegistry.Projections.Legacy;
    using AddressRegistry.Projections.Legacy.AddressDetail;
    using AddressRegistry.Projections.Legacy.AddressDetailV2;
    using AddressRegistry.Projections.Legacy.AddressList;
    using AddressRegistry.Projections.Legacy.AddressListV2;
    using AddressRegistry.Projections.Legacy.AddressSyndication;
    using AddressRegistry.Projections.Legacy.CrabIdToPersistentLocalId;
    using AddressRegistry.Projections.Wfs;
    using AddressRegistry.Projections.Wms;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using NetTopologySuite.IO;

    public class ApiModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;
        private readonly bool _useProjectionsV2;

        public ApiModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
            _useProjectionsV2 = Convert.ToBoolean(_configuration.GetSection(FeatureToggleOptions.ConfigurationKey)[nameof(FeatureToggleOptions.UseProjectionsV2)]);
        }

        protected override void Load(ContainerBuilder builder)
        {
            _services.RegisterModule(new DataDogModule(_configuration));
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

            if (_useProjectionsV2)
            {
                RegisterExtractProjectionsV2(builder);
                RegisterLegacyProjectionsV2(builder);
                RegisterWfsProjectionsV2(builder);
                RegisterWmsProjectionsV2(builder);
                // RegisterWfsProjections(builder); //TODO: Remove when Wfs has been filled in staging
                // RegisterWmsProjections(builder); //TODO: Remove when Wms has been filled in staging
            }
            else
            {
                RegisterExtractProjections(builder);
                RegisterLegacyProjections(builder);
                RegisterWfsProjections(builder);
                RegisterWmsProjections(builder);
            }
        }

        private void RegisterExtractProjections(ContainerBuilder builder)
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
                .RegisterProjections<AddressExtractProjection, ExtractContext>(
                    context => new AddressExtractProjection(context.Resolve<IOptions<ExtractConfig>>(), DbaseCodePage.Western_European_ANSI.ToEncoding(), new WKBReader()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<AddressCrabHouseNumberIdExtractProjection, ExtractContext>(
                    context => new AddressCrabHouseNumberIdExtractProjection(DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<AddressCrabSubaddressIdExtractProjection, ExtractContext>(
                    context => new AddressCrabSubaddressIdExtractProjection(DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    ConnectedProjectionSettings.Default);
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
                    context => new AddressExtractProjectionsV2(context.Resolve<IOptions<ExtractConfig>>(), DbaseCodePage.Western_European_ANSI.ToEncoding(), new WKBReader()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<AddressCrabHouseNumberIdExtractProjection, ExtractContext>(
                    context => new AddressCrabHouseNumberIdExtractProjection(DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<AddressCrabSubaddressIdExtractProjection, ExtractContext>(
                    context => new AddressCrabSubaddressIdExtractProjection(DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    ConnectedProjectionSettings.Default);
        }

        private void RegisterLastChangedProjections(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new AddressLastChangedListModule(
                    _configuration.GetConnectionString("LastChangedList"),
                    _configuration["DataDog:ServiceName"],
                    _services,
                    _loggerFactory));

            builder
                .RegisterProjectionMigrator<AddressRegistry.Projections.LastChangedList.LastChangedListContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjectionMigrator<DataMigrationContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<LastChangedListProjections, LastChangedListContext>(ConnectedProjectionSettings.Default);
        }

        private void RegisterLegacyProjections(ContainerBuilder builder)
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
                .RegisterProjections<AddressDetailProjections, LegacyContext>(
                    () => new AddressDetailProjections(),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<AddressListProjections, LegacyContext>(ConnectedProjectionSettings.Default)
                .RegisterProjections<AddressSyndicationProjections, LegacyContext>(
                    () => new AddressSyndicationProjections(),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<CrabIdToPersistentLocalIdProjections, LegacyContext>(ConnectedProjectionSettings.Default);
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
                .RegisterProjections<AddressDetailProjectionsV2, LegacyContext>(
                    () => new AddressDetailProjectionsV2(),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<AddressListProjectionsV2, LegacyContext>(ConnectedProjectionSettings.Default)
                .RegisterProjections<AddressSyndicationProjections, LegacyContext>(
                    () => new AddressSyndicationProjections(),
                    ConnectedProjectionSettings.Default)
                .RegisterProjections<CrabIdToPersistentLocalIdProjections, LegacyContext>(ConnectedProjectionSettings.Default);
        }

        private void RegisterWfsProjections(ContainerBuilder builder)
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

            .RegisterProjections<AddressRegistry.Projections.Wfs.AddressDetail.AddressDetailProjections, WfsContext>(() =>
                    new AddressRegistry.Projections.Wfs.AddressDetail.AddressDetailProjections(WKBReaderFactory.CreateForLegacy()),
                wfsProjectionSettings);
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

                .RegisterProjections<AddressRegistry.Projections.Wfs.AddressWfs.AddressWfsProjections, WfsContext>(() =>
                        new AddressRegistry.Projections.Wfs.AddressWfs.AddressWfsProjections(WKBReaderFactory.CreateForLegacy()),
                    wfsProjectionSettings);
        }

        private void RegisterWmsProjections(ContainerBuilder builder)
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

            .RegisterProjections<AddressRegistry.Projections.Wms.AddressDetail.AddressDetailProjections, WmsContext>(() =>
                    new AddressRegistry.Projections.Wms.AddressDetail.AddressDetailProjections(WKBReaderFactory.CreateForLegacy()),
                wmsProjectionSettings);
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

                .RegisterProjections<AddressRegistry.Projections.Wms.AddressWmsItem.AddressWmsItemProjections, WmsContext>(() =>
                        new AddressRegistry.Projections.Wms.AddressWmsItem.AddressWmsItemProjections(WKBReaderFactory.CreateForLegacy()),
                    wmsProjectionSettings);
        }
    }
}
