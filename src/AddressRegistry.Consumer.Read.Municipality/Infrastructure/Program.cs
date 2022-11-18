namespace AddressRegistry.Consumer.Read.Municipality.Infrastructure
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Infrastructure.Modules;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.DataDog.Tracing.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Serilog;

    public class Program
    {
        private static readonly AutoResetEvent Closing = new AutoResetEvent(false);
        private static readonly CancellationTokenSource CancellationTokenSource = new CancellationTokenSource();

        protected Program()
        { }

        public static async Task Main(string[] args)
        {
            var cancellationToken = CancellationTokenSource.Token;

            cancellationToken.Register(() => Closing.Set());
            Console.CancelKeyPress += (_, _) => CancellationTokenSource.Cancel();

            AppDomain.CurrentDomain.FirstChanceException += (_, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (_, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var container = ConfigureServices(configuration);

            Log.Information("Starting AddressRegistry.Consumer.Read.Municipality");

            try
            {
                await DistributedLock<Program>.RunAsync(
                async () =>
                {
                    try
                    {
                        var loggerFactory = container.GetRequiredService<ILoggerFactory>();

                        await MigrationsHelper.RunAsync(configuration.GetConnectionString("ConsumerMunicipalityAdmin"),
                            loggerFactory, cancellationToken);

                        var bootstrapServers = configuration["Kafka:BootstrapServers"];
                        var kafkaOptions = new KafkaOptions(bootstrapServers, configuration["Kafka:SaslUserName"],
                            configuration["Kafka:SaslPassword"],
                            EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

                        var topic = $"{configuration["Topic"]}" ??
                                    throw new ArgumentException("Configuration has no Municipality Topic.");
                        var consumerGroupSuffix = configuration["ConsumerGroupSuffix"];
                        var consumerOptions = new MunicipalityConsumerOptions(topic, consumerGroupSuffix);

                        var actualContainer = container.GetRequiredService<ILifetimeScope>();

                        var latestItemConsumerTask =
                            new MunicipalityLatestItemConsumer(actualContainer, kafkaOptions, consumerOptions)
                                .Start(cancellationToken);

                        var bosaItemConsumerTask =
                            new MunicipalityBosaItemConsumer(actualContainer, kafkaOptions, consumerOptions)
                                .Start(cancellationToken);

                        Log.Information("The kafka consumer municipality was started");

                        await Task.WhenAll(latestItemConsumerTask, bosaItemConsumerTask);

                        Log.Information(
                            $"Consumer latest item municipality task stopped with status: {latestItemConsumerTask.Status}");
                        Log.Information(
                            $"Consumer bosa item municipality task stopped with status: {bosaItemConsumerTask.Status}");
                    }
                    catch (Exception e)
                    {
                        Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                        throw;
                    }
                },
                DistributedLockOptions.LoadFromConfiguration(configuration),
                container.GetService<ILogger<Program>>()!);
            }
            catch (Exception e)
            {
                Log.Fatal(e, "Encountered a fatal exception, exiting program.");
                Log.CloseAndFlush();

                // Allow some time for flushing before shutdown.
                await Task.Delay(1000, default);
                throw;
            }

            Log.Information("Stopping...");
            Closing.Close();
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule(configuration, services));

            var tempProvider = services.BuildServiceProvider();
            var loggerFactory = tempProvider.GetRequiredService<ILoggerFactory>();

            builder.RegisterModule(new DataDogModule(configuration));
            builder.RegisterModule(new MunicipalityConsumerModule(configuration, services, loggerFactory, ServiceLifetime.Transient));

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
