namespace AddressRegistry.Tests.BackOffice.Infrastructure
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Infrastructure;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource.Snapshotting;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;
    using global::AutoFixture;
    using Moq;
    using StreetName.Events;

    public class IfMatchHeaderValidatorTests : AddressRegistryTest
    {
        public IfMatchHeaderValidatorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());
            Fixture.Customize(new WithFixedMunicipalityId());
        }

        [Fact]
        public async Task WhenValidIfMatchHeader()
        {
            // Arrange
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var streetNames = new Mock<IStreetNames>();
            var addressWasProposedV2 = new AddressWasProposedV2(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                null,
                Fixture.Create<PostalCode>(),
                Fixture.Create<HouseNumber>(),
                null,
                GeometryMethod.DerivedFromObject,
                GeometrySpecification.Municipality,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
            ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

            streetNames.Setup(x => x.GetAsync(new StreetNameStreamId(streetNamePersistentLocalId), CancellationToken.None))
                .ReturnsAsync(() =>
                {
                    var municipality = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();
                    municipality.Initialize(new List<object>
                    {
                        new MigratedStreetNameWasImported(
                            Fixture.Create<StreetNameId>(),
                            new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                            Fixture.Create<MunicipalityId>(),
                            Fixture.Create<NisCode>(),
                            StreetNameStatus.Current),
                        addressWasProposedV2
                    });

                    return municipality;
                });

            var expectedEtag = new ETag(ETagType.Strong, addressWasProposedV2.GetHash());

            var sut = new IfMatchHeaderValidator(streetNames.Object);

            // Act
            var result = await sut.IsValid(
                expectedEtag.ToString(),
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task WhenNotValidIfMatchHeader()
        {
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var streetNames = new Mock<IStreetNames>();
            streetNames
                .Setup(x => x.GetAsync(new StreetNameStreamId(streetNamePersistentLocalId), CancellationToken.None))
                .ReturnsAsync(() =>
                {
                    var municipality = new StreetNameFactory(NoSnapshotStrategy.Instance).Create();

                    var addressWasProposedV2 = new AddressWasProposedV2(
                        streetNamePersistentLocalId,
                        addressPersistentLocalId,
                        null,
                        Fixture.Create<PostalCode>(),
                        Fixture.Create<HouseNumber>(),
                        null,
                        GeometryMethod.DerivedFromObject,
                        GeometrySpecification.Municipality,
                        GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry());
                    ((ISetProvenance)addressWasProposedV2).SetProvenance(Fixture.Create<Provenance>());

                    municipality.Initialize(new List<object>
                    {
                        new MigratedStreetNameWasImported(
                            Fixture.Create<StreetNameId>(),
                            new StreetNamePersistentLocalId(streetNamePersistentLocalId),
                            Fixture.Create<MunicipalityId>(),
                            Fixture.Create<NisCode>(),
                            StreetNameStatus.Current),
                        addressWasProposedV2
                    });

                    return municipality;
                });

            var sut = new IfMatchHeaderValidator(streetNames.Object);

            // Act
            var result = await sut.IsValid(
                "NON MATCHING ETAG",
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task WhenNoIfMatchHeader(string? etag)
        {
            // Arrange
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var sut = new IfMatchHeaderValidator(Container.Resolve<IStreetNames>());

            // Act
            var result = await sut.IsValid(
                etag,
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }
    }
}
