namespace AddressRegistry.Consumer.Read.Postal.Infrastructure
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Autofac.Features.AttributeFilters;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Consumer;
    using Destructurama;
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

            Log.Information("Starting AddressRegistry.Consumer.Read.Postal");

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

                    Log.Logger = new LoggerConfiguration() //NOSONAR logging configuration is safe
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
                    var loggerFactory = new SerilogLoggerFactory(Log.Logger); //NOSONAR logging configuration is safe

                    services
                        .AddDbContextFactory<PostalConsumerContext>((provider, options) => options
                            .UseLoggerFactory(loggerFactory)
                            .UseSqlServer(hostContext.Configuration.GetConnectionString("ConsumerPostal"),
                                sqlServerOptions =>
                                {
                                    sqlServerOptions.EnableRetryOnFailure();
                                    sqlServerOptions.MigrationsHistoryTable(MigrationTables.ConsumerReadPostal, Schema.ConsumerReadPostal);
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
                            var topic = $"{hostContext.Configuration["Topic"]}" ?? throw new ArgumentException("Configuration has no Postal Topic.");
                            var suffix = hostContext.Configuration["ConsumerGroupSuffix"];
                            var consumerGroupId = $"AddressRegistry.PostalLatestItemConsumer.{topic}{suffix}";

                            var consumerOptions = new ConsumerOptions(
                                new BootstrapServers(bootstrapServers),
                                new Topic(topic),
                                new ConsumerGroupId(consumerGroupId),
                                EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

                            consumerOptions.ConfigureSaslAuthentication(new SaslAuthentication(
                                hostContext.Configuration["Kafka:SaslUserName"],
                                hostContext.Configuration["Kafka:SaslPassword"]));

                            var offset = hostContext.Configuration["ConsumerOffset"];

                            if (!string.IsNullOrWhiteSpace(offset) && long.TryParse(offset, out var result))
                            {
                                var ignoreDataCheck = hostContext.Configuration.GetValue<bool>("IgnoreConsumerOffsetDataCheck", false);

                                if (!ignoreDataCheck)
                                {
                                    using var ctx = c.Resolve<PostalConsumerContext>();

                                    if (ctx.PostalLatestItems.Any())
                                    {
                                        throw new InvalidOperationException(
                                            $"Cannot set Kafka offset to {offset} because {nameof(ctx.PostalLatestItems)} has data.");
                                    }
                                }

                                consumerOptions.ConfigureOffset(new Offset(result));
                            }

                            return new Consumer(consumerOptions, c.Resolve<ILoggerFactory>());
                        })
                        .Keyed<IConsumer>(nameof(PostalLatestItemConsumer))
                        .SingleInstance();

                    builder
                        .RegisterType<PostalLatestItemConsumer>()
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
                                configuration.GetConnectionString("ConsumerPostalAdmin"),
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
