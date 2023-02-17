namespace AddressRegistry.Tests
{
    using System.Collections.Generic;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.SqlStreamStore.Autofac;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.EventHandling.Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Infrastructure.Modules;
    using Microsoft.Extensions.Configuration;
    using Newtonsoft.Json;
    using StreetName;
    using StreetName.Events;
    using Xunit.Abstractions;

    public class AddressRegistryTest : AutofacBasedTest
    {
        protected Fixture Fixture { get; }
        protected string ConfigDetailUrl => "http://base/{0}";
        protected JsonSerializerSettings EventSerializerSettings { get; } = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();

        public AddressRegistryTest(
            ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            Fixture = new Fixture();
            Fixture.Register(() => (ISnapshotStrategy)NoSnapshotStrategy.Instance);
            Fixture.Customize(new WithValidHouseNumber());
        }

        protected override void ConfigureCommandHandling(ContainerBuilder builder)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string> { { "ConnectionStrings:Events", "x" } })
                .AddInMemoryCollection(new Dictionary<string, string> { { "ConnectionStrings:Snapshots", "x" } })
                .AddInMemoryCollection(new Dictionary<string, string> {  { "DetailUrl", ConfigDetailUrl } })
                .Build();

            builder.Register((a) => (IConfiguration)configuration);

            builder
                .RegisterModule(new LegacyCommandHandlingModule(configuration))
                .RegisterModule(new CommandHandlingModule(configuration))
                .RegisterModule(new SqlStreamStoreModule());

            builder.RegisterModule(new SqlSnapshotStoreModule());

            builder
                .Register(c => new StreetNameFactory(Fixture.Create<ISnapshotStrategy>()))
                .As<IStreetNameFactory>();
        }

        protected override void ConfigureEventHandling(ContainerBuilder builder)
        {
            var eventSerializerSettings = EventsJsonSerializerSettingsProvider.CreateSerializerSettings();
            builder.RegisterModule(new EventHandlingModule(typeof(DomainAssemblyMarker).Assembly, eventSerializerSettings));
        }

        public string GetSnapshotIdentifier(string identifier) => $"{identifier}-snapshots";

        protected AddressWasMigratedToStreetName CreateAddressWasMigratedToStreetName(
            AddressPersistentLocalId? addressPersistentLocalId = null,
            StreetNamePersistentLocalId? streetNamePersistentLocalId = null,
            AddressPersistentLocalId? parentAddressPersistentLocalId = null,
            HouseNumber? houseNumber = null,
            BoxNumber? boxNumber = null,
            PostalCode? postalCode = null,
            AddressStatus addressStatus = AddressStatus.Proposed,
            bool isOfficiallyAssigned = true,
            bool isRemoved = false)
        {
            var addressWasMigratedToStreetName = new AddressWasMigratedToStreetName(
                streetNamePersistentLocalId ?? Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressId>(),
                Fixture.Create<AddressStreetNameId>(),
                addressPersistentLocalId ?? Fixture.Create<AddressPersistentLocalId>(),
                addressStatus,
                houseNumber ?? Fixture.Create<HouseNumber>(),
                boxNumber: boxNumber ?? (parentAddressPersistentLocalId is not null ? Fixture.Create<BoxNumber>() : null),
                Fixture.Create<AddressGeometry>(),
                officiallyAssigned: isOfficiallyAssigned,
                postalCode ?? Fixture.Create<PostalCode>(),
                isCompleted: false,
                isRemoved: isRemoved,
                parentAddressPersistentLocalId);
            ((ISetProvenance)addressWasMigratedToStreetName).SetProvenance(Fixture.Create<Provenance>());

            return addressWasMigratedToStreetName;
        }
    }
}
