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
    using Autofac;
    using BackOffice.Infrastructure;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Infrastructure;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using StreetName;
    using StreetName.Exceptions;
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
            var addressPersistentLocalId = new AddressPersistentLocalId(1);
            var puri = $"https://data.vlaanderen.be/id/adres/{addressPersistentLocalId}";

            await _fakeBackOfficeContext.AddIdempotentAddressStreetNameIdRelation(
                addressPersistentLocalId, Fixture.Create<StreetNamePersistentLocalId>(), CancellationToken.None);

            var sut = new ReaddressLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                ticketing.Object,
                Mock.Of<IStreetNames>(),
                MockExceptionIdempotentCommandHandler(() => new SourceAndDestinationAddressAreTheSameException(
                    addressPersistentLocalId,
                    new HouseNumber("100"))).Object,
                _fakeBackOfficeContext,
                Container);

            // Act
            await sut.Handle(new ReaddressLambdaRequest(Fixture.Create<int>().ToString(), new ReaddressSqsRequest
            {
                Request = new ReaddressRequest
                {
                    DoelStraatnaamId = string.Empty,
                    HerAdresseer = new List<AddressToReaddressItem>()
                    {
                        new AddressToReaddressItem
                        {
                            BronAdresId = puri,
                            DoelHuisnummer = "100"
                        }
                    },
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
                        $"Het bronAdresId is hetzelfde als het doelHuisnummer: {puri}.",
                        "BronAdresIdHetzelfdeAlsDoelHuisnummer"),
                    CancellationToken.None));
        }
    }
}
