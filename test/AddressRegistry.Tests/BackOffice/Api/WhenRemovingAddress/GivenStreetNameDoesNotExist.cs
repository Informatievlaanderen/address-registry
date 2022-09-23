namespace AddressRegistry.Tests.BackOffice.Api.WhenRemovingAddress
{
    using System;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using Infrastructure;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Mvc;
    using Xunit;
    using Xunit.Abstractions;

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
            var result = await _controller.Remove(
                _backOfficeContext,
                new AddressRemoveRequestValidator(),
                MockIfMatchValidator(true),
                null,
                ResponseOptions,
                _fixture.Create<AddressRemoveRequest>());

            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
