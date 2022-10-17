namespace AddressRegistry.Tests.BackOffice.Lambda.WhenCorrectingAddressApproval
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using StreetName;
    using StreetName.Exceptions;
    using AutoFixture;
    using Infrastructure;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenStreetNameHasInvalidStatus : BackOfficeLambdaTest
    {
        public GivenStreetNameHasInvalidStatus(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());
        }

        [Fact]
        public async Task WhenStreetNameHasInvalidStatus_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new SqsAddressCorrectApprovalLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler<StreetNameHasInvalidStatusException>().Object);

            // Act
            var request = new SqsLambdaAddressCorrectApprovalRequest
            {
                Request = new AddressBackOfficeCorrectApprovalRequest(),
                MessageGroupId = Fixture.Create<int>().ToString(),
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            };
            await sut.Handle(request, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Deze actie is enkel toegestaan binnen straatnamen met status 'voorgesteld' of 'inGebruik'.",
                        "AdresStraatnaamVoorgesteldOfInGebruik"),
                    CancellationToken.None));
        }
    }
}
