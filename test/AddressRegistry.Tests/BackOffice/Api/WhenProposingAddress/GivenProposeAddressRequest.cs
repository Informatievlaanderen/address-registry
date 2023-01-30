namespace AddressRegistry.Tests.BackOffice.Api.WhenProposingAddress
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests;
    using Be.Vlaanderen.Basisregisters.Sqs.Requests;
    using FluentAssertions;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Mvc;
    using Moq;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenProposeAddressRequest  : BackOfficeApiTest
    {
        private readonly AddressController _controller;

        public GivenProposeAddressRequest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _controller = CreateApiBusControllerWithUser<AddressController>();
        }

        [Fact]
        public async Task ThenTicketLocationIsReturned()
        {
            var ticketId = Fixture.Create<Guid>();
            var expectedLocationResult = new LocationResult(CreateTicketUri(ticketId));

            MockMediator
                .Setup(x => x.Send(It.IsAny<ProposeAddressSqsRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedLocationResult));

            var result = (AcceptedResult)await _controller.Propose(
                MockValidRequestValidator<ProposeAddressRequest>(),
                Fixture.Create<ProposeAddressRequest>());

            result.Should().NotBeNull();
            AssertLocation(result.Location, ticketId);
        }
    }
}
