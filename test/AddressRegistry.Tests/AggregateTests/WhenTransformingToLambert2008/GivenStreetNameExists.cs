namespace AddressRegistry.Tests.AggregateTests.WhenTransformingToLambert2008
{
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetNameExists : AddressRegistryTest
    {
        /// <summary>
        /// <see cref="GeometryHelpers.GmlPointGeometry"/>, and the same physical point in Lambert 2008. Asserting
        /// against fixed coordinates rather than against the transform the aggregate itself runs, so the test
        /// actually pins the reference system instead of restating the implementation.
        /// </summary>
        private const string Lambert72PointWkt = "POINT (103671.37 192046.71)";
        private const string Lambert2008PointWkt = WithExtendedWkbGeometryLambert2008.PointWkt;

        private readonly StreetNameStreamId _streamId;

        public GivenStreetNameExists(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());

            Fixture.Customize<TransformToLambert2008>(composer =>
                composer.FromFactory(() => new TransformToLambert2008(
                    Fixture.Create<StreetNamePersistentLocalId>(),
                    Fixture.Create<Provenance>())));

            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        private static ExtendedWkbGeometry Lambert72Position
            => GeometryHelpers.CreateEwkbFromWkt(Lambert72PointWkt, SystemReferenceId.SridLambert72);

        private static ExtendedWkbGeometry Lambert2008Position
            => GeometryHelpers.CreateEwkbFromWkt(Lambert2008PointWkt, SystemReferenceId.SridLambert2008);

        [Fact]
        public void ThenAddressPositionCrsWasChanged()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithGeometryMethod(GeometryMethod.AppointedByAdministrator)
                .WithGeometrySpecification(GeometrySpecification.Entry)
                .WithExtendedWkbGeometry(Lambert72Position);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(Fixture.Create<TransformToLambert2008>())
                .Then(new Fact(_streamId,
                    new AddressPositionCrsWasChanged(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        GeometryMethod.AppointedByAdministrator,
                        GeometrySpecification.Entry,
                        Lambert2008Position))));
        }

        /// <summary>
        /// Positions written before the event store wrote EWKB carry no SRID at all. They are Lambert 72 by
        /// definition (see ADR 0004), so they transform like any other, and come out carrying SRID 3812.
        /// </summary>
        [Fact]
        public void WithPositionWithoutSrid_ThenPositionIsTransformedFromLambert72()
        {
            var positionWithoutSrid = new ExtendedWkbGeometry(
                GeometryHelpers.CreateWkbWithoutSridFromWkt(Lambert72PointWkt));

            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithGeometryMethod(GeometryMethod.AppointedByAdministrator)
                .WithGeometrySpecification(GeometrySpecification.Entry)
                .WithExtendedWkbGeometry(positionWithoutSrid);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(Fixture.Create<TransformToLambert2008>())
                .Then(new Fact(_streamId,
                    new AddressPositionCrsWasChanged(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        GeometryMethod.AppointedByAdministrator,
                        GeometrySpecification.Entry,
                        Lambert2008Position))));
        }

        /// <summary>Re-running the transformation over a stream must be a no-op, not a second transform.</summary>
        [Fact]
        public void WithPositionAlreadyInLambert2008_ThenNone()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithExtendedWkbGeometry(Lambert2008Position);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    addressWasProposedV2)
                .When(Fixture.Create<TransformToLambert2008>())
                .ThenNone());
        }

        [Fact]
        public void WithoutAddresses_ThenNone()
        {
            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>())
                .When(Fixture.Create<TransformToLambert2008>())
                .ThenNone());
        }

        /// <summary>
        /// The transformation is not an edit, so unlike changing or correcting a position it must reach removed
        /// addresses too — leaving them behind would keep the event store mixed forever.
        /// </summary>
        [Fact]
        public void WithRemovedAddress_ThenAddressPositionCrsWasChanged()
        {
            var removedAddress = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Proposed)
                .WithPosition(Lambert72Position)
                .WithRemoved();

            Assert(new Scenario()
                .Given(_streamId, removedAddress)
                .When(Fixture.Create<TransformToLambert2008>())
                .Then(new Fact(_streamId,
                    new AddressPositionCrsWasChanged(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        removedAddress.GeometryMethod,
                        removedAddress.GeometrySpecification,
                        Lambert2008Position))));
        }

        [Theory]
        [InlineData(AddressStatus.Proposed)]
        [InlineData(AddressStatus.Current)]
        [InlineData(AddressStatus.Rejected)]
        [InlineData(AddressStatus.Retired)]
        public void WithAnyAddressStatus_ThenAddressPositionCrsWasChanged(AddressStatus addressStatus)
        {
            var address = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: addressStatus)
                .WithPosition(Lambert72Position);

            Assert(new Scenario()
                .Given(_streamId, address)
                .When(Fixture.Create<TransformToLambert2008>())
                .Then(new Fact(_streamId,
                    new AddressPositionCrsWasChanged(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        address.GeometryMethod,
                        address.GeometrySpecification,
                        Lambert2008Position))));
        }

        /// <summary>A retired or rejected street name holds positions like any other and must be transformed too.</summary>
        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void WithInactiveStreetName_ThenAddressPositionCrsWasChanged(bool retired)
        {
            var address = Fixture.Create<AddressWasMigratedToStreetName>()
                .AsHouseNumberAddress(addressStatus: AddressStatus.Current)
                .WithPosition(Lambert72Position);

            var streetNameStatusChange = retired
                ? (object)Fixture.Create<StreetNameWasRetired>()
                : Fixture.Create<StreetNameWasRejected>();

            Assert(new Scenario()
                .Given(_streamId, address, streetNameStatusChange)
                .When(Fixture.Create<TransformToLambert2008>())
                .Then(new Fact(_streamId,
                    new AddressPositionCrsWasChanged(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        Fixture.Create<AddressPersistentLocalId>(),
                        address.GeometryMethod,
                        address.GeometrySpecification,
                        Lambert2008Position))));
        }

        [Fact]
        public void WithBoxNumberAddresses_ThenEveryPositionWasTransformed()
        {
            var parentAddressPersistentLocalId = new AddressPersistentLocalId(1);
            var firstChildAddressPersistentLocalId = new AddressPersistentLocalId(2);
            var secondChildAddressPersistentLocalId = new AddressPersistentLocalId(3);

            var parentAddress = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithAddressPersistentLocalId(parentAddressPersistentLocalId)
                .WithExtendedWkbGeometry(Lambert72Position);

            var firstChildAddress = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(parentAddressPersistentLocalId, new BoxNumber("1A"))
                .WithAddressPersistentLocalId(firstChildAddressPersistentLocalId)
                .WithExtendedWkbGeometry(Lambert72Position);

            var secondChildAddress = Fixture.Create<AddressWasProposedV2>()
                .AsBoxNumberAddress(parentAddressPersistentLocalId, new BoxNumber("1B"))
                .WithAddressPersistentLocalId(secondChildAddressPersistentLocalId)
                .WithExtendedWkbGeometry(Lambert72Position);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    parentAddress,
                    firstChildAddress,
                    secondChildAddress)
                .When(Fixture.Create<TransformToLambert2008>())
                .Then(
                    new Fact(_streamId,
                        new AddressPositionCrsWasChanged(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            parentAddressPersistentLocalId,
                            parentAddress.GeometryMethod,
                            parentAddress.GeometrySpecification,
                            Lambert2008Position)),
                    new Fact(_streamId,
                        new AddressPositionCrsWasChanged(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            firstChildAddressPersistentLocalId,
                            firstChildAddress.GeometryMethod,
                            firstChildAddress.GeometrySpecification,
                            Lambert2008Position)),
                    new Fact(_streamId,
                        new AddressPositionCrsWasChanged(
                            Fixture.Create<StreetNamePersistentLocalId>(),
                            secondChildAddressPersistentLocalId,
                            secondChildAddress.GeometryMethod,
                            secondChildAddress.GeometrySpecification,
                            Lambert2008Position))));
        }

        /// <summary>Only the addresses that are not already Lambert 2008 produce an event.</summary>
        [Fact]
        public void WithMixedReferenceSystems_ThenOnlyLambert72PositionsWereTransformed()
        {
            var lambert72AddressPersistentLocalId = new AddressPersistentLocalId(1);
            var lambert2008AddressPersistentLocalId = new AddressPersistentLocalId(2);

            var lambert72Address = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress(new HouseNumber("1"))
                .WithAddressPersistentLocalId(lambert72AddressPersistentLocalId)
                .WithExtendedWkbGeometry(Lambert72Position);

            var lambert2008Address = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress(new HouseNumber("2"))
                .WithAddressPersistentLocalId(lambert2008AddressPersistentLocalId)
                .WithExtendedWkbGeometry(Lambert2008Position);

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<StreetNameWasImported>(),
                    lambert72Address,
                    lambert2008Address)
                .When(Fixture.Create<TransformToLambert2008>())
                .Then(new Fact(_streamId,
                    new AddressPositionCrsWasChanged(
                        Fixture.Create<StreetNamePersistentLocalId>(),
                        lambert72AddressPersistentLocalId,
                        lambert72Address.GeometryMethod,
                        lambert72Address.GeometrySpecification,
                        Lambert2008Position))));
        }

        [Fact]
        public void StateCheck()
        {
            var addressWasProposedV2 = Fixture.Create<AddressWasProposedV2>()
                .AsHouseNumberAddress()
                .WithGeometryMethod(GeometryMethod.AppointedByAdministrator)
                .WithGeometrySpecification(GeometrySpecification.Entry)
                .WithExtendedWkbGeometry(Lambert72Position);

            var addressPositionCrsWasChanged = new AddressPositionCrsWasChanged(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                Lambert2008Position);
            ((ISetProvenance)addressPositionCrsWasChanged).SetProvenance(Fixture.Create<Provenance>());

            var sut = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
            sut.Initialize(new List<object> { addressWasProposedV2, addressPositionCrsWasChanged });

            var address = sut.StreetNameAddresses.First(x =>
                x.AddressPersistentLocalId == Fixture.Create<AddressPersistentLocalId>());

            address.Geometry.Should().Be(new AddressGeometry(
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                Lambert2008Position));

            address.Geometry.Geometry.ToString().ToByteArray().TryReadSrid(out var srid).Should().BeTrue();
            srid.Should().Be(SystemReferenceId.SridLambert2008);
        }
    }
}
