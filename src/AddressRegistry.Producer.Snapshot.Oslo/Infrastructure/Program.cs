namespace AddressRegistry.Producer.Snapshot.Oslo.Infrastructure
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Destructurama;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Extensions.Logging;

    public sealed class Program
    {
        private Program()
        { }

        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.FirstChanceException += (_, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            Log.Information("Initializing AddressRegistry.Producer.Snapshot.Oslo");

            var host = new HostBuilder()
                .ConfigureAppConfiguration((_, configurationBuilder) =>
                {
                    configurationBuilder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                        .AddEnvironmentVariables()
                        .AddCommandLine(args);
                })
                .ConfigureLogging((hostContext, loggingBuilder) =>
                {
                    SelfLog.Enable(Console.WriteLine);
                    Log.Logger = new LoggerConfiguration() //NOSONAR logging configuration is safe
                        .ReadFrom.Configuration(hostContext.Configuration)
                        .Enrich.FromLogContext()
                        .Enrich.WithMachineName()
                        .Enrich.WithThreadId()
                        .Enrich.WithEnvironmentUserName()
                        .Destructure.JsonNetTypes()
                        .CreateLogger();
                    loggingBuilder.ClearProviders();
                    loggingBuilder.AddSerilog(Log.Logger);
                })
                .ConfigureServices((hostContext, services) =>
                {
                    var healthChecksBuilder = services.AddHealthChecks();
                    var connectionStrings = hostContext.Configuration
                        .GetSection("ConnectionStrings")
                        .GetChildren();

                    foreach (var connectionString in connectionStrings
                                 .Where(x => !x.Value.Contains("host", StringComparison.OrdinalIgnoreCase)))
                    {
                        healthChecksBuilder.AddSqlServer(
                            connectionString.Value,
                            name: $"sqlserver-{connectionString.Key.ToLowerInvariant()}",
                            tags: new[] { "db", "sql", "sqlserver" });
                    }

                    foreach (var connectionString in connectionStrings
                                 .Where(x => x.Value.Contains("host", StringComparison.OrdinalIgnoreCase)))
                    {
                        healthChecksBuilder.AddNpgSql(
                            connectionString.Value,
                            name: $"npgsql-{connectionString.Key.ToLowerInvariant()}",
                            tags: new[] { "db", "sql", "npgsql" });
                    }

                    healthChecksBuilder.AddDbContextCheck<ProducerContext>(
                        $"dbcontext-{nameof(ProducerContext).ToLowerInvariant()}",
                        tags: new[] { "db", "sql", "sqlserver" });

                    var origins = hostContext.Configuration
                        .GetSection("Cors")
                        .GetChildren()
                        .Select(c => c.Value)
                        .ToArray();

                    foreach (var origin in origins)
                    {
                        services.AddCors(options =>
                        {
                            options.AddDefaultPolicy(builder =>
                            {
                                builder.WithOrigins(origin);
                            });
                        });
                    }
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
                {
                    var services = new ServiceCollection();
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger);

                    builder.RegisterModule(new ProducerModule(hostContext.Configuration, services, loggerFactory));

                    builder
                        .RegisterType<SnapshotProducer>()
                        .As<IHostedService>()
                        .SingleInstance();

                    builder.Populate(services);
                })
                .ConfigureWebHostDefaults(webHostBuilder =>
                    webHostBuilder
                        .UseStartup<Startup>()
                        .UseKestrel())
                .UseConsoleLifetime()
                .Build();

            Log.Information("Starting AddressRegistry.Producer.Snapshot.Oslo");

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            try
            {
                await DistributedLock<Program>.RunAsync(
                        async () => { await host.RunAsync().ConfigureAwait(false); },
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
