namespace AddressRegistry.Tests.AggregateTests.WhenApprovingAddress
{
    using System;
    using System.Threading.Tasks;
    using Api.BackOffice.Abstractions.Requests;
    using Api.BackOffice.Validators;
    using Autofac;
    using BackOffice;
    using BackOffice.Infrastructure;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Mvc;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;
    using AddressController = Api.BackOffice.AddressController;

    public class GivenStreetNameDoesNotExist : AddressRegistryBackOfficeTest
    {
        private readonly Fixture _fixture;
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;
        private readonly IdempotencyContext _idempotencyContext;

        public GivenStreetNameDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _controller = CreateApiBusControllerWithUser<AddressController>();
            _idempotencyContext = new FakeIdempotencyContextFactory().CreateDbContext(Array.Empty<string>());
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenNotFoundResult()
        {
            //Arrange
            var persistentLocalId = _fixture.Create<int>();

            var request = new AddressApproveRequest
            {
                PersistentLocalId = persistentLocalId
            };

            //Act
            var result = await _controller.Approve(
                _backOfficeContext,
                new AddressApproveRequestValidator(),
                Container.Resolve<IStreetNames>(),
                request,
                null);

            //Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
