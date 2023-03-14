namespace AddressRegistry.Tests.BackOffice.Infrastructure
{
    using System;
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
    using Be.Vlaanderen.Basisregisters.Sqs.Exceptions;
    using FluentAssertions;
    using global::AutoFixture;
    using Moq;
    using StreetName;
    using StreetName.Events;
    using Xunit;
    using Xunit.Abstractions;

    public class IfMatchHeaderValidatorTests : AddressRegistryTest
    {
        private readonly TestBackOfficeContext _backOfficeContext;

        public IfMatchHeaderValidatorTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedStreetNamePersistentLocalId());
            Fixture.Customize(new WithFixedAddressPersistentLocalId());
            Fixture.Customize(new WithFixedMunicipalityId());

            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task WhenValidIfMatchHeader()
        {
            // Arrange
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentLocalId);

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

            var sut = new IfMatchHeaderValidator(_backOfficeContext, streetNames.Object);

            // Act
            var result = await sut.IsValid(
                expectedEtag.ToString(),
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

            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentLocalId);

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

            var sut = new IfMatchHeaderValidator(_backOfficeContext, streetNames.Object);

            // Act
            var result = await sut.IsValid(
                "NON MATCHING ETAG",
                addressPersistentLocalId,
                CancellationToken.None);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void WhenStreetNameAddressRelationCannotBeFound_ThenThrowsAggregateIdIsNotFoundException()
        {
            var sut = new IfMatchHeaderValidator(_backOfficeContext, Mock.Of<IStreetNames>());

            // Act
            Func<Task> act = async () => await sut.IsValid(
                "A tag",
                Fixture.Create<AddressPersistentLocalId>(),
                CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<AggregateIdIsNotFoundException>();
        }

        [Theory]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData(null)]
        public async Task WhenNoIfMatchHeader(string? etag)
        {
            // Arrange
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();
            var sut = new IfMatchHeaderValidator(_backOfficeContext, Container.Resolve<IStreetNames>());

            // Act
            var result = await sut.IsValid(
                etag,
                addressPersistentLocalId,
                CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }
    }
}
