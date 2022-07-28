namespace AddressRegistry.Infrastructure.Modules
{
    using Autofac;
    using Address;
    using Repositories;
    using StreetName;

    public class RepositoriesModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            // We could just scan the assembly for classes using Repository<> and registering them against the only interface they implement
            builder
                .RegisterType<Addresses>()
                .As<IAddresses>();

            builder
                .RegisterType<StreetNames>()
                .As<IStreetNames>();
        }
    }
}
