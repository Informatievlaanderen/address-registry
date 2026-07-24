namespace AddressRegistry.Api.Oslo.Infrastructure.Modules
{
    using Address.V2.Count;
    using Address.V2.Detail;
    using Address.V2.List;
    using Address.V2.Sync;
    using Address.V3.Detail;
    using Address.V3.List;
    using AddressMatch.Matching;
    using AddressMatch.V2.Requests;
    using AddressMatch.V2.Responses;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Municipality;
    using Consumer.Read.StreetName;
    using Elastic.List;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Options;
    using Projections.Legacy;
    using AddressDetailOsloRequest = Address.V2.Detail.AddressDetailOsloRequest;
    using AddressListOsloElasticHandler = Address.V2.List.AddressListOsloElasticHandler;
    using AddressListOsloRequest = Address.V2.List.AddressListOsloRequest;
    using V2 = Address.V2;
    using V2Match = AddressMatch.V2;
    using V3 = Address.V3;

    public sealed class MediatRModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder
                .RegisterType<Mediator>()
                .As<IMediator>()
                .InstancePerLifetimeScope();

            builder.RegisterType<AddressSyndicationHandler>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<AddressListOsloRequest, AddressListOsloResponse>)
                new AddressListOsloElasticHandler(
                    c.Resolve<IAddressApiListElasticsearchClient>(),
                    c.Resolve<IOptions<ResponseOptionsV2>>()))
                .InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<V3.List.AddressListOsloRequest, AddressListOsloV3Response>)
                new V3.List.AddressListOsloElasticHandler(
                    c.Resolve<IAddressApiListElasticsearchClient>(),
                    c.Resolve<IOptions<ResponseOptionsV3>>()))
                .InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<AddressDetailOsloRequest, AddressDetailOsloResponse>)
                new AddressDetailOsloHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<MunicipalityConsumerContext>(),
                    c.Resolve<StreetNameConsumerContext>(),
                    c.Resolve<IOptions<ResponseOptionsV2>>()))
                .InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<V3.Detail.AddressDetailOsloRequest, AddressDetailOsloV3Response>)
                new AddressDetailOsloHandler(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<MunicipalityConsumerContext>(),
                    c.Resolve<StreetNameConsumerContext>(),
                    c.Resolve<IOptions<ResponseOptionsV3>>()))
                .InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<AddressCountRequest, TotaalAantalResponse>)
                new AddressCountElasticHandler(c.Resolve<IAddressApiListElasticsearchClient>()))
                .InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<V3.Count.AddressCountRequest, Be.Vlaanderen.Basisregisters.GrAr.Oslo.TotaalAantalResponse>)
                new V3.Count.AddressCountElasticHandler(c.Resolve<IAddressApiListElasticsearchClient>()))
                .InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<AddressMatchRequest, AddressMatchOsloCollection>)
                new V2Match.AddressMatchHandlerV2(
                    c.Resolve<ILatestQueries>(),
                    c.Resolve<IOptions<ResponseOptionsV2>>()))
                .InstancePerLifetimeScope();
        }
    }
}
