namespace AddressRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using Autofac;
    using AutoFixture;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Handlers;
    using Be.Vlaanderen.Basisregisters.Sqs.Lambda.Infrastructure;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using Microsoft.Extensions.Configuration;
    using Moq;
    using StreetName;
    using StreetName.Exceptions;
    using TicketingService.Abstractions;
    using Xunit;
    using Xunit.Abstractions;
    using global::AutoFixture;
    using Infrastructure;

    public class SqsLambdaHandlerTests : BackOfficeLambdaTest
    {
        public SqsLambdaHandlerTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            Fixture.Customize(new WithFixedMunicipalityId());
        }

        [Fact]
        public async Task TicketShouldBeUpdatedToPendingAndCompleted()
        {
            var ticketing = new Mock<ITicketing>();

            var sqsLambdaRequest = new SqsLambdaAddressApproveRequest
            {
                Request = new AddressBackOfficeApproveRequest{ PersistentLocalId = 1 },
                MessageGroupId = Guid.NewGuid().ToString(),
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            };

            var sut = new FakeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IStreetNames>(),
                ticketing.Object,
                Mock.Of<IIdempotentCommandHandler>());

            await sut.Handle(sqsLambdaRequest, CancellationToken.None);

            ticketing.Verify(x => x.Pending(sqsLambdaRequest.TicketId, CancellationToken.None), Times.Once);
            ticketing.Verify(
                x => x.Complete(sqsLambdaRequest.TicketId,
                    new TicketResult(new ETagResponse("bla", "etag")), CancellationToken.None), Times.Once);
        }

        [Fact]
        public async Task WhenAddressIsNotFoundException_ThenTicketingErrorIsExpected()
        {
            var ticketing = new Mock<ITicketing>();

            var sqsLambdaRequest = new SqsLambdaAddressApproveRequest
            {
                Request = new AddressBackOfficeApproveRequest(),
                MessageGroupId = Guid.NewGuid().ToString(),
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            };

            var sut = new FakeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IStreetNames>(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<AddressIsNotFoundException>().Object);

            await sut.Handle(sqsLambdaRequest, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(sqsLambdaRequest.TicketId, new TicketError("Onbestaand adres.", "AdresIsOnbestaand"),
                    CancellationToken.None));
            ticketing.Verify(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task WhenAddressIsRemovedException_ThenTicketingErrorIsExpected()
        {
            var ticketing = new Mock<ITicketing>();

            var sqsLambdaRequest = new SqsLambdaAddressApproveRequest
            {
                Request = new AddressBackOfficeApproveRequest(),
                MessageGroupId = Guid.NewGuid().ToString(),
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            };

            var sut = new FakeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IStreetNames>(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<AddressIsRemovedException>().Object);

            await sut.Handle(sqsLambdaRequest, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(sqsLambdaRequest.TicketId, new TicketError("Verwijderde adres.", "AdresIsVerwijderd"),
                    CancellationToken.None));
            ticketing.Verify(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None),
                Times.Never);
        }

        [Fact]
        public async Task WhenIfMatchHeaderValueIsMismatch_ThenTicketingErrorIsExpected()
        {
            // Arrange
            var ticketing = new Mock<ITicketing>();

            var streetNamePersistentLocalId = new StreetNamePersistentLocalId(123);
            var addressPersistentLocalId = new AddressPersistentLocalId(456);
            var nisCode = new NisCode("12345");
            var postalCode = new PostalCode("2018");
            var houseNumber = new HouseNumber("11");

            ImportMigratedStreetName(
                new StreetNameId(Guid.NewGuid()),
                streetNamePersistentLocalId,
                nisCode);

            ProposeAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                postalCode,
                Fixture.Create<MunicipalityId>(),
                houseNumber,
                null);

            var sut = new FakeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Container.Resolve<IStreetNames>(),
                ticketing.Object,
                Mock.Of<IIdempotentCommandHandler>());

            // Act
            await sut.Handle(new SqsLambdaAddressApproveRequest
            {
                IfMatchHeaderValue = "Outdated",
                Request = new AddressBackOfficeApproveRequest
                {
                    PersistentLocalId = addressPersistentLocalId
                },
                MessageGroupId = streetNamePersistentLocalId,
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            }, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(
                    It.IsAny<Guid>(),
                    new TicketError("Als de If-Match header niet overeenkomt met de laatste ETag.", "PreconditionFailed"),
                    CancellationToken.None));
        }

        [Fact]
        public async Task WhenNoIfMatchHeaderValueIsPresent_ThenInnerHandleIsCalled()
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            var sqsLambdaRequest = new SqsLambdaAddressApproveRequest
            {
                IfMatchHeaderValue = string.Empty,
                Request = new AddressBackOfficeApproveRequest(),
                MessageGroupId = Guid.NewGuid().ToString(),
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object>(),
                Provenance = Fixture.Create<Provenance>()
            };

            var sut = new FakeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IStreetNames>(),
                Mock.Of<ITicketing>(),
                idempotentCommandHandler.Object);

            await sut.Handle(sqsLambdaRequest, CancellationToken.None);

            //Assert
            idempotentCommandHandler
                .Verify(x =>
                    x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(), It.IsAny<IDictionary<string, object>>(), new CancellationToken()),
                    Times.Once);
        }
    }

    public class FakeLambdaHandler : SqsLambdaHandler<SqsLambdaAddressApproveRequest>
    {
        public FakeLambdaHandler(
            IConfiguration configuration,
            ICustomRetryPolicy retryPolicy,
            IStreetNames streetNames,
            ITicketing ticketing,
            IIdempotentCommandHandler idempotentCommandHandler)
            : base(
                configuration,
                retryPolicy,
                streetNames,
                ticketing,
                idempotentCommandHandler)
        { }

        protected override Task<ETagResponse> InnerHandle(
            SqsLambdaAddressApproveRequest request,
            CancellationToken cancellationToken)
        {
            IdempotentCommandHandler.Dispatch(
                Guid.NewGuid(),
                new object(),
                new Dictionary<string, object>(),
                cancellationToken);

            return Task.FromResult(new ETagResponse("bla", "etag"));
        }
    }
}
