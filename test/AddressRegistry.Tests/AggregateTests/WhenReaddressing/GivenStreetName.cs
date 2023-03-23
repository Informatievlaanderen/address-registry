namespace AddressRegistry.Tests.AggregateTests.WhenReaddressing
{
    using AddressRegistry.Api.BackOffice.Abstractions;
    using StreetName;
    using StreetName.Commands;
    using StreetName.Events;
    using StreetName.Exceptions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Testing;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetName : AddressRegistryTest
    {
        private readonly StreetNameStreamId _streamId;

        public GivenStreetName(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedMunicipalityId());
            Fixture.Customize(new WithFixedValidHouseNumber());
            _streamId = Fixture.Create<StreetNameStreamId>();
        }

        //[Fact]
        //public void WithExistingParent_ThenAddressWasProposed()
        //{
        //    var houseNumber = Fixture.Create<HouseNumber>();
        //    var postalCode = Fixture.Create<PostalCode>();

        //    var parentAddressWasProposed = new AddressWasProposedV2(
        //        Fixture.Create<StreetNamePersistentLocalId>(),
        //        Fixture.Create<AddressPersistentLocalId>(),
        //        parentPersistentLocalId: null,
        //        postalCode,
        //        houseNumber,
        //        boxNumber: null,
        //        GeometryMethod.AppointedByAdministrator,
        //        GeometrySpecification.Entry,
        //        GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
        //    ((ISetProvenance)parentAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

        //    var proposeChildAddress = new ProposeAddress(
        //        Fixture.Create<StreetNamePersistentLocalId>(),
        //        postalCode,
        //        Fixture.Create<MunicipalityId>(),
        //        Fixture.Create<AddressPersistentLocalId>(),
        //        houseNumber,
        //        new BoxNumber("1A"),
        //        GeometryMethod.AppointedByAdministrator,
        //        GeometrySpecification.Entry,
        //        GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
        //        Fixture.Create<Provenance>());

        //    Assert(new Scenario()
        //        .Given(_streamId,
        //            Fixture.Create<MigratedStreetNameWasImported>(),
        //            parentAddressWasProposed)
        //        .When(proposeChildAddress)
        //        .Then(
        //            new Fact(_streamId,
        //                new AddressWasProposedV2(
        //                    proposeChildAddress.StreetNamePersistentLocalId,
        //                    proposeChildAddress.AddressPersistentLocalId,
        //                    new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId),
        //                    proposeChildAddress.PostalCode,
        //                    proposeChildAddress.HouseNumber,
        //                    proposeChildAddress.BoxNumber,
        //                    GeometryMethod.AppointedByAdministrator,
        //                    GeometrySpecification.Entry,
        //                    GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))));
        //}

        //[Fact]
        //public void WithExistingParent_ThenChildAddressWasAddedToStreetNameAddresses()
        //{
        //    var aggregateId = Fixture.Create<StreetNamePersistentLocalId>();
        //    var aggregate = new StreetNameFactory(IntervalStrategy.Default).Create();

        //    var postalCode = Fixture.Create<PostalCode>();
        //    var houseNumber = Fixture.Create<HouseNumber>();
        //    var municipalityId = Fixture.Create<MunicipalityId>();

        //    var geometryMethod = GeometryMethod.AppointedByAdministrator;
        //    var geometrySpecification = GeometrySpecification.Entry;
        //    var geometryPosition = GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry();

        //    var childPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
        //    var childBoxNumber = Fixture.Create<BoxNumber>();

        //    var parentAddressWasProposed = new AddressWasProposedV2(
        //        Fixture.Create<StreetNamePersistentLocalId>(),
        //        Fixture.Create<AddressPersistentLocalId>(),
        //        parentPersistentLocalId: null,
        //        postalCode,
        //        houseNumber,
        //        boxNumber: null,
        //        geometryMethod,
        //        geometrySpecification,
        //        geometryPosition);
        //    ((ISetProvenance)parentAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

        //    var migrateRemovedIdenticalParentAddress = new AddressWasMigratedToStreetName(
        //        Fixture.Create<StreetNamePersistentLocalId>(),
        //        Fixture.Create<AddressId>(),
        //        Fixture.Create<AddressStreetNameId>(),
        //        Fixture.Create<AddressPersistentLocalId>(),
        //        AddressStatus.Current,
        //        houseNumber,
        //        boxNumber: null,
        //        Fixture.Create<AddressGeometry>(),
        //        officiallyAssigned: true,
        //        postalCode: null,
        //        isCompleted: false,
        //        isRemoved: true,
        //        parentPersistentLocalId: null);
        //    ((ISetProvenance)migrateRemovedIdenticalParentAddress).SetProvenance(Fixture.Create<Provenance>());

        //    var migrateRemovedIdenticalChildAddress = new AddressWasMigratedToStreetName(
        //        Fixture.Create<StreetNamePersistentLocalId>(),
        //        Fixture.Create<AddressId>(),
        //        Fixture.Create<AddressStreetNameId>(),
        //        Fixture.Create<AddressPersistentLocalId>(),
        //        AddressStatus.Current,
        //        houseNumber,
        //        boxNumber: childBoxNumber,
        //        Fixture.Create<AddressGeometry>(),
        //        officiallyAssigned: true,
        //        postalCode: null,
        //        isCompleted: false,
        //        isRemoved: true,
        //        parentPersistentLocalId: null);
        //    ((ISetProvenance)migrateRemovedIdenticalChildAddress).SetProvenance(Fixture.Create<Provenance>());

        //    aggregate.Initialize(new List<object>
        //    {
        //        Fixture.Create<MigratedStreetNameWasImported>(),
        //        parentAddressWasProposed,
        //        migrateRemovedIdenticalParentAddress,
        //        migrateRemovedIdenticalChildAddress
        //    });

        //    // Act
        //    aggregate.ProposeAddress(
        //        childPersistentLocalId,
        //        postalCode,
        //        municipalityId,
        //        houseNumber,
        //        childBoxNumber,
        //        geometryMethod,
        //        geometrySpecification,
        //        geometryPosition);

        //    // Assert
        //    var result = aggregate.StreetNameAddresses.GetByPersistentLocalId(new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId));
        //    result.Should().NotBeNull();
        //    result.Children.Count.Should().Be(1);
        //    var child = result.Children.Single();
        //    child.AddressPersistentLocalId.Should().Be(childPersistentLocalId);
        //    child.HouseNumber.Should().Be(houseNumber);
        //    child.PostalCode.Should().Be(postalCode);
        //    child.Status.Should().Be(AddressStatus.Proposed);
        //    child.BoxNumber.Should().Be(childBoxNumber);
        //    child.IsOfficiallyAssigned.Should().BeTrue();
        //    child.Geometry.GeometryMethod.Should().Be(geometryMethod);
        //    child.Geometry.GeometrySpecification.Should().Be(geometrySpecification);
        //    child.Geometry.Geometry.Should().Be(geometryPosition);
        //}

        [Fact]
        public void WithExistingParentRemoved_ThenParentAddressNotFoundExceptionWasThrown()
        {
            var houseNumber = Fixture.Create<HouseNumber>();

            var parentAddressWasProposed = new AddressWasProposedV2(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                parentPersistentLocalId: null,
                Fixture.Create<PostalCode>(),
                houseNumber,
                boxNumber: null,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());

            ((ISetProvenance)parentAddressWasProposed).SetProvenance(Fixture.Create<Provenance>());

            var parentAddressWasRemoved = new AddressWasRemovedV2(
                new StreetNamePersistentLocalId(parentAddressWasProposed.StreetNamePersistentLocalId),
                new AddressPersistentLocalId(parentAddressWasProposed.AddressPersistentLocalId));

            ((ISetProvenance)parentAddressWasRemoved).SetProvenance(Fixture.Create<Provenance>());

            var proposeChildAddress = new ProposeAddress(
                Fixture.Create<StreetNamePersistentLocalId>(),
                Fixture.Create<PostalCode>(),
                Fixture.Create<MunicipalityId>(),
                Fixture.Create<AddressPersistentLocalId>(),
                houseNumber,
                Fixture.Create<BoxNumber>(),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<Provenance>());

            Assert(new Scenario()
                .Given(_streamId,
                    Fixture.Create<MigratedStreetNameWasImported>(),
                    parentAddressWasProposed,
                    parentAddressWasRemoved)
                .When(proposeChildAddress)
                .Throws(new ParentAddressNotFoundException(
                    new StreetNamePersistentLocalId(parentAddressWasProposed.StreetNamePersistentLocalId),
                    houseNumber)));
        }
    }
}
