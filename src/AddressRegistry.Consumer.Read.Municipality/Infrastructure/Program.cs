namespace AddressRegistry.Consumer.Read.Municipality.Infrastructure
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Autofac.Features.AttributeFilters;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Sql.EntityFrameworkCore;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Destructurama;
    using Microsoft.Data.SqlClient;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Extensions.Logging;

    public class Program
    {
        protected Program() { }

        public static async Task Main(string[] args)
        {
            AppDomain.CurrentDomain.FirstChanceException += (_, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            Log.Information("Starting AddressRegistry.Consumer.Read.Municipality");

            var host = new HostBuilder()
                .ConfigureAppConfiguration((_, builder) =>
                {
                    builder
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                        .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
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
                        .AddTransient(s => new TraceDbConnection<MunicipalityConsumerContext>(
                            new SqlConnection(hostContext.Configuration.GetConnectionString("ConsumerMunicipality")),
                            hostContext.Configuration["DataDog:ServiceName"]))
                        .AddDbContextFactory<MunicipalityConsumerContext>((provider, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(provider.GetRequiredService<TraceDbConnection<MunicipalityConsumerContext>>(),
                                sqlServerOptions =>
                                {
                                    sqlServerOptions.EnableRetryOnFailure();
                                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerReadMunicipality, Schema.ConsumerReadMunicipality);
                                }), ServiceLifetime.Transient);
                })
                .UseServiceProviderFactory(new AutofacServiceProviderFactory())
                .ConfigureContainer<ContainerBuilder>((hostContext, builder) =>
                {
                    var services = new ServiceCollection();

                    builder
                        .Register(c =>
                        {
                            var bootstrapServers = hostContext.Configuration["Kafka:BootstrapServers"];
                            var topic = $"{hostContext.Configuration["Topic"]}" ?? throw new ArgumentException("Configuration has no municipality Topic.");
                            var suffix = hostContext.Configuration["ConsumerGroupSuffix"];
                            var consumerGroupId = $"AddressRegistry.MunicipalityBosaItemConsumer.{topic}{suffix}";

                            var consumerOptions = new ConsumerOptions(
                                new BootstrapServers(bootstrapServers),
                                new Topic(topic),
                                new ConsumerGroupId(consumerGroupId),
                                EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

                            consumerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                                hostContext.Configuration["Kafka:SaslUserName"],
                                hostContext.Configuration["Kafka:SaslPassword"]));

                            return new Consumer(consumerOptions, c.Resolve<ILoggerFactory>());
                        })
                        .Keyed<IConsumer>(nameof(MunicipalityBosaItemConsumer))
                        .SingleInstance();

                    builder
                        .Register(c =>
                        {
                            var bootstrapServers = hostContext.Configuration["Kafka:BootstrapServers"];
                            var topic = $"{hostContext.Configuration["Topic"]}" ?? throw new ArgumentException("Configuration has no municipality Topic.");
                            var suffix = hostContext.Configuration["ConsumerGroupSuffix"];
                            var consumerGroupId = $"AddressRegistry.MunicipalityLatestItemConsumer.{topic}{suffix}";

                            var consumerOptions = new ConsumerOptions(
                                new BootstrapServers(bootstrapServers),
                                new Topic(topic),
                                new ConsumerGroupId(consumerGroupId),
                                EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

                            consumerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                                hostContext.Configuration["Kafka:SaslUserName"],
                                hostContext.Configuration["Kafka:SaslPassword"]));

                            return new Consumer(consumerOptions, c.Resolve<ILoggerFactory>());
                        })
                        .Keyed<IConsumer>(nameof(MunicipalityLatestItemConsumer))
                        .SingleInstance();

                    builder.RegisterModule(new DataDogModule(hostContext.Configuration));

                    builder
                        .RegisterType<MunicipalityBosaItemConsumer>()
                        .As<IHostedService>()
                        .WithAttributeFiltering()
                        .SingleInstance();

                    builder
                        .RegisterType<MunicipalityLatestItemConsumer>()
                        .As<IHostedService>()
                        .WithAttributeFiltering()
                        .SingleInstance();

                    builder.Populate(services);
                })
                .UseConsoleLifetime()
                .Build();

            var logger = host.Services.GetRequiredService<ILogger<Program>>();
            var loggerFactory = host.Services.GetRequiredService<ILoggerFactory>();
            var configuration = host.Services.GetRequiredService<IConfiguration>();

            try
            {
                await DistributedLock<Program>.RunAsync(
                        async () =>
                        {
                            await MigrationsHelper.RunAsync(
                                configuration.GetConnectionString("ConsumerMunicipalityAdmin"),
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
