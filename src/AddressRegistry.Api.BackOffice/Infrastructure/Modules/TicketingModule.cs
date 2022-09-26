namespace AddressRegistry.Api.BackOffice.Infrastructure.Modules
{
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using TicketingService.Abstractions;
    using TicketingService.Proxy.HttpProxy;
    using Module = Autofac.Module;

    public sealed class TicketingModule : Module
    {
        internal const string TicketingServiceConfigKey = "TicketingService";

        private readonly string _baseUrl;

        public TicketingModule(
            IConfiguration configuration,
            IServiceCollection services)
        {
            _baseUrl = configuration.GetSection(TicketingServiceConfigKey)["InternalBaseUrl"];
            services
                .AddHttpProxyTicketing(_baseUrl);
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => new TicketingUrl(_baseUrl))
                .As<ITicketingUrl>()
                .SingleInstance();
        }
    }
}
