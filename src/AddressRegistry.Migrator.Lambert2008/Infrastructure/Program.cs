namespace AddressRegistry.Migrator.Lambert2008.Infrastructure
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Be.Vlaanderen.Basisregisters.Aws.DistributedMutex;
    using Destructurama;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Modules;
    using Polly;
    using Serilog;
    using Serilog.Debugging;
    using Serilog.Extensions.Logging;

    public sealed class Program
    {
        private Program()
        { }

        public static async Task Main(string[] args)
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var ct = cancellationTokenSource.Token;

            var closing = new AutoResetEvent(false);
            ct.Register(() => closing.Set());
            Console.CancelKeyPress += (_, _) => cancellationTokenSource.Cancel();

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

            Log.Information("Starting AddressRegistry.Migrator.Lambert2008");

            try
            {
                var watch = Stopwatch.StartNew();

                var converter = new StreamConverter(
                    container.GetRequiredService<ILoggerFactory>(),
                    configuration,
                    container.GetRequiredService<ILifetimeScope>());

                await DistributedLock<Program>.RunAsync(
                    async () =>
                    {
                        await Policy
                            .Handle<SqlException>()
                            .WaitAndRetryAsync(
                                20,
                                _ => TimeSpan.FromSeconds(60),
                                (_, timespan) => Log.Information(
                                    "SqlException occurred, retrying after {Seconds} seconds.", timespan.Seconds))
                            .ExecuteAsync(async () => await converter.ProcessAsync(ct));

                        watch.Stop();
                        Log.Information("Conversion finished, time elapsed '{Elapsed:g}'.", watch.Elapsed);
                    },
                    DistributedLockOptions.LoadFromConfiguration(configuration),
                    container.GetRequiredService<ILogger<Program>>());
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "Encountered a fatal exception, exiting program.");
                await Log.CloseAndFlushAsync();

                // Allow some time for flushing before shutdown.
                await Task.Delay(1000, CancellationToken.None);
                throw;
            }

            Log.Information("Stopping...");
            closing.Close();
        }

        private static IServiceProvider ConfigureServices(IConfiguration configuration)
        {
            SelfLog.Enable(Console.WriteLine);

            Log.Logger = new LoggerConfiguration() //NOSONAR logging configuration is safe
                .ReadFrom.Configuration(configuration)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentUserName()
                .Destructure.JsonNetTypes()
                .CreateLogger();

            var services = new ServiceCollection();
            var loggerFactory = new SerilogLoggerFactory(Log.Logger); //NOSONAR logging configuration is safe
            services.AddSingleton<ILoggerFactory>(loggerFactory);
            services.AddLogging(builder => builder.AddSerilog(Log.Logger));

            var builder = new ContainerBuilder();
            builder.RegisterModule(new ApiModule(configuration, services, loggerFactory));

            return new AutofacServiceProvider(builder.Build());
        }
    }
}
