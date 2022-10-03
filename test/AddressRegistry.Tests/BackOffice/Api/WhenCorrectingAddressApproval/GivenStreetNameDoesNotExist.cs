namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingAddressApproval
{
    using System;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using AddressRegistry.Tests.BackOffice.Infrastructure;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Mvc;
    using Xunit;
    using Xunit.Abstractions;
    using AddressController = AddressRegistry.Api.BackOffice.AddressController;

    public class GivenStreetNameDoesNotExist : BackOfficeApiTest
    {
        private readonly Fixture _fixture;
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenStreetNameDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fixture = new Fixture();
            _controller = CreateApiBusControllerWithUser<AddressController>();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext(Array.Empty<string>());
        }

        [Fact]
        public async Task ThenNotFoundResult()
        {
            //Arrange
            var persistentLocalId = _fixture.Create<int>();

            var request = new AddressCorrectApprovalRequest()
            {
                PersistentLocalId = persistentLocalId
            };

            //Act
            var result = await _controller.CorrectApproval(
                _backOfficeContext,
                new AddressCorrectApprovalRequestValidator(),
                MockIfMatchValidator(true),
                request,
                null);

            //Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
