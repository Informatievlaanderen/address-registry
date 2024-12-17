namespace AddressRegistry.Api.Oslo.Infrastructure.Modules
{
    using Address;
    using Address.Count;
    using Address.Detail;
    using Address.List;
    using Address.Sync;
    using AddressMatch.Requests;
    using AddressMatch.Responses;
    using AddressMatch.V2;
    using AddressMatch.V2.Matching;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Municipality;
    using Consumer.Read.StreetName;
    using Elastic;
    using Elastic.List;
    using MediatR;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Options;
    using Options;
    using Projections.Legacy;

    public sealed class MediatRModule : Module
    {
        private readonly IConfiguration _configuration;

        public MediatRModule(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterType<AddressSyndicationHandler>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            if (_configuration.GetValue("UseElasticForList", defaultValue:false))
            {
                builder.Register(c => (IRequestHandler<AddressListOsloRequest, AddressListOsloResponse>)
                    new AddressListOsloElasticHandler(
                        c.Resolve<IAddressApiListElasticsearchClient>(),
                        c.Resolve<IOptions<ResponseOptions>>()))
                    .InstancePerLifetimeScope();
            }
            else
            {
                builder.Register(c => (IRequestHandler<AddressListOsloRequest, AddressListOsloResponse>)
                    new AddressListOsloHandlerV2(
                        c.Resolve<AddressQueryContext>(),
                        c.Resolve<IOptions<ResponseOptions>>()))
                    .InstancePerLifetimeScope();
            }

            builder.Register(c => (IRequestHandler<AddressDetailOsloRequest, AddressDetailOsloResponse>)
                new AddressDetailOsloHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<MunicipalityConsumerContext>(),
                    c.Resolve<StreetNameConsumerContext>(),
                    c.Resolve<IOptions<ResponseOptions>>()))
                .InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<AddressCountRequest, TotaalAantalResponse>)
                new AddressCountOsloHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<AddressQueryContext>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<AddressMatchRequest, AddressMatchOsloCollection>)
                new AddressMatchHandlerV2(
                    c.Resolve<ILatestQueries>(),
                    c.Resolve<IOptions<ResponseOptions>>()))
                .InstancePerLifetimeScope();
        }
    }
}
