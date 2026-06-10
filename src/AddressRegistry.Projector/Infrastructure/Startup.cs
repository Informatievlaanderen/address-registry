namespace AddressRegistry.Projector.Infrastructure
{
    using System;
    using System.Linq;
    using System.Reflection;
    using System.Threading;
    using AddressRegistry.Infrastructure.Elastic;
    using AddressRegistry.Infrastructure.Modules;
    using AddressRegistry.Projections.Elastic;
    using AddressRegistry.Projections.Elastic.AddressList;
    using AddressRegistry.Projections.Elastic.AddressSearch;
    using AddressRegistry.Projections.Extract;
    using AddressRegistry.Projections.Integration.Infrastructure;
    using AddressRegistry.Projections.Legacy;
    using AddressRegistry.Projections.Wfs;
    using AddressRegistry.Projections.Wms;
    using Asp.Versioning.ApiExplorer;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Configuration;
    using Elastic.Clients.Elasticsearch;
    using FluentValidation;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Microsoft.OpenApi;
    using Modules;
    using Serilog;
    using Serilog.Extensions.Logging;
    using HealthStatus = Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus;

    /// <summary>Represents the startup process for the application.</summary>
    public class Startup
    {
        private const string DatabaseTag = "db";

        private readonly IConfiguration _configuration;
        private readonly ILoggerFactory _loggerFactory = new SerilogLoggerFactory(Log.Logger);

        private readonly CancellationTokenSource _projectionsCancellationTokenSource = new CancellationTokenSource();

        public Startup(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>Configures services for the application.</summary>
        /// <param name="services">The collection of services to configure the application with.</param>
        public void ConfigureServices(IServiceCollection services)
        {
            var baseUrl = _configuration.GetValue<string>("BaseUrl");
            var baseUrlForExceptions = baseUrl.EndsWith("/")
                ? baseUrl.Substring(0, baseUrl.Length - 1)
                : baseUrl;

            services
                .ConfigureDefaultForApi<Startup>(new StartupConfigureOptions
                {
                    Cors =
                    {
                        Origins = _configuration
                            .GetSection("Cors")
                            .GetChildren()
                            .Select(c => c.Value)
                            .ToArray()
                    },
                    Server =
                    {
                        BaseUrl = baseUrlForExceptions
                    },
                    Swagger =
                    {
                        ApiInfo = (provider, description) => new OpenApiInfo
                        {
                            Version = description.ApiVersion.ToString(),
                            Title = "Basisregisters Vlaanderen Address Registry API",
                            Description = GetApiLeadingText(description),
                            Contact = new OpenApiContact
                            {
                                Name = "Digitaal Vlaanderen",
                                Email = "digitaal.vlaanderen@vlaanderen.be",
                                Url = new Uri("https://legacy.basisregisters.vlaanderen")
                            }
                        },
                        XmlCommentPaths = new[] { $"{typeof(Startup).GetTypeInfo().Assembly.GetName().Name}" }
                    },
                    MiddlewareHooks =
                    {
                        AfterHealthChecks = health =>
                        {
                            var connectionStrings = _configuration
                                .GetSection("ConnectionStrings")
                                .GetChildren();

                            if (!_configuration.GetSection("Integration").GetValue("Enabled", false))
                                connectionStrings = connectionStrings
                                    .Where(x => !x.Key.StartsWith("Integration", StringComparison.OrdinalIgnoreCase))
                                    .ToList();


                            foreach (var connectionString in connectionStrings.Where(x => !x.Value.Contains("host", StringComparison.OrdinalIgnoreCase)))
                            {
                                health.AddSqlServer(
                                    connectionString.Value,
                                    name: $"sqlserver-{connectionString.Key.ToLowerInvariant()}",
                                    tags: new[] { DatabaseTag, "sql", "sqlserver" });
                            }

                            foreach (var connectionString in connectionStrings.Where(x => x.Value.Contains("host", StringComparison.OrdinalIgnoreCase)))
                                health.AddNpgSql(
                                    connectionString.Value,
                                    name: $"npgsql-{connectionString.Key.ToLowerInvariant()}",
                                    tags: new[] { DatabaseTag, "sql", "npgsql" });

                            health.AddDbContextCheck<ExtractContext>(
                                $"dbcontext-{nameof(ExtractContext).ToLowerInvariant()}",
                                tags: new[] { DatabaseTag, "sql", "sqlserver" });

                            health.AddDbContextCheck<LegacyContext>(
                                $"dbcontext-{nameof(LegacyContext).ToLowerInvariant()}",
                                tags: new[] { DatabaseTag, "sql", "sqlserver" });

                            health.AddDbContextCheck<LastChangedListContext>(
                                $"dbcontext-{nameof(LastChangedListContext).ToLowerInvariant()}",
                                tags: new[] { DatabaseTag, "sql", "sqlserver" });

                            health.AddDbContextCheck<WfsContext>(
                                $"dbcontext-{nameof(WfsContext).ToLowerInvariant()}",
                                tags: new[] { DatabaseTag, "sql", "sqlserver" });

                            health.AddDbContextCheck<WmsContext>(
                                $"dbcontext-{nameof(WmsContext).ToLowerInvariant()}",
                                tags: new[] { DatabaseTag, "sql", "sqlserver" });

                            health.AddDbContextCheck<ElasticRunnerContext>(
                                $"dbcontext-{nameof(ElasticRunnerContext).ToLowerInvariant()}",
                                tags: new[] { DatabaseTag, "sql", "sqlserver" });

                            health.AddCheck<ProjectionsHealthCheck>(
                                "projections",
                                failureStatus: HealthStatus.Unhealthy,
                                tags: ["projections"]);
                        }
                    }
                })
                .AddValidatorsFromAssemblyContaining<Startup>()
                .Configure<ExtractConfig>(_configuration.GetSection("Extract"))
                .Configure<IntegrationOptions>(_configuration.GetSection("Integration"))
                .Configure<ChangeFeedConfig>(_configuration.GetSection("AddressFeed"));

            services.AddSingleton<ProjectionsHealthCheck>(
                c => new ProjectionsHealthCheck(
                    new AllUnhealthyProjectionsHealthCheckStrategy
                        (c.GetRequiredService<IConnectedProjectionsManager>()), _loggerFactory));

            services.AddHostedService(
                c => new ProjectionAutoRestartBackgroundService(
                    c.GetRequiredService<IConnectedProjectionsManager>(), _loggerFactory));
        }

        public void Configure(
            IServiceProvider serviceProvider,
            IApplicationBuilder app,
            IWebHostEnvironment env,
            IHostApplicationLifetime appLifetime,
            ILoggerFactory loggerFactory,
            IApiVersionDescriptionProvider apiVersionProvider,
            HealthCheckService healthCheckService)
        {
            StartupHelpers.CheckDatabases(healthCheckService, DatabaseTag, loggerFactory).GetAwaiter().GetResult();

            app
                .UseDefaultForApi(new StartupUseOptions
                {
                    Common =
                    {
                        ApplicationContainer = serviceProvider.GetAutofacRoot(),
                        ServiceProvider = serviceProvider,
                        HostingEnvironment = env,
                        ApplicationLifetime = appLifetime,
                        LoggerFactory = loggerFactory
                    },
                    Api =
                    {
                        VersionProvider = apiVersionProvider,
                        Info = groupName => $"Basisregisters.Vlaanderen - Address Information Registry API {groupName}",
                        CSharpClientOptions =
                        {
                            ClassName = "AddressRegistryProjector",
                            Namespace = "Be.Vlaanderen.Basisregisters"
                        },
                        TypeScriptClientOptions =
                        {
                            ClassName = "AddressRegistryProjector"
                        }
                    },
                    Server =
                    {
                        PoweredByName = "Vlaamse overheid - Basisregisters Vlaanderen",
                        ServerName = "Digitaal Vlaanderen"
                    },
                    MiddlewareHooks =
                    {
                        AfterMiddleware = x => x.UseMiddleware<AddNoCacheHeadersMiddleware>()
                    }
                });

            appLifetime.ApplicationStopping.Register(() => _projectionsCancellationTokenSource.Cancel());
            appLifetime.ApplicationStarted.Register(() =>
            {
                var projectionsManager = serviceProvider.GetRequiredService<IConnectedProjectionsManager>();
                projectionsManager.Resume(_projectionsCancellationTokenSource.Token);
            });

            var elasticIndices = new ElasticIndexBase[]
            {
                new AddressSearchElasticIndex(
                    serviceProvider.GetRequiredService<ElasticsearchClient>(),
                    _configuration),
                new AddressListElasticIndex(
                    serviceProvider.GetRequiredService<ElasticsearchClient>(),
                    _configuration)
            };
            foreach (var elasticIndex in elasticIndices)
            {
                elasticIndex.CreateIndexIfNotExist(_projectionsCancellationTokenSource.Token).GetAwaiter().GetResult();
                elasticIndex.CreateAliasIfNotExist(_projectionsCancellationTokenSource.Token).GetAwaiter().GetResult();
            }
        }

        private static string GetApiLeadingText(ApiVersionDescription description)
            => $"Momenteel leest u de documentatie voor versie {description.ApiVersion} van de Basisregisters Vlaanderen Address Registry API{string.Format(description.IsDeprecated ? ", **deze API versie is niet meer ondersteund * *." : ".")}";
    }
}
