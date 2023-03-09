namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework
{
    using System;
    using System.IO;
    using AddressRegistry.Api.Legacy.Infrastructure.Modules;
    using AddressRegistry.Api.Legacy.Infrastructure.Options;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Destructurama;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;
    using Serilog;
    using Serilog.Debugging;
    using Xunit.Abstractions;

    public abstract class IocBasedTest
    {
        private readonly ITestOutputHelper _output;
        private readonly Lazy<IContainer> _container;

        protected IContainer Container => _container.Value;

        protected IocBasedTest(ITestOutputHelper output)
        {
            _output = output;
            _container = new Lazy<IContainer>(BuildContainer);
        }

        private IContainer BuildContainer()
        {
            var containerBuilder =  new ContainerBuilder();
            ConfigureWebApiMocks(containerBuilder);
            ConfigureMocks(containerBuilder);

            return containerBuilder.Build();
        }

        private void ConfigureWebApiMocks(ContainerBuilder containerBuilder)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .AddEnvironmentVariables()
                .Build();

            var services = new ServiceCollection();
            SelfLog.Enable(Console.WriteLine);

            Log.Logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration)
                .WriteTo.TestOutput(_output)
                .Enrich.FromLogContext()
                .Enrich.WithMachineName()
                .Enrich.WithThreadId()
                .Enrich.WithEnvironmentUserName()
                .Destructure.JsonNetTypes()
                .CreateLogger();

            services.AddLogging(l => l.AddSerilog(Log.Logger));
            services.AddMemoryCache()
                .Configure<ResponseOptions>(configuration);

            var tempProvider = services.BuildServiceProvider();

            containerBuilder.RegisterModule(new ApiModule(configuration, services, tempProvider.GetService<ILoggerFactory>()));
            containerBuilder.Populate(services);
        }

        protected abstract void ConfigureMocks(ContainerBuilder containerBuilder);
    }
}
