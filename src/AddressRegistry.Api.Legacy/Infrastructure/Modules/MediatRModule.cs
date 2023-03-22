namespace AddressRegistry.Api.Legacy.Infrastructure.Modules
{
    using Address;
    using Address.Bosa;
    using Address.BosaRepresentation;
    using Address.Count;
    using Address.Detail;
    using Address.List;
    using Address.Sync;
    using AddressMatch;
    using AddressMatch.V1;
    using AddressMatch.V1.Matching;
    using AddressMatch.V2;
    using AddressRegistry.Api.Legacy.AddressMatch.Requests;
    using AddressRegistry.Api.Legacy.AddressMatch.Responses;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Municipality;
    using Consumer.Read.StreetName;
    using CrabHouseNumber;
    using CrabSubaddress;
    using FeatureToggles;
    using MediatR;
    using Microsoft.Extensions.Options;
    using Options;
    using Projections.Legacy;
    using Projections.Syndication;
    using Module = Autofac.Module;

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

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<AddressListRequest, AddressListResponse>)
                        new AddressListHandlerV2(
                            c.Resolve<AddressQueryContext>(),
                            c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<AddressListRequest, AddressListResponse>)
                    new AddressListHandler(
                        c.Resolve<AddressQueryContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<AddressDetailRequest, AddressDetailResponse>)
                        new AddressDetailHandlerV2(
                            c.Resolve<LegacyContext>(),
                            c.Resolve<MunicipalityConsumerContext>(),
                            c.Resolve<StreetNameConsumerContext>(),
                            c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<AddressDetailRequest, AddressDetailResponse>)
                    new AddressDetailHandler(
                        c.Resolve<LegacyContext>(),
                        c.Resolve<SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();


            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<AddressCountRequest, TotaalAantalResponse>)
                        new AddressCountHandlerV2(
                            c.Resolve<LegacyContext>(),
                            c.Resolve<AddressQueryContext>());
                }

                return (IRequestHandler<AddressCountRequest, TotaalAantalResponse>)
                    new AddressCountHandler(
                        c.Resolve<LegacyContext>(),
                        c.Resolve<AddressQueryContext>());
            }).InstancePerLifetimeScope();

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<AddressBosaRequest, AddressBosaResponse>)
                        new AddressBosaHandlerV2(
                            c.Resolve<AddressBosaContext>(),
                            c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<AddressBosaRequest, AddressBosaResponse>)
                    new AddressBosaHandler(
                        c.Resolve<AddressBosaContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<AddressRepresentationBosaRequest, AddressRepresentationBosaResponse>)
                        new AddressRepresentationBosaHandlerV2(
                            c.Resolve<LegacyContext>(),
                            c.Resolve<MunicipalityConsumerContext>(),
                            c.Resolve<StreetNameConsumerContext>(),
                            c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<AddressRepresentationBosaRequest, AddressRepresentationBosaResponse>)
                    new AddressRepresentationBosaHandler(
                        c.Resolve<LegacyContext>(),
                        c.Resolve<SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();

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
                        c.Resolve<AddressMatch.V1.Matching.ILatestQueries>(),
                        c.Resolve<AddressMatchContext>(),
                        c.Resolve<BuildingContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());

            }).InstancePerLifetimeScope();
        }
    }
}
