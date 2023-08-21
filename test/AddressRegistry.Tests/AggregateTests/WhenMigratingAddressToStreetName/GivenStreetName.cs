namespace AddressRegistry.Tests.AggregateTests.WhenMigratingAddressToStreetName
{
    using System;
    using System.Collections.Generic;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using StreetName.Exceptions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetName : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithExtendedWkbGeometry());
            Fixture.Customize(new WithFlemishNisCode());
            Fixture.Customize(new WithFixedValidHouseNumber());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        [Fact]
        public void ThenAddressWasMigratedToStreetName()
        {
            var command = Fixture.Create<MigrateAddressToStreetName>()
                .WithoutParentAddressId();

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>())
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
                            command.PostalCode,
                            command.IsCompleted,
                            command.IsRemoved,
                            null))));
        }

        [Fact]
        public void AddressWasAlreadyMigrated_ThenNone()
        {
            IgnoreExceptionMessage = true;
            var command = Fixture.Create<MigrateAddressToStreetName>();

            var addressMigratedEvent = new AddressWasMigratedToStreetName(
                command.StreetNamePersistentLocalId,
                command.AddressId,
                command.StreetNameId,
                command.AddressPersistentLocalId,
                command.Status,
                command.HouseNumber,
                command.BoxNumber,
                command.Geometry,
                command.OfficiallyAssigned ?? false,
                command.PostalCode,
                command.IsCompleted,
                command.IsRemoved,
                null);

            ((ISetProvenance)addressMigratedEvent).SetProvenance(Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    addressMigratedEvent)
                .When(command)
                .Throws(new InvalidOperationException()));
        }

        [Fact]
        public void ThenExpectedAddressWasAddedToStreetName()
        {
            var command = Fixture.Create<MigrateAddressToStreetName>();
            var aggregate = new StreetNameFactory(IntervalStrategy.Default).Create();
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MigratedStreetNameWasImported>()
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
                null);

            // Assert
            var result = aggregate.StreetNameAddresses.GetByPersistentLocalId(command.AddressPersistentLocalId);
            result.Should().NotBeNull();

            result.AddressPersistentLocalId.Should().Be(command.AddressPersistentLocalId);
            result.Status.Should().Be(command.Status);
            result.HouseNumber.Should().Be(command.HouseNumber);
            result.BoxNumber.Should().Be(command.BoxNumber);
            result.PostalCode.Should().Be(command.PostalCode);
            result.Geometry.Should().Be(command.Geometry);
            result.IsOfficiallyAssigned.Should().Be(command.OfficiallyAssigned ?? false);
            result.IsRemoved.Should().Be(command.IsRemoved);
            result.Parent.Should().BeNull();
        }

        [Fact]
        public void WithoutPostalCode_ThenExpectedAddressWasAddedToStreetName()
        {
            var command = Fixture.Create<MigrateAddressToStreetName>()
                .WithPostalCode(null);

            var aggregate = new StreetNameFactory(IntervalStrategy.Default).Create();
            aggregate.Initialize(new List<object>
            {
                Fixture.Create<MigratedStreetNameWasImported>()
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
                null);

            // Assert
            var result = aggregate.StreetNameAddresses.GetByPersistentLocalId(command.AddressPersistentLocalId);
            result.Should().NotBeNull();

            result.AddressPersistentLocalId.Should().Be(command.AddressPersistentLocalId);
            result.Status.Should().Be(command.Status);
            result.HouseNumber.Should().Be(command.HouseNumber);
            result.BoxNumber.Should().Be(command.BoxNumber);
            result.PostalCode.Should().Be(command.PostalCode);
            result.Geometry.Should().Be(command.Geometry);
            result.IsOfficiallyAssigned.Should().Be(command.OfficiallyAssigned ?? false);
            result.IsRemoved.Should().Be(command.IsRemoved);
            result.Parent.Should().BeNull();
        }

        [Fact]
        public void WithoutParentAddressMigrated_ThenThrowsInvalidOperationException()
        {
            IgnoreExceptionMessage = true;

            var parentAddressWasMigratedToStreetName = Fixture.Create<AddressWasMigratedToStreetName>();

            var command = Fixture.Create<MigrateAddressToStreetName>()
                .WithParentAddressId(new Address.AddressId(parentAddressWasMigratedToStreetName.AddressId));

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>())
                .When(command)
                .Throws(new ParentAddressNotFoundException(command.StreetNamePersistentLocalId, command.HouseNumber)));
        }

        [Fact]
        public void WithProposedAddressAndRetiredStreetName_ThenAddressWasRejected()
        {
            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>()
                .WithStatus(StreetNameStatus.Retired);

            var command = Fixture.Create<MigrateAddressToStreetName>()
                .WithoutParentAddressId()
                .WithStatus(Address.AddressStatus.Proposed);

            Assert(new Scenario()
                .Given(_streamId,
                    migratedStreetNameWasImported)
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.StreetNamePersistentLocalId),
                        new AddressWasMigratedToStreetName(
                            command.StreetNamePersistentLocalId,
                            command.AddressId,
                            command.StreetNameId,
                            command.AddressPersistentLocalId,
                            AddressStatus.Rejected,
                            command.HouseNumber,
                            command.BoxNumber,
                            command.Geometry,
                            command.OfficiallyAssigned ?? false,
                            command.PostalCode,
                            command.IsCompleted,
                            command.IsRemoved,
                            null))));
        }

        [Fact]
        public void WithCurrentAddressAndRetiredStreetName_ThenAddressWasRetired()
        {
            var migratedStreetNameWasImported = Fixture.Create<MigratedStreetNameWasImported>()
                .WithStatus(StreetNameStatus.Retired);

            var command = Fixture.Create<MigrateAddressToStreetName>()
                .WithoutParentAddressId()
                .WithStatus(Address.AddressStatus.Current);

            Assert(new Scenario()
                .Given(_streamId,
                    migratedStreetNameWasImported)
                .When(command)
                .Then(
                    new Fact(new StreetNameStreamId(command.StreetNamePersistentLocalId),
                        new AddressWasMigratedToStreetName(
                            command.StreetNamePersistentLocalId,
                            command.AddressId,
                            command.StreetNameId,
                            command.AddressPersistentLocalId,
                            AddressStatus.Retired,
                            command.HouseNumber,
                            command.BoxNumber,
                            command.Geometry,
                            command.OfficiallyAssigned ?? false,
                            command.PostalCode,
                            command.IsCompleted,
                            command.IsRemoved,
                            null))));
        }
    }
}
