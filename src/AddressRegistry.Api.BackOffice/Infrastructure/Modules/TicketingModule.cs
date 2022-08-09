namespace AddressRegistry.Api.BackOffice.Infrastructure.Modules
{
    using System;
    using Abstractions;
    using Autofac;
    using Microsoft.Extensions.Configuration;
    using TicketingService.Abstractions;
    using TicketingService.Proxy.HttpProxy;

    public class TicketingModule : Module
    {
        private readonly IConfiguration _configuration;

        public TicketingModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            var baseUrl = _configuration["TicketingService:BaseUrl"];
            builder
                .RegisterInstance(new HttpProxyTicketing(baseUrl))
                .As<ITicketing>()
                .SingleInstance();

            var baseUri = new Uri(baseUrl);
            var scheme = baseUri.Scheme;
            var host = $"{baseUri.Host}:{baseUri.Port}";
            var pathBase = baseUri.AbsolutePath;
            builder
                .RegisterInstance(new TicketingUrl(scheme, host, pathBase))
                .As<ITicketingUrl>()
                .SingleInstance();
        }
    }
}
