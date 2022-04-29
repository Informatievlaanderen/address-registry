namespace AddressRegistry.Tests.ProjectionTests.Legacy
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Pipes;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using FluentAssertions;
    using global::AutoFixture;
    using Projections.Legacy.AddressDetailV2;
    using StreetName.Events;
    using Xunit;

    public class AddressDetailItemV2Tests : AddressLegacyProjectionTest<AddressDetailProjectionsV2>
    {
        private readonly Fixture? _fixture;

        public AddressDetailItemV2Tests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new InfrastructureCustomization());
            _fixture.Customize(new WithExtendedWkbGeometry());
        }

        [Fact]
        public async Task WhenAddressWasMigratedToStreetName()
        {
            var addressWasMigratedToStreetName = _fixture.Create<AddressWasMigratedToStreetName>();

            var metadata = new Dictionary<string, object>
            {
                { AddEventHashPipe.HashMetadataKey, addressWasMigratedToStreetName.GetHash() }
            };

            await Sut
                .Given(new Envelope<AddressWasMigratedToStreetName>(new Envelope(addressWasMigratedToStreetName, metadata)))
                .Then(async ct =>
                {
                    var addressDetailItemV2 = (await ct.AddressDetailV2.FindAsync(addressWasMigratedToStreetName.AddressPersistentLocalId));
                    addressDetailItemV2.Should().NotBeNull();
                    addressDetailItemV2.StreetNamePersistentLocalId.Should().Be(addressWasMigratedToStreetName.StreetNamePersistentLocalId);
                    addressDetailItemV2.HouseNumber.Should().Be(addressWasMigratedToStreetName.HouseNumber);
                    addressDetailItemV2.BoxNumber.Should().Be(addressWasMigratedToStreetName.BoxNumber);
                    addressDetailItemV2.PostalCode.Should().Be(addressWasMigratedToStreetName.PostalCode);
                    addressDetailItemV2.Status.Should().Be(addressWasMigratedToStreetName.Status);
                    addressDetailItemV2.OfficiallyAssigned.Should().Be(addressWasMigratedToStreetName.OfficiallyAssigned);
                    addressDetailItemV2.Position.Should().BeEquivalentTo(addressWasMigratedToStreetName.ExtendedWkbGeometry?.ToByteArray() ?? null);
                    addressDetailItemV2.PositionMethod.Should().Be(addressWasMigratedToStreetName.GeometryMethod);
                    addressDetailItemV2.PositionSpecification.Should().Be(addressWasMigratedToStreetName.GeometrySpecification);
                    addressDetailItemV2.Removed.Should().Be(addressWasMigratedToStreetName.IsRemoved);
                    addressDetailItemV2.VersionTimestamp.Should().Be(addressWasMigratedToStreetName.Provenance.Timestamp);
                    addressDetailItemV2.LastEventHash.Should().Be(addressWasMigratedToStreetName.GetHash());
                });
        }

        protected override AddressDetailProjectionsV2 CreateProjection()
            => new AddressDetailProjectionsV2();
    }
}
