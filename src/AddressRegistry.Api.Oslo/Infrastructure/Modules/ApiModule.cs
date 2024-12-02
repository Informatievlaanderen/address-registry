namespace AddressRegistry.Api.Oslo.Infrastructure.Modules
{
    using System;
    using Address;
    using Address.Search;
    using AddressMatch.V2;
    using AddressMatch.V2.Matching;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Consumer.Read.Municipality.Infrastructure.Modules;
    using Consumer.Read.Postal.Infrastructure.Modules;
    using Consumer.Read.StreetName.Infrastructure.Modules;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Projections.AddressMatch;
    using Projections.Legacy;

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

            var logger = loggerFactory.CreateLogger<ApiModule>();
            var connectionString = configuration.GetConnectionString("LegacyProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(services, loggerFactory, connectionString);
            }
            else
            {
                RunInMemoryDb(services, loggerFactory, logger);
            }

            logger.LogInformation("Added {Context} to services:", nameof(AddressQueryContext));
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterModule(new LegacyModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new AddressMatchModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new PostalConsumerModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new MunicipalityConsumerModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new StreetNameConsumerModule(_configuration, _services, _loggerFactory));

            builder
                .RegisterAssemblyTypes(typeof(ILatestQueries).Assembly)
                .AsImplementedInterfaces();

            builder
                .RegisterType<MunicipalityCache>()
                .As<IMunicipalityCache>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<QueryParser>()
                .SingleInstance();

            builder
                .RegisterType<PostalCache>()
                .As<IPostalCache>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.Populate(_services);
        }

        private static void RunOnSqlServer(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string syndicationConnectionString)
        {
            services
                .AddDbContext<AddressQueryContext>((_, options) => options
                    .EnableSensitiveDataLogging()
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(syndicationConnectionString,
                        sqlServerOptions => { sqlServerOptions.EnableRetryOnFailure(); }))
                .AddDbContext<AddressMatchContextV2>((_, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(syndicationConnectionString,
                        sqlServerOptions => { sqlServerOptions.EnableRetryOnFailure(); }));
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger<ApiModule> logger)
        {
            services
                .AddDbContext<AddressQueryContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }))
                .AddDbContext<AddressMatchContextV2>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(AddressQueryContext));
        }
    }
}
