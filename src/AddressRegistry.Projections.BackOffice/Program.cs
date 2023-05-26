namespace AddressRegistry.Projections.BackOffice
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.Projector;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Destructurama;
    using Infrastructure;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Extensions.Logging;
    using AddressRegistry.Infrastructure;
    using Elastic.Apm.DiagnosticSource;
    using Elastic.Apm.EntityFrameworkCore;
    using Elastic.Apm.Extensions.Hosting;
    using Elastic.Apm.SqlClient;

    public sealed class ProgramLogger { }

    public static class Program
    {
        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.FirstChanceException += (_, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            Log.Information("Starting AddressRegistry.Projections.BackOffice");

            var host = new HostBuilder()
                .ConfigureAppConfiguration((_, builder) =>
                {
                    builder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true,
                            reloadOnChange: false)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((hostContext, builder) =>
                {
                    SelfLog.Enable(Console.WriteLine);

                    Log.Logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithThreadId()
                        .Enrich.WithEnvironmentUserName()
                        .Destructure.JsonNetTypes()
                        .CreateLogger();

                    builder.ClearProviders();
                    builder.AddSerilog(Log.Logger);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger);

                    services
                        .ConfigureBackOfficeProjectionsContext(hostContext.Configuration, loggerFactory)
                        .AddTransient(_ => new TraceDbConnection<BackOfficeContext>(
                            new SqlConnection(hostContext.Configuration.GetConnectionString("BackOffice")),
                            hostContext.Configuration["DataDog:ServiceName"]))
                        .AddDbContextFactory<BackOfficeContext>((provider, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(provider.GetRequiredService<TraceDbConnection<BackOfficeContext>>(), sqlServerOptions => sqlServerOptions
                                .EnableRetryOnFailure()
                                .MigrationsHistoryTable(MigrationTables.BackOffice, Schema.BackOffice)
                            ))
                        .AddHostedService<ProjectorRunner>();
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
                {
                    builder.RegisterModule(
                        new EventHandlingModule(
                            typeof(DomainAssemblyMarker).Assembly,
                            EventsJsonSerializerSettingsProvider.CreateSerializerSettings()
                        ));

                    builder.RegisterModule<EnvelopeModule>();
                    builder.RegisterEventstreamModule(hostContext.Configuration);
                    builder.RegisterModule(new ProjectorModule(hostContext.Configuration));

                    builder.RegisterProjections<BackOfficeProjections, BackOfficeProjectionsContext>(
                        c => new BackOfficeProjections(c.Resolve<IDbContextFactory<BackOfficeContext>>()),
                        ConnectedProjectionSettings.Default);
                })
                .UseElasticApm(
                    new EfCoreDiagnosticsSubscriber(),
                    new HttpDiagnosticsSubscriber(),
                    new SqlClientDiagnosticSubscriber())
                .UseConsoleLifetime()
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<ProgramLogger>>();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            try
            {
                await DistributedLock<ProgramLogger>.RunAsync(
                        async () =>
                        {
                            await Infrastructure.MigrationsHelper.RunAsync(
                                configuration.GetConnectionString("BackOfficeProjectionsAdmin"),
                                loggerFactory,
                                CancellationToken.None);

                            await host.RunAsync().ConfigureAwait(false);
                        },
                        DistributedLockOptions.LoadFromConfiguration(configuration),
                        logger)
                    .ConfigureAwait(false);
            }
            catch (AggregateException aggregateException)
            {
                foreach (var innerException in aggregateException.InnerExceptions)
                {
                    logger.LogCritical(innerException, "Encountered a fatal exception, exiting program.");
                }
            }
            catch (Exception e)
            {
                logger.LogCritical(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                await Task.Delay(500, default);
                throw;
            }
            finally
            {
                logger.LogInformation("Stopping...");
            }
        }
    }
}
