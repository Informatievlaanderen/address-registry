namespace AddressRegistry.Snapshot.Verifier.Infrastructure.Modules
{
    using AddressRegistry.Infrastructure;
    using AddressRegistry.Infrastructure.Modules;
    using Autofac;
    using Autofac.Extensions.DependencyInjection;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Logging;

    public class ApiModule : Module
    {
        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;
        private readonly ILoggerFactory _loggerFactory;

        public ApiModule(
            IConfiguration configuration,
            IServiceCollection services,
            ILoggerFactory loggerFactory)
        {
            _configuration = configuration;
            _services = services;
            _loggerFactory = loggerFactory;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterModule(new CommandHandlingModule(_configuration));

            builder.RegisterSnapshotModule(_configuration);

            builder.Populate(_services);
        }
    }
}
