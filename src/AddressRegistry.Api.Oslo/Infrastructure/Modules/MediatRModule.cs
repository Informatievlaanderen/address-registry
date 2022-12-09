namespace AddressRegistry.Api.Oslo.Infrastructure.Modules
{
    using Address;
    using Address.Count;
    using Address.Detail;
    using Address.List;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Municipality;
    using Consumer.Read.StreetName;
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

            // request & notification handlers
            builder.Register<ServiceFactory>(context =>
            {
                var ctx = context.Resolve<IComponentContext>();
                return type => ctx.Resolve(type);
            });

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<AddressListOsloRequest, AddressListOsloResponse>)
                        new AddressListOsloHandlerV2(
                            c.Resolve<AddressQueryContext>(),
                            c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<AddressListOsloRequest, AddressListOsloResponse>)
                    new AddressListOsloHandler(
                        c.Resolve<AddressQueryContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();

            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<AddressDetailOsloRequest, AddressDetailOsloResponse>)
                        new AddressDetailOsloHandlerV2(
                            c.Resolve<LegacyContext>(),
                            c.Resolve<MunicipalityConsumerContext>(),
                            c.Resolve<StreetNameConsumerContext>(),
                            c.Resolve<IOptions<ResponseOptions>>());
                }

                return (IRequestHandler<AddressDetailOsloRequest, AddressDetailOsloResponse>)
                    new AddressDetailOsloHandler(
                        c.Resolve<LegacyContext>(),
                        c.Resolve<SyndicationContext>(),
                        c.Resolve<IOptions<ResponseOptions>>());
            }).InstancePerLifetimeScope();


            builder.Register(c =>
            {
                if (c.Resolve<UseProjectionsV2Toggle>().FeatureEnabled)
                {
                    return (IRequestHandler<AddressCountRequest, TotaalAantalResponse>)
                        new AddressCountOsloHandlerV2(
                            c.Resolve<LegacyContext>(),
                            c.Resolve<AddressQueryContext>());
                }

                return(IRequestHandler<AddressCountRequest, TotaalAantalResponse>)
                    new AddressCountOsloHandler(
                        c.Resolve<LegacyContext>(),
                        c.Resolve<AddressQueryContext>());
            }).InstancePerLifetimeScope();
        }
    }
}
