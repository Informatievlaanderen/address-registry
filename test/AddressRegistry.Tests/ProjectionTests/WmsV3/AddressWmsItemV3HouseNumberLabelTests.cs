namespace AddressRegistry.Tests.ProjectionTests.WmsV3
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using EventExtensions;
    using FluentAssertions;
    using global::AutoFixture;
    using NetTopologySuite.IO;
    using Projections.Wms;
    using Projections.Wms.AddressWmsItemV3;
    using Xunit;

    public class AddressWmsItemV3HouseNumberLabelTests2 : AddressWmsItemV3ProjectionTest
    {
        private readonly Fixture _fixture;
        private readonly WKBReader _wkbReader;

        public AddressWmsItemV3HouseNumberLabelTests2()
        {
            _fixture = new Fixture();
            _fixture.Customize(new WithFixedAddressPersistentLocalId());
            _fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            _fixture.Customize<AddressStatus>(_ => new WithoutUnknownStreetNameAddressStatus());
            _fixture.Customize(new WithValidHouseNumber());
            _fixture.Customize(new WithValidBoxNumber());
            _fixture.Customize(new WithExtendedWkbGeometry());
            _fixture.Customize(new InfrastructureCustomization());

            _wkbReader = WKBReaderFactory.Create();
        }

        /*
         * Case 1: Huisnummer + busnummer(s) Ooststraat 5       In gebruik (1,2) = _5_
         * Case 1: Huisnummer + busnummer(s) Ooststraat 5 bus 1 In gebruik (1,2) = _5_
         * Case 1: Huisnummer + busnummer(s) Ooststraat 5 bus 2 In gebruik (1,2) = _5_
         */
        [Fact]
        public async Task Case1()
        {
            var five = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(1),
                new HouseNumber("5"),
                AddressStatus.Current);
            var fiveBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("5"),
                new BoxNumber("1"),
                new AddressPersistentLocalId(five.AddressPersistentLocalId),
                AddressStatus.Current,
                new ExtendedWkbGeometry(five.ExtendedWkbGeometry));
            var fiveBoxTwo = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("5"),
                new BoxNumber("2"),
                new AddressPersistentLocalId(five.AddressPersistentLocalId),
                AddressStatus.Current,
                new ExtendedWkbGeometry(five.ExtendedWkbGeometry));

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(five, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(fiveBoxTwo, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(fiveBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var fiveProjection = await ct.AddressWmsItemsV3.FindAsync(five.AddressPersistentLocalId);
                    fiveProjection.Should().NotBeNull();
                    var fiveBoxOneProjection = await ct.AddressWmsItemsV3.FindAsync(fiveBoxOne.AddressPersistentLocalId);
                    fiveBoxOneProjection.Should().NotBeNull();
                    var fiveBoxTwoProjection = await ct.AddressWmsItemsV3.FindAsync(fiveBoxTwo.AddressPersistentLocalId);
                    fiveBoxTwoProjection.Should().NotBeNull();

                    fiveProjection!.HouseNumberLabel.Should().Be("5");
                    fiveBoxOneProjection!.HouseNumberLabel.Should().Be("5");
                    fiveBoxTwoProjection!.HouseNumberLabel.Should().Be("5");

                    fiveProjection.HouseNumberLabelLength.Should().Be(1);
                    fiveBoxOneProjection.HouseNumberLabelLength.Should().Be(1);
                    fiveBoxTwoProjection.HouseNumberLabelLength.Should().Be(1);

                    fiveProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                    fiveBoxOneProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                    fiveBoxTwoProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        /*
         * Case 1: Huisnummers Zuidstraat 10 In gebruik (3,4) = 10-16
         * Case 1: Huisnummers Zuidstraat 12 In gebruik (3,4) = 10-16
         * Case 1: Huisnummers Zuidstraat 16 In gebruik (3,4) = 10-16
         */
        [Fact]
        public async Task Case2()
        {
            var ten = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(1),
                new HouseNumber("10"),
                AddressStatus.Current);
            var twelve = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("12"),
                AddressStatus.Current);
            var sixteen = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("16"),
                AddressStatus.Current);

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(sixteen, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(ten, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(twelve, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var tenProjection = await ct.AddressWmsItemsV3.FindAsync(ten.AddressPersistentLocalId);
                    tenProjection.Should().NotBeNull();
                    var twelveProjection = await ct.AddressWmsItemsV3.FindAsync(twelve.AddressPersistentLocalId);
                    twelveProjection.Should().NotBeNull();
                    var sixteenProjection = await ct.AddressWmsItemsV3.FindAsync(sixteen.AddressPersistentLocalId);
                    sixteenProjection.Should().NotBeNull();

                    tenProjection!.HouseNumberLabel.Should().Be("10-16");
                    twelveProjection!.HouseNumberLabel.Should().Be("10-16");
                    sixteenProjection!.HouseNumberLabel.Should().Be("10-16");

                    tenProjection.HouseNumberLabelLength.Should().Be(5);
                    twelveProjection.HouseNumberLabelLength.Should().Be(5);
                    sixteenProjection.HouseNumberLabelLength.Should().Be(5);

                    tenProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    twelveProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    sixteenProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                });
        }

        /*
         * Case 3: Huisnummer + busnummer(s) Weststraat 28       In gebruik (5,6) = _28-30_
         * Case 3: Huisnummer + busnummer(s) Weststraat 28 bus 001 In gebruik (5,6) = _28-30_
         * Case 3: Huisnummer + busnummer(s) Weststraat 30       In gebruik (5,6) = _28-30_
         */
        [Fact]
        public async Task Case3()
        {
            var twentyEight = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(1),
                new HouseNumber("28"),
                AddressStatus.Current);
            var twentyEightBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("28"),
                new BoxNumber("001"),
                new AddressPersistentLocalId(twentyEight.AddressPersistentLocalId),
                AddressStatus.Current);
            var thirty = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("30"),
                AddressStatus.Current);

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(thirty, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(twentyEight, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(twentyEightBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var twenty8 = await ct.AddressWmsItemsV3.FindAsync(twentyEight.AddressPersistentLocalId);
                    twenty8.Should().NotBeNull();
                    var twenty8Box1 = await ct.AddressWmsItemsV3.FindAsync(twentyEightBoxOne.AddressPersistentLocalId);
                    twenty8Box1.Should().NotBeNull();
                    var thirty0 = await ct.AddressWmsItemsV3.FindAsync(thirty.AddressPersistentLocalId);
                    thirty0.Should().NotBeNull();

                    twenty8!.HouseNumberLabel.Should().Be("28-30");
                    twenty8Box1!.HouseNumberLabel.Should().Be("28-30");
                    thirty0!.HouseNumberLabel.Should().Be("28-30");

                    twenty8.HouseNumberLabelLength.Should().Be(5);
                    twenty8Box1.HouseNumberLabelLength.Should().Be(5);
                    thirty0.HouseNumberLabelLength.Should().Be(5);

                    twenty8.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                    twenty8Box1.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                    thirty0.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        /*
         * Case 4: Andere status Noordstraat 56       In gebruik (7,8) = 56
         * Case 4: Andere status Noordstraat 58       Voorgesteld (7,8) = 58
         */
        [Fact]
        public async Task Case4()
        {
            var fiftySix = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(1),
                new HouseNumber("56"),
                AddressStatus.Current);
            var fiftyEight = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("58"),
                AddressStatus.Proposed);

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(fiftySix, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(fiftyEight, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var fifty6 = await ct.AddressWmsItemsV3.FindAsync(fiftySix.AddressPersistentLocalId);
                    fifty6.Should().NotBeNull();
                    var fifty8 = await ct.AddressWmsItemsV3.FindAsync(fiftyEight.AddressPersistentLocalId);
                    fifty8.Should().NotBeNull();

                    fifty6!.HouseNumberLabel.Should().Be("56");
                    fifty8!.HouseNumberLabel.Should().Be("58");

                    fifty6.HouseNumberLabelLength.Should().Be(2);
                    fifty8.HouseNumberLabelLength.Should().Be(2);

                    fifty6.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    fifty8.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                });
        }

        /*
         * Case 5: Twee straten Bailleartstraat 2       In gebruik (10,10) = 2-4 ; 128
         * Case 5: Twee straten Bailleartstraat 4       In gebruik (10,10) = 2-4 ; 128
         * Case 5: Twee straten Kortijksesteenweg 128   In gebruik (10,10) = 2-4 ; 128
         */
        [Fact]
        public async Task Case5()
        {
            var two = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(1),
                new HouseNumber("2"),
                AddressStatus.Current);
            var four = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("4"),
                AddressStatus.Current);
            var otherStreet = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new StreetNamePersistentLocalId(two.StreetNamePersistentLocalId + 1),
                new HouseNumber("128"),
                AddressStatus.Current);

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(otherStreet, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(two, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(four, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var twoProjection = await ct.AddressWmsItemsV3.FindAsync(two.AddressPersistentLocalId);
                    twoProjection.Should().NotBeNull();
                    var fourProjection = await ct.AddressWmsItemsV3.FindAsync(four.AddressPersistentLocalId);
                    fourProjection.Should().NotBeNull();
                    var otherStreetProjection = await ct.AddressWmsItemsV3.FindAsync(otherStreet.AddressPersistentLocalId);
                    otherStreetProjection.Should().NotBeNull();

                    twoProjection!.HouseNumberLabel.Should().Be("2-4 ; 128");
                    fourProjection!.HouseNumberLabel.Should().Be("2-4 ; 128");
                    otherStreetProjection!.HouseNumberLabel.Should().Be("2-4 ; 128");

                    twoProjection.HouseNumberLabelLength.Should().Be(9);
                    fourProjection.HouseNumberLabelLength.Should().Be(9);
                    otherStreetProjection.HouseNumberLabelLength.Should().Be(9);

                    twoProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    fourProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    otherStreetProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                });
        }

        /*
         * Case 6: Twee straten + busnummer(s) Kerkstraat 13         In gebruik (8,16) = _7 ; 13_
         * Case 6: Twee straten + busnummer(s) Kerkstraat 13 bus 001 In gebruik (8,16) = _7 ; 13_
         * Case 6: Twee straten + busnummer(s) Schoolstraat 7        In gebruik (8,16) = _7 ; 13_
         */
        [Fact]
        public async Task Case6()
        {
            var thirteen = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(1),
                new HouseNumber("13"),
                AddressStatus.Current);
            var thirteenBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("13"),
                new BoxNumber("001"),
                new AddressPersistentLocalId(thirteen.AddressPersistentLocalId),
                AddressStatus.Current);
            var otherStreet = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new StreetNamePersistentLocalId(thirteen.StreetNamePersistentLocalId + 1),
                new HouseNumber("7"),
                AddressStatus.Current);

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(otherStreet, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(thirteen, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(thirteenBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var thirteenProjection = await ct.AddressWmsItemsV3.FindAsync(thirteen.AddressPersistentLocalId);
                    thirteenProjection.Should().NotBeNull();
                    var thirteenBoxOneProjection = await ct.AddressWmsItemsV3.FindAsync(thirteenBoxOne.AddressPersistentLocalId);
                    thirteenBoxOneProjection.Should().NotBeNull();
                    var sixteenProjection = await ct.AddressWmsItemsV3.FindAsync(otherStreet.AddressPersistentLocalId);
                    sixteenProjection.Should().NotBeNull();

                    thirteenProjection!.HouseNumberLabel.Should().Be("7 ; 13");
                    thirteenBoxOneProjection!.HouseNumberLabel.Should().Be("7 ; 13");
                    sixteenProjection!.HouseNumberLabel.Should().Be("7 ; 13");

                    thirteenProjection.HouseNumberLabelLength.Should().Be(6);
                    thirteenBoxOneProjection.HouseNumberLabelLength.Should().Be(6);
                    sixteenProjection.HouseNumberLabelLength.Should().Be(6);

                    thirteenProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                    thirteenBoxOneProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                    sixteenProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        /*
         * Case 7: Busnummer op andere locatie Noendries (9000 Gent) 81       In gebruik (1,1) = 81
         * Case 7: Busnummer op andere locatie Noendries (9000 Gent) 81 bus 001 In gebruik (6,6) = _81_
         */
        [Fact]
        public async Task Case7()
        {
            var eightyOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(1),
                new HouseNumber("81"),
                AddressStatus.Current);
            var eightyOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("81"),
                new BoxNumber("001"),
                new AddressPersistentLocalId(eightyOne.AddressPersistentLocalId),
                AddressStatus.Current,
                GeometryHelpers.CreateEwkbFromWkt("POINT(6 6)"));

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(eightyOne, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(eightyOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var eightyOneProjection = await ct.AddressWmsItemsV3.FindAsync(eightyOne.AddressPersistentLocalId);
                    eightyOneProjection.Should().NotBeNull();
                    var eightyOneBoxOneProjection = await ct.AddressWmsItemsV3.FindAsync(eightyOneBoxOne.AddressPersistentLocalId);
                    eightyOneBoxOneProjection.Should().NotBeNull();

                    eightyOneProjection!.HouseNumberLabel.Should().Be("81");
                    eightyOneBoxOneProjection!.HouseNumberLabel.Should().Be("81");

                    eightyOneProjection.HouseNumberLabelLength.Should().Be(2);
                    eightyOneBoxOneProjection.HouseNumberLabelLength.Should().Be(2);

                    eightyOneProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    eightyOneBoxOneProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        /*
         * Case 8: Busnummer op andere locatie. Ander huisnummer op locatie van huisnummer Noendries (9000 Gent) 79          In gebruik (1,1) = 79-81
         * Case 8: Busnummer op andere locatie. Ander huisnummer op locatie van huisnummer Noendries (9000 Gent) 81          In gebruik (1,1) = 79-81
         * Case 8: Busnummer op andere locatie. Ander huisnummer op locatie van huisnummer Noendries (9000 Gent) 81 bus 0101 In gebruik (6,6) = _81_
         */
        [Fact]
        public async Task Case8()
        {
            var seventyNine = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(1),
                new HouseNumber("79"),
                AddressStatus.Current);
            var eightyOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("81"),
                AddressStatus.Current);
            var eightyOneBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("81"),
                new BoxNumber("0101"),
                new AddressPersistentLocalId(eightyOne.AddressPersistentLocalId),
                AddressStatus.Current,
                GeometryHelpers.CreateEwkbFromWkt("POINT(6 6)"));

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(seventyNine, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(eightyOne, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(eightyOneBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var seventyNineProjection = await ct.AddressWmsItemsV3.FindAsync(seventyNine.AddressPersistentLocalId);
                    seventyNineProjection.Should().NotBeNull();
                    var eightyOneProjection = await ct.AddressWmsItemsV3.FindAsync(eightyOne.AddressPersistentLocalId);
                    eightyOneProjection.Should().NotBeNull();
                    var eightyOneBoxOneProjection = await ct.AddressWmsItemsV3.FindAsync(eightyOneBoxOne.AddressPersistentLocalId);
                    eightyOneBoxOneProjection.Should().NotBeNull();

                    seventyNineProjection!.HouseNumberLabel.Should().Be("79-81");
                    eightyOneProjection!.HouseNumberLabel.Should().Be("79-81");
                    eightyOneBoxOneProjection!.HouseNumberLabel.Should().Be("81");

                    seventyNineProjection.HouseNumberLabelLength.Should().Be(5);
                    eightyOneProjection.HouseNumberLabelLength.Should().Be(5);
                    eightyOneBoxOneProjection.HouseNumberLabelLength.Should().Be(2);

                    seventyNineProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    eightyOneProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    eightyOneBoxOneProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        /*
         * Case 9: Busnummer op andere locatie + ander huisnummer op de andere locatie Zonnestraat 33         In gebruik (8,8) = 33
         * Case 9: Busnummer op andere locatie + ander huisnummer op de andere locatie Zonnestraat 33 bus 001 In gebruik (44,44) = _33-35_
         * Case 9: Busnummer op andere locatie + ander huisnummer op de andere locatie Zonnestraat 35         In gebruik (44,44) = _33-35_
         */
        [Fact]
        public async Task Case9()
        {
            var thirtyThree = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(1),
                new HouseNumber("33"),
                AddressStatus.Current);
            var thirtyThreeBoxOne = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(2),
                new HouseNumber("33"),
                new BoxNumber("001"),
                new AddressPersistentLocalId(thirtyThree.AddressPersistentLocalId),
                AddressStatus.Current,
                GeometryHelpers.CreateEwkbFromWkt("POINT(44 44)"));
            var thirtyFive = CreateAddressWasMigratedToStreetName(
                new AddressPersistentLocalId(3),
                new HouseNumber("35"),
                AddressStatus.Current,
                GeometryHelpers.CreateEwkbFromWkt("POINT(44 44)"));

            await Sut
                .Given(
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(thirtyFive, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(thirtyThree, new Dictionary<string, object>())),
                    new Envelope<AddressWasMigratedToStreetName>(new Envelope(thirtyThreeBoxOne, new Dictionary<string, object>())))
                .Then(async ct =>
                {
                    var thirtyThreeProjection = await ct.AddressWmsItemsV3.FindAsync(thirtyThree.AddressPersistentLocalId);
                    thirtyThreeProjection.Should().NotBeNull();
                    var thirtyThreeBoxOneProjection = await ct.AddressWmsItemsV3.FindAsync(thirtyThreeBoxOne.AddressPersistentLocalId);
                    thirtyThreeBoxOneProjection.Should().NotBeNull();
                    var thirtyFiveProjection = await ct.AddressWmsItemsV3.FindAsync(thirtyFive.AddressPersistentLocalId);
                    thirtyFiveProjection.Should().NotBeNull();

                    thirtyThreeProjection!.HouseNumberLabel.Should().Be("33");
                    thirtyThreeBoxOneProjection!.HouseNumberLabel.Should().Be("33-35");
                    thirtyFiveProjection!.HouseNumberLabel.Should().Be("33-35");

                    thirtyThreeProjection.HouseNumberLabelLength.Should().Be(2);
                    thirtyThreeBoxOneProjection.HouseNumberLabelLength.Should().Be(5);
                    thirtyFiveProjection.HouseNumberLabelLength.Should().Be(5);

                    thirtyThreeProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithoutBoxNumbersOnSamePosition);
                    thirtyThreeBoxOneProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                    thirtyFiveProjection.LabelType.Should().Be(WmsAddressLabelType.HouseNumberWithBoxNumbersOnSamePosition);
                });
        }

        protected override AddressWmsItemV3Projections CreateProjection()
            =>  new AddressWmsItemV3Projections(_wkbReader, new HouseNumberLabelUpdaterUpdater());

        private AddressWasMigratedToStreetName CreateAddressWasMigratedToStreetName(
            AddressPersistentLocalId addressPersistentLocalId,
            HouseNumber houseNumber,
            AddressStatus addressStatus,
            ExtendedWkbGeometry? position = null)
        {
            var @event = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithAddressPersistentLocalId(addressPersistentLocalId)
                .WithParentAddressPersistentLocalId(null)
                .WithHouseNumber(houseNumber)
                .WithStatus(addressStatus)
                .WithBoxNumber(null)
                .WithNotRemoved();

            if (position is not null)
            {
                @event = @event.WithPosition(position);
            }

            return @event;
        }

        private AddressWasMigratedToStreetName CreateAddressWasMigratedToStreetName(
            AddressPersistentLocalId addressPersistentLocalId,
            HouseNumber houseNumber,
            BoxNumber? boxNumber,
            AddressPersistentLocalId? parentAddressPersistentLocalId,
            AddressStatus addressStatus,
            ExtendedWkbGeometry? position = null)
        {
            var @event = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithAddressPersistentLocalId(addressPersistentLocalId)
                .WithParentAddressPersistentLocalId(parentAddressPersistentLocalId)
                .WithHouseNumber(houseNumber)
                .WithStatus(addressStatus)
                .WithBoxNumber(boxNumber)
                .WithNotRemoved();

            if (position is not null)
            {
                @event = @event.WithPosition(position);
            }

            return @event;
        }

        private AddressWasMigratedToStreetName CreateAddressWasMigratedToStreetName(
            AddressPersistentLocalId addressPersistentLocalId,
            StreetNamePersistentLocalId? streetNamePersistentLocalId,
            HouseNumber houseNumber,
            AddressStatus addressStatus,
            ExtendedWkbGeometry? position = null)
        {
            var @event = _fixture.Create<AddressWasMigratedToStreetName>()
                .WithAddressPersistentLocalId(addressPersistentLocalId)
                .WithParentAddressPersistentLocalId(null)
                .WithHouseNumber(houseNumber)
                .WithStatus(addressStatus)
                .WithBoxNumber(null)
                .WithNotRemoved();

            if (streetNamePersistentLocalId is not null)
                @event = @event.WithStreetNamePersistentLocalId(streetNamePersistentLocalId);

            if (position is not null)
            {
                @event = @event.WithPosition(position);
            }

            return @event;
        }
    }
}
