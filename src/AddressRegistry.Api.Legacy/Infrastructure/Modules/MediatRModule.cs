namespace AddressRegistry.Api.Legacy.Infrastructure.Modules
{
    using Address;
    using Address.Bosa;
    using Address.BosaRepresentation;
    using Address.Count;
    using Address.Detail;
    using Address.List;
    using Address.Sync;
    using AddressMatch.Requests;
    using AddressMatch.Responses;
    using AddressMatch.V1;
    using AddressMatch.V1.Matching;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Municipality;
    using Consumer.Read.StreetName;
    using CrabHouseNumber;
    using CrabSubaddress;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Options;
    using Projections.Legacy;

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

            builder.RegisterType<CrabHouseNumberAddressHandler>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<CrabHouseNumberCountHandler>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<CrabSubaddressHandler>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.RegisterType<CrabSubaddressCountHandler>()
                .AsImplementedInterfaces()
                .InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<AddressListRequest, AddressListResponse>)
                new AddressListHandlerV2(
                    c.Resolve<AddressQueryContext>(),
                    c.Resolve<IOptions<ResponseOptions>>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<AddressDetailRequest, AddressDetailResponse>)
                new AddressDetailHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<MunicipalityConsumerContext>(),
                    c.Resolve<StreetNameConsumerContext>(),
                    c.Resolve<IOptions<ResponseOptions>>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<AddressCountRequest, TotaalAantalResponse>)
                new AddressCountHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<AddressQueryContext>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<AddressBosaRequest, AddressBosaResponse>)
                new AddressBosaHandlerV2(
                    c.Resolve<AddressBosaContext>(),
                    c.Resolve<IOptions<ResponseOptions>>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<AddressRepresentationBosaRequest, AddressRepresentationBosaResponse>)
                new AddressRepresentationBosaHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<MunicipalityConsumerContext>(),
                    c.Resolve<StreetNameConsumerContext>(),
                    c.Resolve<IOptions<ResponseOptions>>())).InstancePerLifetimeScope();

            builder.Register(c =>
            {
                // Decision made to keep addressmatch with old (CRAB) data in legacy endpoint
                // if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                // {
                //     return (IRequestHandler<AddressMatchRequest, AddressMatchCollection>)
                //         new AddressMatchHandlerV2(
                //             c.Resolve<AddressMatch.V2.Matching.ILatestQueries>(),
                //             c.Resolve<AddressMatchContextV2>(),
                //             c.Resolve<IOptions<ResponseOptions>>());
                // }

                return (IRequestHandler<AddressMatchRequest, AddressMatchCollection>)
                    new AddressMatchHandler(
                        c.Resolve<IKadRrService>(),
                        c.Resolve<ILatestQueries>(),
                        c.Resolve<AddressMatchContext>(),
                        c.Resolve<BuildingContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());

            }).InstancePerLifetimeScope();
        }
    }
}
