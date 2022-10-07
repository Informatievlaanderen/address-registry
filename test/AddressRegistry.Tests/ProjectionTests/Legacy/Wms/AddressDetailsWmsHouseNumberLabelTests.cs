namespace AddressRegistry.Tests.ProjectionTests.Legacy.Wms
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading.Tasks;
    using Address;
    using Address.Events;
    using AddressRegistry.Projections.Wms;
    using AddressRegistry.Projections.Wms.AddressDetail;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AspNetCore.Mvc.Formatters.Json;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector.Testing;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Mvc.NewtonsoftJson;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using Newtonsoft.Json;
    using Xunit;
    using Xunit.Abstractions;

    public class AddressDetailsWmsHouseNumberLabelTests : ProjectionTest<WmsContext, AddressDetailProjections>
    {
        private readonly Fixture _fixture;

        public AddressDetailsWmsHouseNumberLabelTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            LogExtensions.LogSerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;

            _fixture = new Fixture();
            _fixture.Customize(new NodaTimeCustomization());
            _fixture.Customize(new SetProvenanceImplementationsCallSetProvenance());
        }

        [Fact]
        public async Task AddressBecameComplete()
        {
            var ctx = CreateContext();
            
            var houseNumber1 = CreateAddress("1", 111, 222);
            ctx.AddressDetail.Add(houseNumber1);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6A = CreateAddress("6A", 111, 222, addressId: addressId, complete: false);
            ctx.AddressDetail.Add(houseNumber6A);

            await ctx.SaveChangesAsync();

            var @event = new AddressBecameComplete(addressId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1-6A"),
                        CreateAddress(houseNumber6A.HouseNumber!, 111, 222, "1-6A")),
                ctx);
        }

        [Fact]
        public async Task AddressBecameIncomplete()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222, "1-6A");
            ctx.AddressDetail.Add(houseNumber1);

            var houseNumber4 = CreateAddress("4", 111, 222, "1-6A");
            ctx.AddressDetail.Add(houseNumber4);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6A = CreateAddress("6A", 111, 222, "1-6A", addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6A);

            await ctx.SaveChangesAsync();

            var @event = new AddressBecameIncomplete(addressId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1-4"),
                        CreateAddress(houseNumber4.HouseNumber!, 111, 222, "1-4"),
                        CreateAddress(houseNumber6A.HouseNumber!, 111, 222)),
                ctx);
        }

        [Fact]
        public async Task AddressWasRemoved()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222, "1-6A");
            ctx.AddressDetail.Add(houseNumber1);

            var houseNumber4 = CreateAddress("4", 111, 222, "1-6A");
            ctx.AddressDetail.Add(houseNumber4);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6A = CreateAddress("6A", 111, 222, "1-6A", addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6A);

            await ctx.SaveChangesAsync();

            var @event = new AddressWasRemoved(addressId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1-4"),
                        CreateAddress(houseNumber4.HouseNumber!, 111, 222, "1-4"),
                        CreateAddress(houseNumber6A.HouseNumber!, 111, 222)),
                ctx);
        }

        [Fact]
        public async Task AddressBecameCurrent()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222);
            ctx.AddressDetail.Add(houseNumber1);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6A = CreateAddress("6A", 111, 222, addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6A);

            var houseNumber6C = CreateAddress("6C", 111, 222);
            ctx.AddressDetail.Add(houseNumber6C);

            var houseNumber10 = CreateAddress("10", 111, 222, status: AddressDetailProjections.AdresStatusInGebruik);
            ctx.AddressDetail.Add(houseNumber10);

            await ctx.SaveChangesAsync();

            var @event = new AddressBecameCurrent(addressId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1-6C"),
                        CreateAddress(houseNumber6A.HouseNumber!, 111, 222, "6A-10"),
                        CreateAddress(houseNumber6C.HouseNumber!, 111, 222, "1-6C"),
                        CreateAddress(houseNumber10.HouseNumber!, 111, 222, "6A-10")),
                ctx);
        }

        [Fact]
        public async Task AddressWasCorrectedToCurrent()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222);
            ctx.AddressDetail.Add(houseNumber1);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6A = CreateAddress("6A", 111, 222, addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6A);

            var houseNumber6C = CreateAddress("6C", 111, 222);
            ctx.AddressDetail.Add(houseNumber6C);

            var houseNumber10 = CreateAddress("10", 111, 222, status: AddressDetailProjections.AdresStatusInGebruik);
            ctx.AddressDetail.Add(houseNumber10);

            await ctx.SaveChangesAsync();

            var @event = new AddressWasCorrectedToCurrent(addressId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1-6C"),
                        CreateAddress(houseNumber6A.HouseNumber!, 111, 222, "6A-10"),
                        CreateAddress(houseNumber6C.HouseNumber!, 111, 222, "1-6C"),
                        CreateAddress(houseNumber10.HouseNumber!, 111, 222, "6A-10")),
                ctx);
        }

        [Fact]
        public async Task AddressStatusWasCorrectedToRemoved()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222);
            ctx.AddressDetail.Add(houseNumber1);

            var houseNumber4 = CreateAddress("4", 111, 222);
            ctx.AddressDetail.Add(houseNumber4);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6A = CreateAddress("6A", 111, 222, addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6A);

            await ctx.SaveChangesAsync();

            var @event = new AddressStatusWasCorrectedToRemoved(addressId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1-4"),
                        CreateAddress(houseNumber4.HouseNumber!, 111, 222, "1-4"),
                        CreateAddress(houseNumber6A.HouseNumber!, 111, 222)),
                ctx);
        }

        [Fact]
        public async Task AddressStatusWasRemoved()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222);
            ctx.AddressDetail.Add(houseNumber1);

            var houseNumber4 = CreateAddress("4", 111, 222);
            ctx.AddressDetail.Add(houseNumber4);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6A = CreateAddress("6A", 111, 222, addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6A);

            await ctx.SaveChangesAsync();

            var @event = new AddressStatusWasRemoved(addressId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1-4"),
                        CreateAddress(houseNumber4.HouseNumber!, 111, 222, "1-4"),
                        CreateAddress(houseNumber6A.HouseNumber!, 111, 222)),
                ctx);
        }

        [Fact]
        public async Task AddressWasProposed()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222, status: AddressDetailProjections.AdresStatusInGebruik);
            ctx.AddressDetail.Add(houseNumber1);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6A = CreateAddress("6A", 111, 222, status: AddressDetailProjections.AdresStatusInGebruik, addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6A);

            var houseNumber6C = CreateAddress("6C", 111, 222, status: AddressDetailProjections.AdresStatusInGebruik);
            ctx.AddressDetail.Add(houseNumber6C);

            var houseNumber10 = CreateAddress("10", 111, 222, status: AddressDetailProjections.AdresStatusVoorgesteld);
            ctx.AddressDetail.Add(houseNumber10);

            await ctx.SaveChangesAsync();

            var @event = new AddressWasProposed(addressId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1-6C"),
                        CreateAddress(houseNumber6A.HouseNumber!, 111, 222, "6A-10"),
                        CreateAddress(houseNumber6C.HouseNumber!, 111, 222, "1-6C"),
                        CreateAddress(houseNumber10.HouseNumber!, 111, 222, "6A-10")),
                ctx);
        }

        [Fact]
        public async Task AddressWasCorrectedToProposed()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222, status: AddressDetailProjections.AdresStatusInGebruik);
            ctx.AddressDetail.Add(houseNumber1);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6A = CreateAddress("6A", 111, 222, status: AddressDetailProjections.AdresStatusInGebruik,addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6A);

            var houseNumber6C = CreateAddress("6C", 111, 222, status: AddressDetailProjections.AdresStatusInGebruik);
            ctx.AddressDetail.Add(houseNumber6C);

            var houseNumber10 = CreateAddress("10", 111, 222, status: AddressDetailProjections.AdresStatusVoorgesteld);
            ctx.AddressDetail.Add(houseNumber10);

            await ctx.SaveChangesAsync();

            var @event = new AddressWasCorrectedToProposed(addressId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1-6C"),
                        CreateAddress(houseNumber6A.HouseNumber!, 111, 222, "6A-10"),
                        CreateAddress(houseNumber6C.HouseNumber!, 111, 222, "1-6C"),
                        CreateAddress(houseNumber10.HouseNumber!, 111, 222, "6A-10")),
                ctx);
        }

        [Fact]
        public async Task AddressWasRetired()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222, status: AddressDetailProjections.AdresStatusInGebruik);
            ctx.AddressDetail.Add(houseNumber1);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6A = CreateAddress("6A", 111, 222, status: AddressDetailProjections.AdresStatusInGebruik, addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6A);

            var houseNumber6C = CreateAddress("6C", 111, 222, status: AddressDetailProjections.AdresStatusInGebruik);
            ctx.AddressDetail.Add(houseNumber6C);

            var houseNumber10 = CreateAddress("10", 111, 222, status: AddressDetailProjections.AdresStatusGehistoreerd);
            ctx.AddressDetail.Add(houseNumber10);

            await ctx.SaveChangesAsync();

            var @event = new AddressWasRetired(addressId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1-6C"),
                        CreateAddress(houseNumber6A.HouseNumber!, 111, 222, "6A-10"),
                        CreateAddress(houseNumber6C.HouseNumber!, 111, 222, "1-6C"),
                        CreateAddress(houseNumber10.HouseNumber!, 111, 222, "6A-10")),
                ctx);
        }

        [Fact]
        public async Task AddressWasCorrectedToRetired()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222, status: AddressDetailProjections.AdresStatusInGebruik);
            ctx.AddressDetail.Add(houseNumber1);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6A = CreateAddress("6A", 111, 222, status: AddressDetailProjections.AdresStatusInGebruik, addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6A);

            var houseNumber6C = CreateAddress("6C", 111, 222, status: AddressDetailProjections.AdresStatusInGebruik);
            ctx.AddressDetail.Add(houseNumber6C);

            var houseNumber10 = CreateAddress("10", 111, 222, status: AddressDetailProjections.AdresStatusGehistoreerd);
            ctx.AddressDetail.Add(houseNumber10);

            await ctx.SaveChangesAsync();

            var @event = new AddressWasCorrectedToRetired(addressId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1-6C"),
                        CreateAddress(houseNumber6A.HouseNumber!, 111, 222, "6A-10"),
                        CreateAddress(houseNumber6C.HouseNumber!, 111, 222, "1-6C"),
                        CreateAddress(houseNumber10.HouseNumber!, 111, 222, "6A-10")),
                ctx);
        }

        [Fact]
        public async Task AddressWasPositioned()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222);
            ctx.AddressDetail.Add(houseNumber1);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6A = CreateAddress("6A", 111, 222, addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6A);

            var houseNumber6C = CreateAddress("6C", 333, 444);
            ctx.AddressDetail.Add(houseNumber6C);

            await ctx.SaveChangesAsync();

            var @event = new AddressWasPositioned(
                addressId,
                new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Building,
                    new ExtendedWkbGeometry(CreateExtendedWkb(333, 444))));
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1"),
                        CreateAddress(houseNumber6A.HouseNumber!, 333, 444, "6A-6C"),
                        CreateAddress(houseNumber6C.HouseNumber!, 333, 444, "6A-6C")),
                ctx);
        }

        [Fact]
        public async Task AddressPositionWasCorrected()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222);
            ctx.AddressDetail.Add(houseNumber1);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6A = CreateAddress("6A", 111, 222, addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6A);

            var houseNumber6C = CreateAddress("6C", 333, 444);
            ctx.AddressDetail.Add(houseNumber6C);

            await ctx.SaveChangesAsync();

            var @event = new AddressPositionWasCorrected(
                addressId,
                new AddressGeometry(
                    GeometryMethod.AppointedByAdministrator,
                    GeometrySpecification.Building,
                    new ExtendedWkbGeometry(CreateExtendedWkb(333, 444))));
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1"),
                        CreateAddress(houseNumber6A.HouseNumber!, 333, 444, "6A-6C"),
                        CreateAddress(houseNumber6C.HouseNumber!, 333, 444, "6A-6C")),
                ctx);
        }

        [Fact]
        public async Task AddressPositionWasRemoved()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222);
            ctx.AddressDetail.Add(houseNumber1);

            var houseNumber6A = CreateAddress("6A", 111, 222);
            ctx.AddressDetail.Add(houseNumber6A);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6C = CreateAddress("6C", 111, 222, addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6C);

            await ctx.SaveChangesAsync();

            var @event = new AddressPositionWasRemoved(addressId);
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1-6A"),
                        CreateAddress(houseNumber6A.HouseNumber!, 111, 222, "1-6A"),
                        CreateAddress(houseNumber6C.HouseNumber!, null, null)),
                ctx);
        }

        [Fact]
        public async Task AddressHouseNumberWasChanged()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222);
            ctx.AddressDetail.Add(houseNumber1);

            var houseNumber6A = CreateAddress("6A", 111, 222);
            ctx.AddressDetail.Add(houseNumber6A);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6C = CreateAddress("6C", 111, 222, addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6C);

            await ctx.SaveChangesAsync();

            var @event = new AddressHouseNumberWasChanged(addressId, new HouseNumber("7C"));
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1-7C"),
                        CreateAddress(houseNumber6A.HouseNumber!, 111, 222, "1-7C"),
                        CreateAddress(houseNumber6C.HouseNumber!, 111, 222, "1-7C")),
                ctx);
        }

        [Fact]
        public async Task AddressHouseNumberWasCorrected()
        {
            var ctx = CreateContext();

            var houseNumber1 = CreateAddress("1", 111, 222);
            ctx.AddressDetail.Add(houseNumber1);

            var houseNumber6A = CreateAddress("6A", 111, 222);
            ctx.AddressDetail.Add(houseNumber6A);

            var addressId = _fixture.Create<AddressId>();
            var houseNumber6C = CreateAddress("6C", 111, 222, addressId: addressId);
            ctx.AddressDetail.Add(houseNumber6C);

            await ctx.SaveChangesAsync();

            var @event = new AddressHouseNumberWasCorrected(addressId, new HouseNumber("7C"));
            ((ISetProvenance)@event).SetProvenance(_fixture.Create<Provenance>());

            await Assert(
                Given(@event)
                    .Expect(
                        new HouseNumberLabelComparer(),
                        _ => ctx.AddressDetail,
                        CreateAddress(houseNumber1.HouseNumber!, 111, 222, "1-7C"),
                        CreateAddress(houseNumber6A.HouseNumber!, 111, 222, "1-7C"),
                        CreateAddress(houseNumber6C.HouseNumber!, 111, 222, "1-7C")),
                ctx);
        }

        private AddressDetailItem CreateAddress(
            string houseNumber,
            int? positionX,
            int? positionY,
            string? houseNumberLabel = null,
            string? status = null,
            AddressId? addressId = null,
            bool complete = true)
        {
            var a = new AddressDetailItem
            {
                AddressId = addressId ?? _fixture.Create<AddressId>(),
                HouseNumber = houseNumber,
                Status = status ?? AddressDetailProjections.AdresStatusVoorgesteld,
                Complete = complete
            };

            a.SetHouseNumberLabel(houseNumberLabel);
            a.SetPosition(positionX != null && positionY != null ? new Point(positionX.Value, positionY.Value) : null);
            return a;
        }

        private byte[] CreateExtendedWkb(int x, int y)
        {
            var wkbWriter = new WKBWriter { Strict = false, HandleSRID = true };
            var wktReader = new WKTReader().Read($"POINT ({x} {y})");
            wktReader.SRID = SpatialReferenceSystemId.Lambert72;
            return wkbWriter.Write(wktReader);
        }

        protected override WmsContext CreateContext(DbContextOptions<WmsContext> options) => new(options);

        private readonly WKBReader _wkbReader = WKBReaderFactory.CreateForLegacy();
        protected override AddressDetailProjections CreateProjection() => new(_wkbReader);
    }

    public class HouseNumberLabelComparer : IEntityComparer<AddressDetailItem>
    {
        [SuppressMessage("ReSharper", "CompareOfFloatsByEqualityOperator")]
        public IEnumerable<EntityComparisonDifference<AddressDetailItem>> Compare(
            AddressDetailItem expected,
            AddressDetailItem actual)
        {
            if (expected.HouseNumberLabel != actual.HouseNumberLabel)
            {
                yield return new EntityComparisonDifference<AddressDetailItem>(expected, actual,
                    $"Expected HouseNumberLabel to be {expected.HouseNumberLabel} but found {actual.HouseNumberLabel}");
            }

            if (expected.HouseNumberLabelLength != actual.HouseNumberLabelLength)
            {
                yield return new EntityComparisonDifference<AddressDetailItem>(expected, actual,
                    $"Expected HouseNumberLabelLength to be {expected.HouseNumberLabelLength} but found {actual.HouseNumberLabelLength}");
            }

            if (expected.PositionX != actual.PositionX
                || expected.PositionY != actual.PositionY)
            {
                yield return new EntityComparisonDifference<AddressDetailItem>(expected, actual,
                    $"Expected position {expected.PositionX}-{expected.PositionY} but was {actual.PositionX}-{actual.PositionY}");
            }

            if (expected.Position?.X != actual.Position?.X
                || expected.Position?.Y != actual.Position?.Y)
            {
                yield return new EntityComparisonDifference<AddressDetailItem>(expected, actual,
                    $"Expected position {expected.Position?.X}-{expected.Position?.Y} but was {actual.Position?.X}-{actual.Position?.Y}");
            }
        }
    }
}
