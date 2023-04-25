namespace AddressRegistry.Tests.BackOffice.Lambda.WhenReaddress
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.SqsRequests;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Exceptions;
    using AddressRegistry.Tests.BackOffice.Infrastructure;
    using AddressRegistry.Tests.BackOffice.Lambda.Infrastructure;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;

    public class GivenSourceAndDestinationAddressAreTheSame : BackOfficeLambdaTest
    {
        private readonly BackOfficeContext _fakeBackOfficeContext;

        public GivenSourceAndDestinationAddressAreTheSame(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _fakeBackOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
        }

        [Fact]
        public async Task ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var sut = new ReaddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler(() => new SourceAndDestinationAddressAreTheSameException(
                    new AddressPersistentLocalId(1),
                    new HouseNumber("100"))).Object,
                _fakeBackOfficeContext,
                Container);

            // Act
            await sut.Handle(new ReaddressLambdaRequest(Fixture.Create<int>().ToString(), new ReaddressSqsRequest
            {
                Request = new ReaddressRequest
                {
                    DoelStraatnaamId = string.Empty,
                    HerAdresseer = new List<AddressToReaddressItem>(),
                    OpheffenAdressen = new List<string>()
                },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            }), CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError(
                        "Het bronAdresId is hetzelfde als het doelHuisnummer: http://base/1.",
                        "BronAdresIdHetzelfdeAlsDoelHuisnummer"),
                    CancellationToken.None));
        }
    }
}
