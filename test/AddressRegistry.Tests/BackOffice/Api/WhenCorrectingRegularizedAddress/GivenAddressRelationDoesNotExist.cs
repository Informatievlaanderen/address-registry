namespace AddressRegistry.Tests.BackOffice.Api.WhenCorrectingRegularizedAddress
{
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using FluentAssertions;
    using Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using StreetName;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenAddressRelationDoesNotExist : BackOfficeApiTest
    {
        private readonly AddressController _controller;
        private readonly TestBackOfficeContext _backOfficeContext;

        public GivenAddressRelationDoesNotExist(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task ThenReturnsNotFoundResult()
        {
            //Act
            var result = await _controller.CorrectRegularization(
                _backOfficeContext,
                MockIfMatchValidator(true),
                null,
                new AddressPersistentLocalId(123),
                CancellationToken.None);

            //Assert
            result.Should().BeOfType<NotFoundResult>();
        }
    }
}
