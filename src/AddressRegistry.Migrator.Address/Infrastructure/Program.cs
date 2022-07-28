namespace AddressRegistry.Migrator.Address.Infrastructure
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Be.Vlaanderen.Basisregisters.Projector.Modules;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Polly;
    using Serilog;

    public class Program
    {
        protected Program()
        { }
        
        public static async Task Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var ct = cancellationTokenSource.Token;

            var closing = new AutoResetEvent(false);
            ct.Register(() => closing.Set());
            Console.CancelKeyPress += (sender, eventArgs) => cancellationTokenSource.Cancel();

            AppDomain.CurrentDomain.FirstChanceException += (sender, eventArgs) =>
                Log.Debug(
                    eventArgs.Exception,
                    "FirstChanceException event raised in {AppDomain}.",
                    AppDomain.CurrentDomain.FriendlyName);

            AppDomain.CurrentDomain.UnhandledException += (sender, eventArgs) =>
                Log.Fatal((Exception)eventArgs.ExceptionObject, "Encountered a fatal exception, exiting program.");

            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .AddCommandLine(args)
                .Build();

            var container = ConfigureServices(configuration);

            Log.Information("Starting AddressRegistry.Migrator");

            try
            {
                var watch = Stopwatch.StartNew();

                var migrator = new StreamMigrator(
                    container.GetRequiredService<ILoggerFactory>(),
                    configuration,
                    container.GetRequiredService<ILifetimeScope>());

                await DistributedLock<Program>.RunAsync(
                    async () =>
                    {
                        try
                        {
                            await Policy
                                .Handle<SqlException>()
                                .WaitAndRetryAsync(10, _ => TimeSpan.FromSeconds(60),
                                    (_, timespan) =>
                                        Log.Information($"SqlException occurred retrying after {timespan.Seconds} seconds."))
                                .ExecuteAsync(async () =>
                                {
                                    await migrator.ProcessAsync(ct);
                                });

                            watch.Stop();
                            Log.Information($"Migration finished, time elapsed '{watch.Elapsed:g}'");
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
