namespace AddressRegistry.Tests.ProjectionTests.Legacy.Wms
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using Api.BackOffice.Abstractions;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Extensions;
    using FluentAssertions;
    using global::AutoFixture;
    using NetTopologySuite.IO;
    using Projections.Wms.AddressWmsItem;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    public class AddressWmsItemHouseNumberLabelTests : AddressWmsItemProjectionTest
    {
        private readonly Fixture _fixture;
        private readonly WKBReader _wkbReader;

        public AddressWmsItemHouseNumberLabelTests()
        {
            _fixture = new Fixture();
            _fixture.Customize(new WithFixedAddressPersistentLocalId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _fixture.Customize<AddressStatus>(_ => new WithoutUnknownStreetNameAddressStatus());
            _fixture.Customize(new WithValidHouseNumber());
            _fixture.Customize(new WithExtendedWkbGeometry());
            _fixture.Customize(new InfrastructureCustomization());

            _wkbReader = WKBReaderFactory.Create();
        }

        protected override AddressWmsItemProjections CreateProjection()
            =>  new AddressWmsItemProjections(_wkbReader);

        private AddressWasMigratedToStreetName CreateAddressWasMigratedToStreetName(
            AddressPersistentLocalId addressPersistentLocalId,
            HouseNumber houseNumber,
            AddressStatus addressStatus,
            ExtendedWkbGeometry? position = null)
        {
            var @event = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithAddressPersistentLocalId(addressPersistentLocalId)
                .WithHouseNumber(houseNumber)
                .WithStatus(addressStatus)
                .WithNotRemoved();

            if (position is not null)
            {
                @event = @event.WithPosition(position);
            }

            return @event;
        }

        [Fact]
        public async Task WhenAddressWasMigratedToStreetName()
        {
            var houseNumberOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(1),
                new HouseNumber("1"),
                AddressStatus.Proposed);
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOne.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOne.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOne, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOne.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("1-3");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                });
        }

        [Fact]
        public async Task WhenAddressWasProposedV2()
        {
            var houseNumberOne = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOne.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOne.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOne.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("1-3");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                });
        }

        [Fact]
        public async Task WhenAddressWasApproved()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasApproved = _fixture.Create<AddressWasApproved>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Current,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasApproved>(new Envelope(houseNumberOneWasApproved, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromApprovedToProposed()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasApproved = _fixture.Create<AddressWasApproved>();
            var houseNumberOneWasCorrectedFromApprovedToProposed = _fixture.Create<AddressWasCorrectedFromApprovedToProposed>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Current,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasApproved>(new Envelope(houseNumberOneWasApproved, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasCorrectedFromApprovedToProposed>(new Envelope(houseNumberOneWasCorrectedFromApprovedToProposed, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("2");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(1);
                    three.HouseNumberLabelLength.Should().Be(3);
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasApproved = _fixture.Create<AddressWasApproved>();
            var houseNumberOneWasCorrectedFromApprovedToProposed = _fixture.Create<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Current,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasApproved>(new Envelope(houseNumberOneWasApproved, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>(new Envelope(houseNumberOneWasCorrectedFromApprovedToProposed, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("2");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(1);
                    three.HouseNumberLabelLength.Should().Be(3);
                });
        }

        [Fact]
        public async Task WhenAddressWasRejected()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasRejected = _fixture.Create<AddressWasRejected>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Rejected,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasRejected>(new Envelope(houseNumberOneWasRejected, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseHouseNumberWasRejected()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasRejected = _fixture.Create<AddressWasRejectedBecauseHouseNumberWasRejected>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Rejected,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>(new Envelope(houseNumberOneWasRejected, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseHouseNumberWasRetired()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasRejected = _fixture.Create<AddressWasRejectedBecauseHouseNumberWasRetired>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Rejected,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>(new Envelope(houseNumberOneWasRejected, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseStreetNameWasRejected()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasRejected = _fixture.Create<AddressWasRejectedBecauseStreetNameWasRejected>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Rejected,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasRejectedBecauseStreetNameWasRejected>(new Envelope(houseNumberOneWasRejected, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseStreetNameWasRejected()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));

            var houseNumberOneWasRetired = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRejected>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Rejected,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRejected>(new Envelope(houseNumberOneWasRetired, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1");
                    two!.HouseNumberLabel.Should().Be("2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(1);
                    two.HouseNumberLabelLength.Should().Be(1);
                    three.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenAddressWasRejectedBecauseStreetNameWasRetired()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasRejected = _fixture.Create<AddressWasRejectedBecauseStreetNameWasRetired>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Rejected,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasRejectedBecauseStreetNameWasRetired>(new Envelope(houseNumberOneWasRejected, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenAddressWasDeregulated()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasDeregulated = _fixture.Create<AddressWasDeregulated>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Current,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasDeregulated>(new Envelope(houseNumberOneWasDeregulated, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromRejectedToProposed()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasRejected = _fixture.Create<AddressWasRejected>();
            var houseNumberOneWasCorrectedFromRejectedToProposed = _fixture.Create<AddressWasCorrectedFromRejectedToProposed>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Current,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasRejected>(new Envelope(houseNumberOneWasRejected, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasCorrectedFromRejectedToProposed>(new Envelope(houseNumberOneWasCorrectedFromRejectedToProposed, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("2");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(1);
                    three.HouseNumberLabelLength.Should().Be(3);
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredV2()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasApproved = _fixture.Create<AddressWasApproved>();
            var houseNumberOneWasRetired = _fixture.Create<AddressWasRetiredV2>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Retired,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Current,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasApproved>(new Envelope(houseNumberOneWasApproved, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasRetiredV2>(new Envelope(houseNumberOneWasRetired, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseHouseNumberWasRetired()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasApproved = _fixture.Create<AddressWasApproved>();
            var houseNumberOneWasRetired = _fixture.Create<AddressWasRetiredBecauseHouseNumberWasRetired>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Retired,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Current,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasApproved>(new Envelope(houseNumberOneWasApproved, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>(new Envelope(houseNumberOneWasRetired, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenAddressWasRetiredBecauseStreetNameWasRetired()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasApproved = _fixture.Create<AddressWasApproved>();
            var houseNumberOneWasRetired = _fixture.Create<AddressWasRetiredBecauseStreetNameWasRetired>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Retired,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Current,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasApproved>(new Envelope(houseNumberOneWasApproved, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRetired>(new Envelope(houseNumberOneWasRetired, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenAddressWasCorrectedFromRetiredToCurrent()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasApproved = _fixture.Create<AddressWasApproved>();
            var houseNumberOneWasRetired = _fixture.Create<AddressWasRetiredV2>();
            var houseNumberOneWasCorrectedFromRetiredToCurrent = _fixture.Create<AddressWasCorrectedFromRetiredToCurrent>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Retired,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Current,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasApproved>(new Envelope(houseNumberOneWasApproved, new Dictionary<string, object>())),
                    new Envelope<AddressWasRetiredV2>(new Envelope(houseNumberOneWasRetired, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasCorrectedFromRetiredToCurrent>(new Envelope(houseNumberOneWasCorrectedFromRetiredToCurrent, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("2");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(1);
                    three.HouseNumberLabelLength.Should().Be(3);
                });
        }

        [Fact]
        public async Task WhenAddressHouseNumberWasCorrectedV2()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("4"));
            var houseNumberOneWasCorrected = _fixture.Create<AddressHouseNumberWasCorrectedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressHouseNumberWasCorrectedV2>(new Envelope(houseNumberOneWasCorrected, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("1-3");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasChanged()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));

            var originalPosition = houseNumberOneWasProposed.ExtendedWkbGeometry;
            var newPosition = GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry();

            var addressPositionWasChanged = new AddressPositionWasChanged(
                _fixture.Create<StreetNamePersistentLocalId>(),
                _fixture.Create<AddressPersistentLocalId>(),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Building,
                newPosition);
            ((ISetProvenance)addressPositionWasChanged).SetProvenance(_fixture.Create<Provenance>());
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Proposed,
                newPosition);
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(originalPosition));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressPositionWasChanged>(new Envelope(addressPositionWasChanged, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenAddressPositionWasCorrectedV2()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));

            var originalPosition = houseNumberOneWasProposed.ExtendedWkbGeometry;
            var newPosition = GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry();

            var addressPositionWasChanged = new AddressPositionWasCorrectedV2(
                _fixture.Create<StreetNamePersistentLocalId>(),
                _fixture.Create<AddressPersistentLocalId>(),
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Building,
                newPosition);
            ((ISetProvenance)addressPositionWasChanged).SetProvenance(_fixture.Create<Provenance>());
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Proposed,
                newPosition);
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(originalPosition));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressPositionWasCorrectedV2>(new Envelope(addressPositionWasChanged, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WhenAddressWasRemovedV2()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasRemoved = _fixture.Create<AddressWasRemovedV2>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasRemovedV2>(new Envelope(houseNumberOneWasRemoved, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().BeNull();
                    two!.HouseNumberLabel.Should().Be("2-3");
                    three!.HouseNumberLabel.Should().Be("2-3");

                    one.HouseNumberLabelLength.Should().BeNull();
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                });
        }

        [Fact]
        public async Task WhenAddressWasRemovedBecauseStreetNameWasRemoved()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasRemoved = _fixture.Create<AddressWasRemovedBecauseStreetNameWasRemoved>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>(new Envelope(houseNumberOneWasRemoved, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().BeNull();
                    two!.HouseNumberLabel.Should().Be("2-3");
                    three!.HouseNumberLabel.Should().Be("2-3");

                    one.HouseNumberLabelLength.Should().BeNull();
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                });
        }

        [Fact]
        public async Task WhenAddressWasRemovedBecauseHouseNumberWasRemoved()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasRemoved = _fixture.Create<AddressWasRemovedBecauseHouseNumberWasRemoved>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>(new Envelope(houseNumberOneWasRemoved, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItems.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItems.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var three = await ct.AddressWmsItems.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    one.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().BeNull();
                    two!.HouseNumberLabel.Should().Be("2-3");
                    three!.HouseNumberLabel.Should().Be("2-3");

                    one.HouseNumberLabelLength.Should().BeNull();
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                });
        }
    }
}
