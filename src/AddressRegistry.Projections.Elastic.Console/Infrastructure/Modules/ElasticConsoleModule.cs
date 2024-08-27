namespace AddressRegistry.Projections.Elastic.Console.Infrastructure.Modules
{
    using System;
    using AddressRegistry.Infrastructure;
    using AddressSearch;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Consumer.Read.Municipality;
    using Consumer.Read.Municipality.Infrastructure.Modules;
    using Consumer.Read.Postal;
    using Consumer.Read.Postal.Infrastructure.Modules;
    using Consumer.Read.StreetName;
    using Consumer.Read.StreetName.Infrastructure.Modules;
    using Elastic.Infrastructure;
    using global::Elastic.Clients.Elasticsearch;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ElasticConsoleModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public ElasticConsoleModule(
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

            builder.Populate(_services);
        }

        private void RegisterProjectionSetup(ContainerBuilder builder)
        {
            builder
                .RegisterModule(
                    new EventHandlingModule(
                        typeof(DomainAssemblyMarker).Assembly,
                        EventsJsonSerializerSettingsProvider.CreateSerializerSettings()))
                .RegisterModule<EnvelopeModule>()
                .RegisterEventstreamModule(_configuration)
                .RegisterModule(new ProjectorModule(_configuration));

            RegisterProjections(builder);
        }

        private void RegisterProjections(ContainerBuilder builder)
        {
            //TODO-rik wat moet in de Projector geregistreerd worden zodat daar de progress kan worden opgevraagd?

            builder
                .RegisterModule(
                    new ElasticRunnerModule(
                        _configuration,
                        _services,
                        _loggerFactory))
                .RegisterModule(new ElasticModule(_configuration))
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

            builder
                .Register(c => new ElasticIndex(
                    c.Resolve<ElasticsearchClient>(),
                    _configuration))
                .AsSelf()
                .SingleInstance();
        }
    }
}
