namespace AddressRegistry.Consumer.Read.Municipality.Infrastructure.Modules
{
    using System;
    using Autofac;
    using Destructurama;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
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
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentUserName()
                .Destructure.JsonNetTypes()
                .CreateLogger();

            services.AddLogging(l =>
            {
                l.ClearProviders();
                l.AddSerilog(Log.Logger);
            });
        }
    }
}
