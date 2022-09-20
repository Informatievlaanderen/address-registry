namespace AddressRegistry.Api.BackOffice.Infrastructure.Modules
{
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TicketingService.Abstractions;
    using TicketingService.Proxy.HttpProxy;

    public class TicketingModule : Module
    {
        private const string TicketingServiceConfigKey = "TicketingService";

        private readonly IConfiguration _configuration;
        private readonly IServiceCollection _services;

        public TicketingModule(
            IConfiguration configuration,
            IServiceCollection services)
        {
            _configuration = configuration;
            _services = services;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var baseUrl = _configuration.GetSection(TicketingServiceConfigKey)["BaseUrl"];
            builder
                .Register(_ => new TicketingUrl(baseUrl))
                .As<ITicketingUrl>()
                .SingleInstance();

            _services
                .AddHttpClient()
                .AddHttpProxyTicketing(baseUrl);
        }
    }
}
