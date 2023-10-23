namespace AddressRegistry.Tests.AggregateTests.WhenMigratingAddressToStreetName
{
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;
    using AddressId = Address.AddressId;

    public class GivenStreetNameWithParentAddress : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetNameWithParentAddress(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithoutParentAddressPersistentLocalId());
            Fixture.Customize(new WithExtendedWkbGeometry());
            Fixture.Customize(new WithFlemishNisCode());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressWasMigratedToStreetName()
        {
            var parentAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>();

            var command = Fixture.Create<MigrateAddressToStreetName>()
                .WithOfficiallyAssigned(true)
                .WithParentAddressId(new AddressId(parentAddressWasMigratedToStreetName.AddressId));

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>().WithStatus(StreetNameStatus.Current),
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
                        true,
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
                .WithOfficiallyAssigned(true)
                .WithParentAddressId(new AddressId(parentAddressWasMigratedToStreetName.AddressId));

            var aggregate = new StreetNameFactory(IntervalStrategy.Default).Create();
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MigratedStreetNameWasImported>().WithStatus(StreetNameStatus.Current),
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
            child.IsOfficiallyAssigned.Should().Be(command.OfficiallyAssigned ?? false);
            child.IsRemoved.Should().Be(command.IsRemoved);
            child.Parent.Should().Be(parent);

            parent.Children.Should().Contain(child);
        }

        [Fact]
        public void BoxNumberWithoutPostalCode_ThenParentPostalCodeIsUsed()
        {
            var parentAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>();

            var command = Fixture.Create<MigrateAddressToStreetName>()
                .WithParentAddressId(new AddressId(parentAddressWasMigratedToStreetName.AddressId))
                .WithOfficiallyAssigned(true)
                .WithPostalCode(null);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>().WithStatus(StreetNameStatus.Current),
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
                            command.OfficiallyAssigned ?? false,
                            new PostalCode(parentAddressWasMigratedToStreetName.PostalCode),
                            command.IsCompleted,
                            command.IsRemoved,
                            new AddressPersistentLocalId(parentAddressWasMigratedToStreetName.AddressPersistentLocalId)))));
        }
    }
}
