namespace AddressRegistry.Tests.BackOffice.Api.WhenApprovingAddress
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Address;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Address;
    using AddressRegistry.Api.BackOffice.Address.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using StreetName;
    using StreetName.Commands;
    using Xunit;
    using Xunit.Abstractions;
    using AddressGeometry = Address.AddressGeometry;
    using AddressId = Address.AddressId;
    using AddressStatus = Address.AddressStatus;
    using PostalCode = Address.PostalCode;
    using StreetNameId = Address.StreetNameId;

    public class GivenAddressIsRemoved : AddressRegistryBackOfficeTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;

        public GivenAddressIsRemoved(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>("John Doe");
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

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
                AddressStatus.Current,
                new AddressRegistry.Address.HouseNumber(houseNumber),
                null,
                Fixture.Create<AddressGeometry>(),
                true,
                new PostalCode(postInfoId),
                true,
                true,
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
                .ThrowAsync<ApiException>()
                .Result
                .Where(x =>
                     x.StatusCode == StatusCodes.Status410Gone
                     && x.Message == "Verwijderde adres.");
        }
    }
}
