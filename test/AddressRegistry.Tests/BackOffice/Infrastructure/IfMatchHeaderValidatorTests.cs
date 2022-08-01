namespace AddressRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Infrastructure;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentAssertions;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;
    using global::AutoFixture;
    using StreetName.Commands;
    using StreetName.Events;

    public class IfMatchHeaderValidatorTests : AddressRegistryBackOfficeTest
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

            var nisCode = new NisCode("12345");
            var postalCode = new PostalCode("2018");
            var houseNumber = new HouseNumber("11");

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                nisCode);

            ProposeAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                postalCode,
                Fixture.Create<MunicipalityId>(),
                houseNumber,
                null);

            var approveAddress = Fixture.Create<ApproveAddress>();
            DispatchArrangeCommand(approveAddress);

            var lastEvent = new AddressWasApproved(streetNamePersistentLocalId, addressPersistentLocalId);
            ((ISetProvenance)lastEvent).SetProvenance(approveAddress.Provenance);

            var validEtag = new ETag(ETagType.Strong, lastEvent.GetHash());
            var sut = new IfMatchHeaderValidator(Container.Resolve<IStreetNames>());

            // Act
            var result = await sut.IsValid(
                validEtag.ToString(),
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                CancellationToken.None);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task WhenNotValidIfMatchHeader()
        {
            // Arrange
            var streetNamePersistentLocalId = Fixture.Create<StreetNamePersistentLocalId>();
            var addressPersistentLocalId = Fixture.Create<AddressPersistentLocalId>();

            var nisCode = new NisCode("12345");
            var postalCode = new PostalCode("2018");
            var houseNumber = new HouseNumber("11");

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                nisCode);

            ProposeAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                postalCode,
                Fixture.Create<MunicipalityId>(),
                houseNumber,
                null);

            var invalidEtag = new ETag(ETagType.Strong, "NotValidHash");
            var sut = new IfMatchHeaderValidator(Container.Resolve<IStreetNames>());

            // Act
            var result = await sut.IsValid(
                invalidEtag.ToString(),
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
