namespace AddressRegistry.Tests.ProjectionTests.WmsV3
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using Projections.Wms;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.DataStructures;
    using AddressRegistry.StreetName.Events;
    using EventExtensions;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using FluentAssertions;
    using global::AutoFixture;
    using Xunit;
    using Envelope = Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore.Envelope;

    public partial class AddressWmsItemV3HouseNumberLabelTests
    {
        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasMigratedToStreetName()
        {
            var houseNumberOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(1),
                new HouseNumber("1"),
                AddressStatus.Proposed);

            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOne.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOne.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("1-3");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasProposedV2()
        {
            var houseNumberOne = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
                .WithParentAddressPersistentLocalId(null)
                .WithHouseNumber(new HouseNumber("1"))
                .WithBoxNumber(null);
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOne.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOne, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOne.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("1-3");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasProposedForMunicipalityMerger()
        {
            var houseNumberOne = _fixture.Create<AddressWasProposedForMunicipalityMerger>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
                .WithParentAddressPersistentLocalId(null)
                .WithHouseNumber(new HouseNumber("1"))
                .WithBoxNumber(null);
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOne.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasProposedForMunicipalityMerger>(new Envelope(houseNumberOne, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOne.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("1-3");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasApproved()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasApproved>(new Envelope(houseNumberOneWasApproved, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasCorrectedFromApprovedToProposed()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasCorrectedFromApprovedToProposed>(new Envelope(houseNumberOneWasCorrectedFromApprovedToProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("2");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(1);
                    three.HouseNumberLabelLength.Should().Be(3);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>(new Envelope(houseNumberOneWasCorrectedFromApprovedToProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("2");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(1);
                    three.HouseNumberLabelLength.Should().Be(3);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRejected()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Rejected,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasRejected>(new Envelope(houseNumberOneWasRejected, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRejectedBecauseOfMunicipalityMerger()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Rejected,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            var houseNumberOneWasRejected = _fixture.Create<AddressWasRejectedBecauseOfMunicipalityMerger>();
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
                    new Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>(new Envelope(houseNumberOneWasRejected, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRejectedBecauseHouseNumberWasRejected()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Rejected,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>(new Envelope(houseNumberOneWasRejected, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRejectedBecauseHouseNumberWasRetired()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>(new Envelope(houseNumberOneWasRejected, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRejectedBecauseStreetNameWasRejected()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Rejected,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasRejectedBecauseStreetNameWasRejected>(new Envelope(houseNumberOneWasRejected, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRetiredBecauseStreetNameWasRejected()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());

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
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRejected>(new Envelope(houseNumberOneWasRetired, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1");
                    two!.HouseNumberLabel.Should().Be("2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(1);
                    two.HouseNumberLabelLength.Should().Be(1);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRejectedBecauseStreetNameWasRetired()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Rejected,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasRejectedBecauseStreetNameWasRetired>(new Envelope(houseNumberOneWasRejected, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasDeregulated()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Current,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasDeregulated>(new Envelope(houseNumberOneWasDeregulated, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasCorrectedFromRejectedToProposed()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasCorrectedFromRejectedToProposed>(new Envelope(houseNumberOneWasCorrectedFromRejectedToProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("2");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(1);
                    three.HouseNumberLabelLength.Should().Be(3);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRetiredV2()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Retired,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasRetiredV2>(new Envelope(houseNumberOneWasRetired, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRetiredBecauseOfMunicipalityMerger()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Retired,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            var houseNumberOneWasApproved = _fixture.Create<AddressWasApproved>();
            var houseNumberOneWasRetired = _fixture.Create<AddressWasRetiredBecauseOfMunicipalityMerger>();
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
                    new Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>(new Envelope(houseNumberOneWasRetired, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRetiredBecauseHouseNumberWasRetired()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Retired,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>(new Envelope(houseNumberOneWasRetired, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRetiredBecauseStreetNameWasRetired()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Retired,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasRetiredBecauseStreetNameWasRetired>(new Envelope(houseNumberOneWasRetired, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasCorrectedFromRetiredToCurrent()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Current,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasCorrectedFromRetiredToCurrent>(new Envelope(houseNumberOneWasCorrectedFromRetiredToCurrent, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("2");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(1);
                    three.HouseNumberLabelLength.Should().Be(3);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressHouseNumberWasCorrectedV2()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("4"))
                .WithParentAddressPersistentLocalId(null)
                .WithBoxNumber(null);
            var houseNumberOneWasCorrected = _fixture.Create<AddressHouseNumberWasCorrectedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberTwoBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("2"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberTwo.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressHouseNumberWasCorrectedV2>(new Envelope(houseNumberOneWasCorrected, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwoBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var twoBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwoBoxOne.AddressPersistentLocalId);
                    twoBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("1-3");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    twoBoxOne!.HouseNumberLabel.Should().Be("2");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                    twoBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    twoBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressPositionWasChanged()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.CreateEwkbFromWkt("POINT (1 1)"));

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
                    new Envelope<AddressPositionWasChanged>(new Envelope(addressPositionWasChanged, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressPositionWasCorrectedV2()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.CreateEwkbFromWkt("POINT (1 1)"));

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
                    new Envelope<AddressPositionWasCorrectedV2>(new Envelope(addressPositionWasChanged, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressHouseNumberWasReaddressed()
        {
            var addressPersistentLocalId = _fixture.Create<AddressPersistentLocalId>();
            var addressBoxNumberPersistentLocalId = new AddressPersistentLocalId(addressPersistentLocalId + 1);
            var houseNumberThreeWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("3"))
                .WithParentAddressPersistentLocalId(null)
                .WithBoxNumber(null);
            var boxNumberThreeWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithAddressPersistentLocalId(addressBoxNumberPersistentLocalId)
                .WithHouseNumber(new HouseNumber("3"))
                .WithBoxNumber(new BoxNumber("A"))
                .WithExtendedWkbGeometry(GeometryHelpers.CreateEwkbFromWkt("POINT (1 1)"));

            var originalPosition = houseNumberThreeWasProposed.ExtendedWkbGeometry;
            var newPosition = GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry();

            var streetNameWasReaddressed = new AddressHouseNumberWasReaddressed(
                _fixture.Create<StreetNamePersistentLocalId>(),
                addressPersistentLocalId,
                new ReaddressedAddressData(
                    new AddressPersistentLocalId(addressPersistentLocalId + 10),
                    addressPersistentLocalId,
                    isDestinationNewlyProposed: true,
                    AddressStatus.Proposed,
                    new HouseNumber("3"),
                    boxNumber: null,
                    new PostalCode(houseNumberThreeWasProposed.PostalCode),
                    new AddressGeometry(
                        houseNumberThreeWasProposed.GeometryMethod,
                        houseNumberThreeWasProposed.GeometrySpecification,
                        newPosition),
                    sourceIsOfficiallyAssigned: true),
                new List<ReaddressedAddressData>
                {
                    new ReaddressedAddressData(
                        new AddressPersistentLocalId(addressPersistentLocalId + 11),
                        addressBoxNumberPersistentLocalId,
                        isDestinationNewlyProposed: true,
                        AddressStatus.Proposed,
                        new HouseNumber("3"),
                        new BoxNumber("A"),
                        new PostalCode(houseNumberThreeWasProposed.PostalCode),
                        new AddressGeometry(
                            houseNumberThreeWasProposed.GeometryMethod,
                            houseNumberThreeWasProposed.GeometrySpecification,
                            GeometryHelpers.CreateEwkbFromWkt("POINT (1 1)")),
                        sourceIsOfficiallyAssigned: true)
                });
            ((ISetProvenance)streetNameWasReaddressed).SetProvenance(_fixture.Create<Provenance>());

            var houseNumberFiveWasMigrated = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(20),
                new HouseNumber("5"),
                AddressStatus.Proposed,
                newPosition);
            var houseNumberSevenWasMigrated = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(30),
                new HouseNumber("7"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(originalPosition));

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberThreeWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasProposedV2>(new Envelope(boxNumberThreeWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberSevenWasMigrated, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberFiveWasMigrated, new Dictionary<string, object>())),
                    new Envelope<AddressHouseNumberWasReaddressed>(new Envelope(streetNameWasReaddressed, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var houseNumberThree = await ct.AddressWmsItemsV3.FindAsync(houseNumberThreeWasProposed.AddressPersistentLocalId);
                    houseNumberThree.Should().NotBeNull();
                    var houseNumberFive = await ct.AddressWmsItemsV3.FindAsync(houseNumberFiveWasMigrated.AddressPersistentLocalId);
                    houseNumberFive.Should().NotBeNull();
                    var houseNumberSeven = await ct.AddressWmsItemsV3.FindAsync(houseNumberSevenWasMigrated.AddressPersistentLocalId);
                    houseNumberSeven.Should().NotBeNull();
                    var boxNumberThree = await ct.AddressWmsItemsV3.FindAsync(boxNumberThreeWasProposed.AddressPersistentLocalId);
                    boxNumberThree.Should().NotBeNull();

                    houseNumberThree!.HouseNumberLabel.Should().Be("3-5");
                    houseNumberFive!.HouseNumberLabel.Should().Be("3-5");
                    houseNumberSeven!.HouseNumberLabel.Should().Be("7");
                    boxNumberThree!.HouseNumberLabel.Should().Be("3");

                    houseNumberThree.HouseNumberLabelLength.Should().Be(3);
                    houseNumberFive.HouseNumberLabelLength.Should().Be(3);
                    houseNumberSeven.HouseNumberLabelLength.Should().Be(1);
                    boxNumberThree.HouseNumberLabelLength.Should().Be(1);

                    houseNumberThree.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    houseNumberFive.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    houseNumberSeven.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    boxNumberThree.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasProposedBecauseOfReaddress()
        {
            var houseNumberOne = _fixture.Create<AddressWasProposedBecauseOfReaddress>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(1))
                .WithParentAddressPersistentLocalId(null)
                .WithHouseNumber(new HouseNumber("1"))
                .WithBoxNumber(null);
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOne.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasProposedBecauseOfReaddress>(new Envelope(houseNumberOne, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOne.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("1-3");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRejectedBecauseOfReaddress()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Rejected,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            var houseNumberOneWasRejected = _fixture.Create<AddressWasRejectedBecauseOfReaddress>();
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
                    new Envelope<AddressWasRejectedBecauseOfReaddress>(new Envelope(houseNumberOneWasRejected, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRetiredBecauseOfReaddress()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Retired,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            var houseNumberOneWasApproved = _fixture.Create<AddressWasApproved>();
            var houseNumberOneWasRetired = _fixture.Create<AddressWasRetiredBecauseOfReaddress>();
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
                    new Envelope<AddressWasRetiredBecauseOfReaddress>(new Envelope(houseNumberOneWasRetired, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-2");
                    two!.HouseNumberLabel.Should().Be("1-2");
                    three!.HouseNumberLabel.Should().Be("3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(1);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRemovedV2()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasRemoved = _fixture.Create<AddressWasRemovedV2>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberTwoBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("2"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberTwo.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwoBoxOne, new Dictionary<string, object>())),
                    new Envelope<AddressWasRemovedV2>(new Envelope(houseNumberOneWasRemoved, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var twoBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwoBoxOne.AddressPersistentLocalId);
                    twoBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().BeNull();
                    two!.HouseNumberLabel.Should().Be("2-3");
                    three!.HouseNumberLabel.Should().Be("2-3");
                    twoBoxOne!.HouseNumberLabel.Should().Be("2");

                    one.HouseNumberLabelLength.Should().BeNull();
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                    twoBoxOne.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRemovedBecauseStreetNameWasRemoved()
        {
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(new HouseNumber("1"));
            var houseNumberOneWasRemoved = _fixture.Create<AddressWasRemovedBecauseStreetNameWasRemoved>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberTwoBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("2"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberTwo.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwoBoxOne, new Dictionary<string, object>())),
                    new Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>(new Envelope(houseNumberOneWasRemoved, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var twoBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwoBoxOne.AddressPersistentLocalId);
                    twoBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().BeNull();
                    two!.HouseNumberLabel.Should().Be("2-3");
                    three!.HouseNumberLabel.Should().Be("2-3");
                    twoBoxOne!.HouseNumberLabel.Should().Be("2");

                    one.HouseNumberLabelLength.Should().BeNull();
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                    twoBoxOne.HouseNumberLabelLength.Should().Be(1);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressWasRemovedBecauseHouseNumberWasRemoved()
        {
            var houseNumberOneWasProposed = CreateHouseNumberOneWasProposed();
            var houseNumberOneWasRemoved = _fixture.Create<AddressWasRemovedBecauseHouseNumberWasRemoved>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("2"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberTwoBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("2"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberTwo.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
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
                    new Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>(new Envelope(houseNumberOneWasRemoved, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwoBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var twoBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwoBoxOne.AddressPersistentLocalId);
                    twoBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().BeNull();
                    two!.HouseNumberLabel.Should().Be("2-3");
                    three!.HouseNumberLabel.Should().Be("2-3");
                    twoBoxOne!.HouseNumberLabel.Should().Be("2");

                    one.HouseNumberLabelLength.Should().BeNull();
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                    twoBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    twoBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        [Fact]
        public async Task WithBoxNumberOnDifferentPosition_WhenAddressRemovalWasCorrected()
        {
            var houseNumberOne = new HouseNumber("1");
            var houseNumberOneWasProposed = _fixture.Create<AddressWasProposedV2>()
                .WithHouseNumber(houseNumberOne)
                .WithParentAddressPersistentLocalId(null)
                .WithBoxNumber(null);
            var houseNumberOneWasRemoved = _fixture.Create<AddressWasRemovedV2>();
            var houseNumberTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId + 1),
                new HouseNumber("2"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(houseNumberTwo.AddressPersistentLocalId + 1),
                new HouseNumber("3"),
                AddressStatus.Proposed,
                new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberOneRemovalWasCorrected = _fixture.Create<AddressRemovalWasCorrected>()
                .WithAddressPersistentLocalId(new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId))
                .WithHouseNumber(houseNumberOne)
                .WithBoxNumber(null)
                .WithStatus(AddressStatus.Proposed)
                .WithGeometry(new ExtendedWkbGeometry(houseNumberOneWasProposed.ExtendedWkbGeometry));
            var houseNumberOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(11),
                new HouseNumber("1"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(houseNumberOneWasProposed.AddressPersistentLocalId),
                AddressStatus.Proposed,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());

            await Sut
                .Given(
                    new Envelope<AddressWasProposedV2>(new Envelope(houseNumberOneWasProposed, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasRemovedV2>(new Envelope(houseNumberOneWasRemoved, new Dictionary<string, object>())),
                    new Envelope<AddressRemovalWasCorrected>(new Envelope(houseNumberOneRemovalWasCorrected, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(houseNumberOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var one = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneWasProposed.AddressPersistentLocalId);
                    one.Should().NotBeNull();
                    var two = await ct.AddressWmsItemsV3.FindAsync(houseNumberTwo.AddressPersistentLocalId);
                    two.Should().NotBeNull();
                    var three = await ct.AddressWmsItemsV3.FindAsync(houseNumberThree.AddressPersistentLocalId);
                    three.Should().NotBeNull();
                    var oneBoxOne = await ct.AddressWmsItemsV3.FindAsync(houseNumberOneBoxOne.AddressPersistentLocalId);
                    oneBoxOne.Should().NotBeNull();

                    one!.HouseNumberLabel.Should().Be("1-3");
                    two!.HouseNumberLabel.Should().Be("1-3");
                    three!.HouseNumberLabel.Should().Be("1-3");
                    oneBoxOne!.HouseNumberLabel.Should().Be("1");

                    one.HouseNumberLabelLength.Should().Be(3);
                    two.HouseNumberLabelLength.Should().Be(3);
                    three.HouseNumberLabelLength.Should().Be(3);
                    oneBoxOne.HouseNumberLabelLength.Should().Be(1);

                    one.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    two.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    three.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    oneBoxOne.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }
    }
}
