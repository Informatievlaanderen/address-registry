namespace AddressRegistry.Tests.AggregateTests.WhenMigratingAddressToStreetName
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetNameWithParentAddress : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetNameWithParentAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithoutParentAddressPersistentLocalId());
            Fixture.Customize(new WithExtendedWkbGeometry());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressWasMigratedToStreetName()
        {
            var parentAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>();

            var command = Fixture.Create<MigrateAddressToStreetName>()
                .WithParentAddressId(new AddressRegistry.Address.AddressId(parentAddressWasMigratedToStreetName.AddressId));

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddressWasMigratedToStreetName)
                .When(command)
                .Then(
                new Fact(new StreetNameStreamId(command.StreetNamePersistentLocalId),
                    new AddressWasMigratedToStreetName(
                        command.StreetNamePersistentLocalId,
                        command.AddressId,
                        command.StreetNameId,
                        command.AddressPersistentLocalId,
                        command.Status,
                        command.HouseNumber,
                        command.BoxNumber,
                        command.Geometry,
                        command.OfficiallyAssigned,
                        command.PostalCode,
                        command.IsCompleted,
                        command.IsRemoved,
                        new AddressPersistentLocalId(parentAddressWasMigratedToStreetName.AddressPersistentLocalId)))));
        }


        [Fact]
        public void ThenExpectedAddressWasAddedToStreetNameAndParent()
        {
            var parentAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>();

            var command = Fixture.Create<MigrateAddressToStreetName>()
                .WithParentAddressId(new AddressRegistry.Address.AddressId(parentAddressWasMigratedToStreetName.AddressId));

            var aggregate = StreetName.Factory();
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<StreetNameWasImported>(),
                parentAddressWasMigratedToStreetName
            });

            // Act
            aggregate.MigrateAddress(
                command.AddressId,
                command.StreetNameId,
                command.AddressPersistentLocalId,
                command.Status,
                command.HouseNumber,
                command.BoxNumber,
                command.Geometry,
                command.OfficiallyAssigned,
                command.PostalCode,
                command.IsCompleted,
                command.IsRemoved,
                command.ParentAddressId);

            // Assert
            var parent = aggregate.StreetNameAddresses.GetByPersistentLocalId(new AddressPersistentLocalId(parentAddressWasMigratedToStreetName.AddressPersistentLocalId));
            var child = aggregate.StreetNameAddresses.GetByPersistentLocalId(command.AddressPersistentLocalId);
            child.Should().NotBeNull();

            child.AddressPersistentLocalId.Should().Be(command.AddressPersistentLocalId);
            child.Status.Should().Be(command.Status);
            child.HouseNumber.Should().Be(command.HouseNumber);
            child.BoxNumber.Should().Be(command.BoxNumber);
            child.PostalCode.Should().Be(command.PostalCode);
            child.Geometry.Should().Be(command.Geometry);
            child.IsOfficiallyAssigned.Should().Be(command.OfficiallyAssigned);
            child.IsRemoved.Should().Be(command.IsRemoved);
            child.Parent.Should().Be(parent);

            parent.Children.Should().Contain(child);
        }
    }
}
