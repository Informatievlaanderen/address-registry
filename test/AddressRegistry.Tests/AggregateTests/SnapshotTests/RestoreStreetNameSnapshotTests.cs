namespace AddressRegistry.Tests.AggregateTests.SnapshotTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using global::AutoFixture;
    using StreetName;
    using StreetName.DataStructures;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class RestoreStreetNameSnapshotTests : AddressRegistryTest
    {
        private readonly StreetName _sut;
        private readonly StreetNameSnapshot? _streetNameSnapshot;

        public RestoreStreetNameSnapshotTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            var random = new Random(Fixture.Create<int>());

            Fixture.Customize(new InfrastructureCustomization());
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());

            Fixture.Register<Fixture, StreetNameAddresses>(fixture =>
            {
                var addresses = new StreetNameAddresses();
                var houseNumberData = fixture
                    .Build<AddressData>()
                    .FromFactory(() => new AddressData(
                        fixture.Create<AddressPersistentLocalId>(),
                        fixture.Create<AddressStatus>(),
                        fixture.Create<HouseNumber>(),
                        null,
                        fixture.Create<PostalCode>(),
                        fixture.Create<AddressGeometry>(),
                        fixture.Create<bool>(),
                        fixture.Create<bool>(),
                        null,
                        null,
                        Fixture.Create<string>(),
                        Fixture.Create<ProvenanceData>()))
                    .CreateMany(random.Next(2, 10));

                var houseNumbers = fixture.Build<IEnumerable<StreetNameAddress>>().FromFactory(() =>
                {
                    var houseNumberAddresses = new List<StreetNameAddress>();

                    foreach (var data in houseNumberData)
                    {
                        var streetNameAddress = new StreetNameAddress(null, o => { });
                        streetNameAddress.RestoreSnapshot(fixture.Create<StreetNamePersistentLocalId>(), data);

                        houseNumberAddresses.Add(streetNameAddress);
                    }

                    return houseNumberAddresses;
                }).Create().ToList();

                var boxNumberData = fixture
                    .Build<AddressData>()
                    .FromFactory(() => new AddressData(
                        fixture.Create<AddressPersistentLocalId>(),
                        fixture.Create<AddressStatus>(),
                        fixture.Create<HouseNumber>(),
                        fixture.Create<BoxNumber>(),
                        fixture.Create<PostalCode>(),
                        fixture.Create<AddressGeometry>(),
                        fixture.Create<bool>(),
                        fixture.Create<bool>(),
                        houseNumbers.ToArray()[random.Next(0, houseNumbers.Count)],
                        null,
                        Fixture.Create<string>(),
                        Fixture.Create<ProvenanceData>()))
                    .CreateMany(random.Next(2, 10));

                var boxNumbers = fixture.Build<IEnumerable<StreetNameAddress>>().FromFactory(() =>
                {
                    var boxNumberAddresses = new List<StreetNameAddress>();

                    foreach (var data in boxNumberData)
                    {
                        var streetNameAddress = new StreetNameAddress(null, o => { });
                        streetNameAddress.RestoreSnapshot(fixture.Create<StreetNamePersistentLocalId>(), data);

                        boxNumberAddresses.Add(streetNameAddress);
                    }

                    return boxNumberAddresses;
                }).Create().ToList();

                addresses.AddRange(houseNumbers);
                addresses.AddRange(boxNumbers);

                return addresses;
            });

            _sut = new StreetNameFactory(IntervalStrategy.Default).Create();
            _streetNameSnapshot = Fixture.Create<StreetNameSnapshot>();
            _sut.Initialize(new List<object>
            {
                _streetNameSnapshot
            });
        }

        [Fact]
        public void ThenAggregateStreetNameStateIsExpected()
        {
            _sut.PersistentLocalId.Should().Be(new StreetNamePersistentLocalId(_streetNameSnapshot.StreetNamePersistentLocalId));
            _sut.MigratedNisCode.Should().Be(new NisCode(_streetNameSnapshot.MigratedNisCode));
            _sut.Status.Should().Be(_streetNameSnapshot.StreetNameStatus);
            _sut.IsRemoved.Should().Be(_streetNameSnapshot.IsRemoved);
        }

        [Fact]
        public void ThenAggregateAddressesStateAreExpected()
        {
            _sut.StreetNameAddresses.Should().NotBeEmpty();
            foreach (var address in _sut.StreetNameAddresses)
            {
                var snapshotAddress = _streetNameSnapshot.Addresses.SingleOrDefault(x => x.AddressPersistentLocalId == address.AddressPersistentLocalId);

                snapshotAddress.Should().NotBeNull();

                address.PostalCode.Should().Be(new PostalCode(snapshotAddress.PostalCode));
                address.HouseNumber.Should().Be(new HouseNumber(snapshotAddress.HouseNumber));

                if (snapshotAddress.BoxNumber is null)
                {
                    address.BoxNumber.Should().BeNull();
                }
                else
                {
                    address.BoxNumber.Should().Be(new BoxNumber(snapshotAddress.BoxNumber));
                }

                address.Geometry.Geometry.Should().Be(new ExtendedWkbGeometry(snapshotAddress.ExtendedWkbGeometry));
                address.Geometry.GeometryMethod.Should().Be(snapshotAddress.GeometryMethod);
                address.Geometry.GeometrySpecification.Should().Be(snapshotAddress.GeometrySpecification);

                address.Status.Should().Be(snapshotAddress.Status);
                address.IsOfficiallyAssigned.Should().Be(snapshotAddress.IsOfficiallyAssigned);
                address.IsRemoved.Should().Be(snapshotAddress.IsRemoved);
                address.LastProvenanceData.Should().Be(snapshotAddress.LastProvenanceData);
                address.LastEventHash.Should().Be(snapshotAddress.LastEventHash);
            }
        }
    }
}
