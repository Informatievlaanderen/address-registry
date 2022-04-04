namespace AddressRegistry.Consumer.Infrastructure
{
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Simple;
    using Be.Vlaanderen.Basisregisters.Projector.ConnectedProjections;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Serilog;

    public class Program
    {
        private static readonly AutoResetEvent closing = new AutoResetEvent(false);
        private static readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        protected Program()
        { }

        public static async Task Main(string[] args)
        {
            var cancellationToken = cancellationTokenSource.Token;

            cancellationToken.Register(() => closing.Set());
            Console.CancelKeyPress += (sender, eventArgs) => cancellationTokenSource.Cancel();

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception) eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var container = ConfigureServices(configuration);

            Log.Information("Starting AddressRegistry.Consumer");

            try
            {
                await DistributedLock<Program>.RunAsync(
                    async () =>
                    {
                        try
                        {
                            var loggerFactory = container.GetRequiredService<ILoggerFactory>();

                            var bootstrapServers = configuration["Kafka:BootstrapServers"];
                            var kafkaOptions = new KafkaOptions(bootstrapServers, configuration["Kafka:SaslUserName"], configuration["Kafka:SaslPassword"], EventsJsonSerializerSettingsProvider.CreateSerializerSettings());

                            var topic = $"{configuration["MunicipalityTopic"]}" ?? throw new ArgumentException("Configuration has no MunicipalityTopic.");
                            var consumerGroupSuffix = configuration["MunicipalityConsumerGroupSuffix"];
                            var consumerOptions = new ConsumerOptions(topic, consumerGroupSuffix);

                            var actualContainer = container.GetRequiredService<ILifetimeScope>();

                            var projectionsManager = actualContainer.Resolve<IConnectedProjectionsManager>();
                            var projectionsTask = projectionsManager.Start(cancellationToken);

                            var consumer = new Consumer(actualContainer, loggerFactory, kafkaOptions, consumerOptions);
                            var consumerTask = consumer.Start(cancellationToken);

                            await Task.WhenAll(projectionsTask, consumerTask);
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
            closing.Close();
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            var services = new ServiceCollection();
            var builder = new ContainerBuilder();

            builder.RegisterModule(new LoggingModule(configuration, services));

            var tempProvider = services.BuildServiceProvider();
            var loggerFactory = tempProvider.GetRequiredService<ILoggerFactory>();

            builder.RegisterModule(new ApiModule(configuration, services, loggerFactory));

            builder.RegisterModule(new ProjectorModule(configuration));

            builder.Populate(services);

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
