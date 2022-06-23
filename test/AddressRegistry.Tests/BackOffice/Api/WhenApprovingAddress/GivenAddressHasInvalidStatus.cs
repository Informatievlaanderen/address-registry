namespace AddressRegistry.Tests.BackOffice.Api.WhenApprovingAddress
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Address;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Address;
    using AddressRegistry.Api.BackOffice.Address.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using FluentAssertions;
    using FluentValidation;
    using global::AutoFixture;
    using Infrastructure;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;
    using AddressGeometry = Address.AddressGeometry;
    using AddressId = Address.AddressId;
    using AddressStatus = Address.AddressStatus;
    using PostalCode = Address.PostalCode;
    using StreetNameId = Address.StreetNameId;

    public class GivenAddressHasInvalidStatus : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;

        public GivenAddressHasInvalidStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>("John Doe");
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        //TODO: add rejected
        [Fact]
        public void ThenThrowApiException()
        {
            string postInfoId = "8200";
            string houseNumber = "11";
            string streetNameIdStr = Guid.NewGuid().ToString("D");
            int addressPersistentIdInt = 123;

            var legacyStreetNameId = StreetNameId.CreateFor(streetNameIdStr);
            var streetNameId = AddressRegistry.StreetName.StreetNameId.CreateFor(streetNameIdStr);
            var streetNamePersistentId = Fixture.Create<StreetNamePersistentLocalId>();
            var legacyAddressPersistentLocalId = new PersistentLocalId(addressPersistentIdInt);
            var addressPersistentLocalId = new AddressPersistentLocalId(addressPersistentIdInt);

            ImportMigratedStreetName(streetNameId, streetNamePersistentId);
            MigrateAddresToStreetName(
                Fixture.Create<AddressId>(),
                streetNamePersistentId,
                legacyStreetNameId,
                legacyAddressPersistentLocalId,
                AddressStatus.Retired,
                new AddressRegistry.Address.HouseNumber(houseNumber),
                null,
                Fixture.Create<AddressGeometry>(),
                true,
                new PostalCode(postInfoId),
                true,
                false,
                null
            );

            _backOfficeContext.AddressPersistentIdStreetNamePersistentIds.Add(
                new AddressPersistentIdStreetNamePersistentId(addressPersistentLocalId, streetNamePersistentId));
            _backOfficeContext.SaveChanges();

            var approveRequest = new AddressApproveRequest
            {
                PersistentLocalId = addressPersistentLocalId
            };

            //Act
            Func<Task> act = async () => await _controller.Approve(
                _idempotencyContext,
                _backOfficeContext,
                new AddressApproveRequestValidator(),
                Container.Resolve<IStreetNames>(),
                approveRequest,
                null, CancellationToken.None);

            // Assert
            act
                .Should()
                .ThrowAsync<ValidationException>()
                .Result
                .Where(x =>
                     x.Errors.Any(e => e.ErrorCode == "AdresGehistoreerdOfAfgekeurd"
                     && e.ErrorMessage == "Deze actie is enkel toegestaan op adressen met status 'voorgesteld'."));
        }
    }
}
