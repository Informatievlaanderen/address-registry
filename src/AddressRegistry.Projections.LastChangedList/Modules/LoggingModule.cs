namespace AddressRegistry.Projections.LastChangedList.Modules
{
    using System;
    using Autofac;
    using Destructurama;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Serilog;
    using Serilog.Debugging;

    public class LoggingModule : Module
    {
        public LoggingModule(
            IConfiguration configuration,
            IServiceCollection services)
        {
            SelfLog.Enable(Console.WriteLine);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.File(
                    "tracing.log",
                    retainedFileCountLimit: 20,
                    fileSizeLimitBytes: 104857600,
                    rollOnFileSizeLimit: true,
                    rollingInterval: RollingInterval.Day)
                .WriteTo.Console()
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentUserName()
                .Destructure.JsonNetTypes()
                .CreateLogger();

            services.AddLogging(l => l.AddSerilog(Log.Logger));
        }
    }
}
