// namespace AddressRegistry.Tests.AggregateTests.WhenRenamingStreetName
// {
//     using System.Collections.Generic;
//     using System.Linq;
//     using AddressRegistry.Api.BackOffice.Abstractions;
//     using AddressRegistry.StreetName;
//     using AddressRegistry.StreetName.DataStructures;
//     using AddressRegistry.StreetName.Events;
//     using AddressRegistry.Tests.AutoFixture;
//     using AddressRegistry.Tests.EventBuilders;
//     using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
//     using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
//     using FluentAssertions;
//     using global::AutoFixture;
//     using Xunit;
//     using Xunit.Abstractions;
//
//     public class StateCheck : AddressRegistryTest
//     {
//         private readonly StreetNamePersistentLocalId _streetNamePersistentLocalId;
//
//         public StateCheck(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
//         {
//             Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
//
//             _streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
//         }
//
//         [Fact]
//         public void ThenValidState()
//         {
//             var sourceAddressPersistentLocalId = new AddressPersistentLocalId(100);
//             var sourceAddressFirstBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(101);
//             var sourceAddressSecondBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(102);
//
//             var destinationAddressPersistentLocalId = new AddressPersistentLocalId(1);
//             var destinationAddressFirstBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(2);
//             var destinationAddressSecondBoxNumberAddressPersistentLocalId = new AddressPersistentLocalId(3);
//
//             var postalCode = Fixture.Create<PostalCode>();
//             var houseNumberEleven = new HouseNumber("11");
//
//             var sourceAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
//                 .WithAddressPersistentLocalId(sourceAddressPersistentLocalId)
//                 .WithHouseNumber(houseNumberEleven)
//                 .WithPostalCode(postalCode)
//                 .WithAddressGeometry(new AddressGeometry(
//                     GeometryMethod.AppointedByAdministrator,
//                     GeometrySpecification.Entry,
//                     GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
//                 .Build();
//
//             var sourceAddressFirstBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
//                 .WithAddressPersistentLocalId(sourceAddressFirstBoxNumberAddressPersistentLocalId)
//                 .WithHouseNumber(houseNumberEleven)
//                 .WithPostalCode(postalCode)
//                 .WithAddressGeometry(new AddressGeometry(
//                     GeometryMethod.AppointedByAdministrator,
//                     GeometrySpecification.Entry,
//                     GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
//                 .WithBoxNumber(new BoxNumber("A1"), sourceAddressPersistentLocalId)
//                 .Build();
//
//             var sourceAddressSecondBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
//                 .WithAddressPersistentLocalId(sourceAddressSecondBoxNumberAddressPersistentLocalId)
//                 .WithHouseNumber(houseNumberEleven)
//                 .WithPostalCode(postalCode)
//                 .WithAddressGeometry(new AddressGeometry(
//                     GeometryMethod.AppointedByAdministrator,
//                     GeometrySpecification.Entry,
//                     GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
//                 .WithBoxNumber(new BoxNumber("A2"), sourceAddressPersistentLocalId)
//                 .Build();
//
//             var destinationAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
//                 .WithAddressPersistentLocalId(destinationAddressPersistentLocalId)
//                 .WithHouseNumber(houseNumberEleven)
//                 .WithPostalCode(postalCode)
//                 .WithAddressGeometry(new AddressGeometry(
//                     GeometryMethod.AppointedByAdministrator,
//                     GeometrySpecification.Entry,
//                     GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
//                 .Build();
//
//             var destinationAddressFirstBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Proposed)
//                 .WithAddressPersistentLocalId(destinationAddressFirstBoxNumberAddressPersistentLocalId)
//                 .WithHouseNumber(houseNumberEleven)
//                 .WithPostalCode(postalCode)
//                 .WithAddressGeometry(new AddressGeometry(
//                     GeometryMethod.AppointedByAdministrator,
//                     GeometrySpecification.Entry,
//                     GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
//                 .WithBoxNumber(new BoxNumber("A1"), destinationAddressPersistentLocalId)
//                 .Build();
//
//             var destinationAddressSecondBoxNumberAddressWasMigrated = new AddressWasMigratedToStreetNameBuilder(Fixture, AddressStatus.Current)
//                 .WithAddressPersistentLocalId(destinationAddressSecondBoxNumberAddressPersistentLocalId)
//                 .WithHouseNumber(houseNumberEleven)
//                 .WithPostalCode(postalCode)
//                 .WithAddressGeometry(new AddressGeometry(
//                     GeometryMethod.AppointedByAdministrator,
//                     GeometrySpecification.Entry,
//                     GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry()))
//                 .WithBoxNumber(new BoxNumber("A3"), destinationAddressPersistentLocalId)
//                 .Build();
//
//             // A2
//             var destinationFirstBoxNumberWasProposed = new AddressWasProposedBecauseOfReaddress(
//                 _streetNamePersistentLocalId,
//                 destinationAddressFirstBoxNumberAddressPersistentLocalId,
//                 sourceAddressFirstBoxNumberAddressPersistentLocalId,
//                 destinationAddressPersistentLocalId,
//                 new PostalCode(sourceAddressWasMigrated.PostalCode!),
//                 houseNumberEleven,
//                 new BoxNumber(sourceAddressSecondBoxNumberAddressWasMigrated.BoxNumber!),
//                 sourceAddressFirstBoxNumberAddressWasMigrated.GeometryMethod,
//                 sourceAddressFirstBoxNumberAddressWasMigrated.GeometrySpecification,
//                 new ExtendedWkbGeometry(sourceAddressFirstBoxNumberAddressWasMigrated.ExtendedWkbGeometry));
//
//             var destinationSecondBoxNumberWasRejected = new AddressWasRejectedBecauseOfReaddress(
//                 _streetNamePersistentLocalId,
//                 destinationAddressSecondBoxNumberAddressPersistentLocalId);
//
//             var addressHouseNumberWasReaddressed = new AddressHouseNumberWasReaddressed(
//                 _streetNamePersistentLocalId,
//                 destinationAddressPersistentLocalId,
//                 readdressedHouseNumber: new ReaddressedAddressData(
//                     sourceAddressPersistentLocalId,
//                     destinationAddressPersistentLocalId,
//                     isDestinationNewlyProposed: true,
//                     sourceAddressWasMigrated.Status,
//                     houseNumberEleven,
//                     boxNumber: null,
//                     new PostalCode(sourceAddressWasMigrated.PostalCode!),
//                     new AddressGeometry(
//                         sourceAddressWasMigrated.GeometryMethod,
//                         sourceAddressWasMigrated.GeometrySpecification,
//                         new ExtendedWkbGeometry(sourceAddressWasMigrated.ExtendedWkbGeometry)),
//                     sourceAddressWasMigrated.OfficiallyAssigned),
//                 readdressedBoxNumbers: new List<ReaddressedAddressData>
//                 {
//                         new ReaddressedAddressData(
//                             sourceAddressFirstBoxNumberAddressPersistentLocalId,
//                             destinationAddressFirstBoxNumberAddressPersistentLocalId,
//                             isDestinationNewlyProposed: false,
//                             sourceAddressFirstBoxNumberAddressWasMigrated.Status,
//                             houseNumberEleven,
//                             new BoxNumber(sourceAddressFirstBoxNumberAddressWasMigrated.BoxNumber!), //A1
//                             new PostalCode(sourceAddressWasMigrated.PostalCode!),
//                             new AddressGeometry(
//                                 sourceAddressFirstBoxNumberAddressWasMigrated.GeometryMethod,
//                                 sourceAddressFirstBoxNumberAddressWasMigrated.GeometrySpecification,
//                                 new ExtendedWkbGeometry(sourceAddressFirstBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
//                             sourceAddressFirstBoxNumberAddressWasMigrated.OfficiallyAssigned),
//                         new ReaddressedAddressData(
//                             sourceAddressFirstBoxNumberAddressPersistentLocalId,
//                             destinationAddressFirstBoxNumberAddressPersistentLocalId,
//                             isDestinationNewlyProposed: true,
//                             sourceAddressFirstBoxNumberAddressWasMigrated.Status,
//                             houseNumberEleven,
//                             new BoxNumber(sourceAddressSecondBoxNumberAddressWasMigrated.BoxNumber!), //A2
//                             new PostalCode(sourceAddressWasMigrated.PostalCode!),
//                             new AddressGeometry(
//                                 sourceAddressFirstBoxNumberAddressWasMigrated.GeometryMethod,
//                                 sourceAddressFirstBoxNumberAddressWasMigrated.GeometrySpecification,
//                                 new ExtendedWkbGeometry(sourceAddressFirstBoxNumberAddressWasMigrated.ExtendedWkbGeometry)),
//                             sourceAddressFirstBoxNumberAddressWasMigrated.OfficiallyAssigned),
//                 });
//             ((ISetProvenance)addressHouseNumberWasReaddressed).SetProvenance(Fixture.Create<Provenance>());
//
//             var streetName = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
//             streetName.Initialize(new List<object>
//             {
//                 Fixture.Create<StreetNameWasImported>(),
//                 sourceAddressWasMigrated,
//                 sourceAddressFirstBoxNumberAddressWasMigrated,
//                 sourceAddressSecondBoxNumberAddressWasMigrated,
//                 destinationHouseNumberAddressWasProposed,
//                 destinationFirstBoxNumberWasProposed,
//                 destinationSecondBoxNumberWasRejected,
//                 addressHouseNumberWasReaddressed
//             });
//
//             var sourceAddress = streetName.StreetNameAddresses.SingleOrDefault(x => x.AddressPersistentLocalId == sourceAddressPersistentLocalId);
//             sourceAddress.Should().NotBeNull();
//             sourceAddress!.Status.Should().Be(AddressStatus.Rejected);
//
//             var sourceAddressFirstBoxNumberAddress = streetName.StreetNameAddresses.SingleOrDefault(x => x.AddressPersistentLocalId == sourceAddressFirstBoxNumberAddressPersistentLocalId);
//             sourceAddressFirstBoxNumberAddress.Should().NotBeNull();
//             sourceAddressFirstBoxNumberAddress!.Status.Should().Be(AddressStatus.Rejected);
//
//             var sourceAddressSecondBoxNumberAddress = streetName.StreetNameAddresses.SingleOrDefault(x => x.AddressPersistentLocalId == sourceAddressSecondBoxNumberAddressPersistentLocalId);
//             sourceAddressSecondBoxNumberAddress.Should().NotBeNull();
//             sourceAddressSecondBoxNumberAddress!.Status.Should().Be(AddressStatus.Retired);
//
//             var destinationAddress = streetName.StreetNameAddresses.FirstOrDefault(x => x.AddressPersistentLocalId == destinationAddressPersistentLocalId);
//             destinationAddress.Should().NotBeNull();
//             destinationAddress!.HouseNumber.Should().Be(houseNumberEleven);
//             destinationAddress.Status.Should().Be(sourceAddressWasMigrated.Status);
//             destinationAddress.Geometry.GeometryMethod.Should().Be(sourceAddressWasMigrated.GeometryMethod);
//             destinationAddress.Geometry.GeometrySpecification.Should().Be(sourceAddressWasMigrated.GeometrySpecification);
//             destinationAddress.Geometry.Geometry.Should().Be(new ExtendedWkbGeometry(sourceAddressWasMigrated.ExtendedWkbGeometry));
//             destinationAddress.PostalCode.Should().Be(new PostalCode(sourceAddressWasMigrated.PostalCode!));
//             destinationAddress.IsOfficiallyAssigned.Should().Be(sourceAddressWasMigrated.OfficiallyAssigned);
//
//             destinationAddress.Children.Should().HaveCount(2);
//             var destinationAddressFirstBoxNumberAddress = destinationAddress.Children
//                 .SingleOrDefault(x => x.AddressPersistentLocalId == destinationAddressFirstBoxNumberAddressPersistentLocalId);
//             destinationAddressFirstBoxNumberAddress.Should().NotBeNull();
//             destinationAddressFirstBoxNumberAddress!.HouseNumber.Should().Be(houseNumberEleven);
//             destinationAddressFirstBoxNumberAddress.Status.Should().Be(sourceAddressFirstBoxNumberAddressWasMigrated.Status);
//             destinationAddressFirstBoxNumberAddress.Geometry.GeometryMethod.Should().Be(sourceAddressFirstBoxNumberAddressWasMigrated.GeometryMethod);
//             destinationAddressFirstBoxNumberAddress.Geometry.GeometrySpecification.Should().Be(sourceAddressFirstBoxNumberAddressWasMigrated.GeometrySpecification);
//             destinationAddressFirstBoxNumberAddress.Geometry.Geometry.Should().Be(new ExtendedWkbGeometry(sourceAddressFirstBoxNumberAddressWasMigrated.ExtendedWkbGeometry));
//             destinationAddressFirstBoxNumberAddress.PostalCode.Should().Be(new PostalCode(sourceAddressFirstBoxNumberAddressWasMigrated.PostalCode!));
//             destinationAddressFirstBoxNumberAddress.IsOfficiallyAssigned.Should().Be(sourceAddressFirstBoxNumberAddressWasMigrated.OfficiallyAssigned);
//
//             var destinationAddressSecondBoxNumberAddress = destinationAddress.Children
//                 .SingleOrDefault(x => x.AddressPersistentLocalId == destinationAddressSecondBoxNumberAddressPersistentLocalId);
//             destinationAddressSecondBoxNumberAddress.Should().NotBeNull();
//             destinationAddressSecondBoxNumberAddress!.HouseNumber.Should().Be(houseNumberEleven);
//             destinationAddressSecondBoxNumberAddress.Status.Should().Be(sourceAddressSecondBoxNumberAddressWasMigrated.Status);
//             destinationAddressSecondBoxNumberAddress.Geometry.GeometryMethod.Should().Be(sourceAddressSecondBoxNumberAddressWasMigrated.GeometryMethod);
//             destinationAddressSecondBoxNumberAddress.Geometry.GeometrySpecification.Should().Be(sourceAddressSecondBoxNumberAddressWasMigrated.GeometrySpecification);
//             destinationAddressSecondBoxNumberAddress.Geometry.Geometry.Should().Be(new ExtendedWkbGeometry(sourceAddressSecondBoxNumberAddressWasMigrated.ExtendedWkbGeometry));
//             destinationAddressSecondBoxNumberAddress.PostalCode.Should().Be(new PostalCode(sourceAddressSecondBoxNumberAddressWasMigrated.PostalCode!));
//             destinationAddressSecondBoxNumberAddress.IsOfficiallyAssigned.Should().Be(sourceAddressSecondBoxNumberAddressWasMigrated.OfficiallyAssigned);
//
//             destinationAddress.LastEventHash.Should().Be(addressHouseNumberWasReaddressed.GetHash());
//             destinationAddressFirstBoxNumberAddress.LastEventHash.Should().Be(addressHouseNumberWasReaddressed.GetHash());
//             destinationAddressSecondBoxNumberAddress.LastEventHash.Should().Be(addressHouseNumberWasReaddressed.GetHash());
//         }
//     }
// }
