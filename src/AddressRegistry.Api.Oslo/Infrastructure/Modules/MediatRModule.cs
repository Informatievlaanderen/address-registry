namespace AddressRegistry.Api.Oslo.Infrastructure.Modules
{
    using Address;
    using Address.Count;
    using Address.Detail;
    using Address.List;
    using AddressMatch.Requests;
    using AddressMatch.Responses;
    using AddressMatch.V2;
    using AddressMatch.V2.Matching;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Municipality;
    using Consumer.Read.StreetName;
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

            builder.Register(c => (IRequestHandler<AddressListOsloRequest, AddressListOsloResponse>)
                new AddressListOsloHandlerV2(
                    c.Resolve<AddressQueryContext>(),
                    c.Resolve<IOptions<ResponseOptions>>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<AddressDetailOsloRequest, AddressDetailOsloResponse>)
                new AddressDetailOsloHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<MunicipalityConsumerContext>(),
                    c.Resolve<StreetNameConsumerContext>(),
                    c.Resolve<IOptions<ResponseOptions>>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<AddressCountRequest, TotaalAantalResponse>)
                new AddressCountOsloHandlerV2(
                    c.Resolve<LegacyContext>(),
                    c.Resolve<AddressQueryContext>())).InstancePerLifetimeScope();

            builder.Register(c => (IRequestHandler<AddressMatchRequest, AddressMatchOsloCollection>)
                new AddressMatchHandlerV2(
                    c.Resolve<ILatestQueries>(),
                    c.Resolve<IOptions<ResponseOptions>>())).InstancePerLifetimeScope();
        }
    }
}
