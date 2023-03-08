namespace AddressRegistry.Api.Legacy.Infrastructure.Modules
{
    using System;
    using Address;
    using Address.Bosa;
    using AddressMatch;
    using AddressMatch.V1;
    using AddressMatch.V1.Matching;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Microsoft;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Be.Vlaanderen.Basisregisters.DependencyInjection;
    using Consumer.Read.Municipality.Infrastructure.Modules;
    using Consumer.Read.StreetName.Infrastructure.Modules;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Projections.Legacy;
    using Projections.Syndication;

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
            var connectionString = configuration.GetConnectionString("SyndicationProjections");
            var buildingConnectionString = configuration.GetConnectionString("BuildingLegacyProjections");

            var hasConnectionString = !string.IsNullOrWhiteSpace(connectionString);
            if (hasConnectionString)
            {
                RunOnSqlServer(configuration, services, loggerFactory, connectionString, buildingConnectionString);
            }
            else
            {
                RunInMemoryDb(services, loggerFactory, logger);
            }

            logger.LogInformation("Added {Context} to services:", nameof(AddressBosaContext));
            logger.LogInformation("Added {Context} to services:", nameof(AddressQueryContext));
            logger.LogInformation("Added {Context} to services:", nameof(AddressMatchContext));
            logger.LogInformation("Added {Context} to services:", nameof(BuildingContext));
        }

        protected override void Load(ContainerBuilder builder)
        {
            _services.RegisterModule(new DataDogModule(_configuration));

            builder
                .RegisterModule(new LegacyModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new SyndicationModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new MunicipalityConsumerModule(_configuration, _services, _loggerFactory))
                .RegisterModule(new StreetNameConsumerModule(_configuration, _services, _loggerFactory));

            builder
                .RegisterAssemblyTypes(typeof(IKadRrService).Assembly)
                .AsImplementedInterfaces();

            builder
                .RegisterType<ProblemDetailsHelper>()
                .AsSelf();

            builder.Populate(_services);
        }

        private static void RunOnSqlServer(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            string backofficeProjectionsConnectionString,
            string buildingProjectionsConnectionString)
        {
            services
                .AddScoped(s => new TraceDbConnection<AddressBosaContext>(
                    new SqlConnection(backofficeProjectionsConnectionString),
                    configuration["DataDog:ServiceName"]))
                .AddDbContext<AddressBosaContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection<AddressBosaContext>>(),
                        sqlServerOptions => { sqlServerOptions.EnableRetryOnFailure(); }))
                .AddScoped(s => new TraceDbConnection<AddressQueryContext>(
                    new SqlConnection(backofficeProjectionsConnectionString),
                    configuration["DataDog:ServiceName"]))
                .AddDbContext<AddressQueryContext>((provider, options) => options
                    .EnableSensitiveDataLogging()
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection<AddressQueryContext>>(),
                        sqlServerOptions => { sqlServerOptions.EnableRetryOnFailure(); }))
                .AddScoped(s => new TraceDbConnection<AddressMatchContext>(
                    new SqlConnection(backofficeProjectionsConnectionString),
                    configuration["DataDog:ServiceName"]))
                .AddDbContext<AddressMatchContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection<AddressMatchContext>>(),
                        sqlServerOptions => { sqlServerOptions.EnableRetryOnFailure(); }))
                .AddScoped(s => new TraceDbConnection<BuildingContext>(
                    new SqlConnection(buildingProjectionsConnectionString),
                    configuration["DataDog:ServiceName"]))
                .AddDbContext<BuildingContext>((provider, options) => options
                    .UseLoggerFactory(loggerFactory)
                    .UseSqlServer(provider.GetRequiredService<TraceDbConnection<BuildingContext>>(),
                        sqlServerOptions => { sqlServerOptions.EnableRetryOnFailure(); }));
        }

        private static void RunInMemoryDb(
            IServiceCollection services,
            ILoggerFactory loggerFactory,
            ILogger<ApiModule> logger)
        {
            services
                .AddDbContext<AddressBosaContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }))
                .AddDbContext<AddressQueryContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }))
                .AddDbContext<AddressMatchContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }))
                .AddDbContext<BuildingContext>(options => options
                    .UseLoggerFactory(loggerFactory)
                    .UseInMemoryDatabase(Guid.NewGuid().ToString(), sqlServerOptions => { }));

            logger.LogWarning("Running InMemory for {Context}!", nameof(AddressBosaContext));
            logger.LogWarning("Running InMemory for {Context}!", nameof(AddressQueryContext));
            logger.LogWarning("Running InMemory for {Context}!", nameof(AddressMatchContext));
            logger.LogWarning("Running InMemory for {Context}!", nameof(BuildingContext));
        }
    }
}
