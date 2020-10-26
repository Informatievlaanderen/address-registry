namespace AddressRegistry.Projector.Infrastructure.Modules
{
    using Address;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Be.Vlaanderen.Basisregisters.Shaperon;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using AddressRegistry.Infrastructure;
    using AddressRegistry.Projections.Extract;
    using AddressRegistry.Projections.Extract.AddressCrabHouseNumberIdExtract;
    using AddressRegistry.Projections.Extract.AddressCrabSubaddressIdExtract;
    using AddressRegistry.Projections.Extract.AddressExtract;
    using AddressRegistry.Projections.LastChangedList;
    using AddressRegistry.Projections.Legacy;
    using AddressRegistry.Projections.Legacy.AddressDetail;
    using AddressRegistry.Projections.Legacy.AddressList;
    using AddressRegistry.Projections.Legacy.AddressSyndication;
    using AddressRegistry.Projections.Legacy.AddressVersion;
    using AddressRegistry.Projections.Legacy.CrabIdToPersistentLocalId;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Microsoft.Extensions.Options;
    using NetTopologySuite.IO;

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
            builder.RegisterModule(new DataDogModule(_configuration));
            RegisterProjectionSetup(builder);

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
            RegisterExtractProjections(builder);
            RegisterLastChangedProjections(builder);
            RegisterLegacyProjections(builder);
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
                    RetryPolicy.NoRetries)
                .RegisterProjections<AddressLinkExtractProjection, ExtractContext>(
                    context => new AddressLinkExtractProjection(context.Resolve<IOptions<ExtractConfig>>(), DbaseCodePage.Western_European_ANSI.ToEncoding(), new WKBReader()),
                    RetryPolicy.NoRetries)
                .RegisterProjections<AddressCrabHouseNumberIdExtractProjection, ExtractContext>(
                    context => new AddressCrabHouseNumberIdExtractProjection(DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    RetryPolicy.NoRetries)
                .RegisterProjections<AddressCrabSubaddressIdExtractProjection, ExtractContext>(
                    context => new AddressCrabSubaddressIdExtractProjection(DbaseCodePage.Western_European_ANSI.ToEncoding()),
                    RetryPolicy.NoRetries);
        }

        private void RegisterLastChangedProjections(ContainerBuilder builder)
        {
            builder.RegisterModule(
                new LastChangedListModule(
                    _configuration.GetConnectionString("LastChangedList"),
                    _configuration["DataDog:ServiceName"],
                    _services,
                    _loggerFactory));

            builder
                .RegisterProjectionMigrator<AddressRegistry.Projections.LastChangedList.LastChangedListContextMigrationFactory>(
                    _configuration,
                    _loggerFactory)
                .RegisterProjections<LastChangedListProjections, LastChangedListContext>(RetryPolicy.NoRetries);
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
                    () => new AddressDetailProjections(WKBReaderFactory.Create()),
                    RetryPolicy.NoRetries)
                .RegisterProjections<AddressListProjections, LegacyContext>(RetryPolicy.NoRetries)
                .RegisterProjections<AddressSyndicationProjections, LegacyContext>(
                    () => new AddressSyndicationProjections(),
                    RetryPolicy.NoRetries)
                .RegisterProjections<AddressVersionProjections, LegacyContext>(
                    () => new AddressVersionProjections(),
                    RetryPolicy.NoRetries)
                .RegisterProjections<CrabIdToPersistentLocalIdProjections, LegacyContext>(RetryPolicy.NoRetries);
        }
    }
}
