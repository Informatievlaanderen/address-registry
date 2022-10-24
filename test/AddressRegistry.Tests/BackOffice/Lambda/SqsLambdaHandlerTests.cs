namespace AddressRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Abstractions.Validation;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Handlers;
    using AddressRegistry.Api.BackOffice.Handlers.Lambda.Requests;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Requests;
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

            var sqsLambdaRequest = new ApproveAddressLambdaRequest(Guid.NewGuid().ToString(), new ApproveAddressSqsRequest
            {
                Request = new ApproveBackOfficeRequest { PersistentLocalId = 1 },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });

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

            var sqsLambdaRequest = new ApproveAddressLambdaRequest(Guid.NewGuid().ToString(), new ApproveAddressSqsRequest
            {
                Request = new ApproveBackOfficeRequest { PersistentLocalId = 1 },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });

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

            var sqsLambdaRequest = new ApproveAddressLambdaRequest(Guid.NewGuid().ToString(), new ApproveAddressSqsRequest
            {
                Request = new ApproveBackOfficeRequest { PersistentLocalId = 1 },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });

            var sut = new FakeLambdaHandler(
                Container.Resolve<IConfiguration>(),
                new FakeRetryPolicy(),
                Mock.Of<IStreetNames>(),
                ticketing.Object,
                MockExceptionIdempotentCommandHandler<AddressIsRemovedException>().Object);

            await sut.Handle(sqsLambdaRequest, CancellationToken.None);

            //Assert
            ticketing.Verify(x =>
                x.Error(sqsLambdaRequest.TicketId, new TicketError("Verwijderd adres.", "VerwijderdAdres"),
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

            var sqsLambdaRequest = new ApproveAddressLambdaRequest(streetNamePersistentLocalId, new ApproveAddressSqsRequest
            {
                Request = new ApproveBackOfficeRequest { PersistentLocalId = addressPersistentLocalId },
                IfMatchHeaderValue = "Outdated",
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });

            // Act
            await sut.Handle(sqsLambdaRequest, CancellationToken.None);

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

            var sqsLambdaRequest = new ApproveAddressLambdaRequest(Guid.NewGuid().ToString(), new ApproveAddressSqsRequest
            {
                Request = new ApproveBackOfficeRequest { PersistentLocalId = 1 },
                TicketId = Guid.NewGuid(),
                Metadata = new Dictionary<string, object?>(),
                ProvenanceData = Fixture.Create<ProvenanceData>()
            });

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

    public sealed class FakeLambdaHandler : SqsLambdaHandler<ApproveAddressLambdaRequest>
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
            ApproveAddressLambdaRequest request,
            CancellationToken cancellationToken)
        {
            IdempotentCommandHandler.Dispatch(
                Guid.NewGuid(),
                new object(),
                new Dictionary<string, object>(),
                cancellationToken);

            return Task.FromResult(new ETagResponse("bla", "etag"));
        }

        protected override TicketError? InnerMapDomainException(DomainException exception, ApproveAddressLambdaRequest request)
        {
            return exception switch
            {
                StreetNameHasInvalidStatusException => new TicketError(
                    ValidationErrors.Common.StreetNameIsNotActive.Message,
                    ValidationErrors.Common.StreetNameIsNotActive.Code),
                AddressHasInvalidStatusException => new TicketError(
                    ValidationErrors.RetireAddress.AddressInvalidStatus.Message,
                    ValidationErrors.RetireAddress.AddressInvalidStatus.Code),
                _ => null
            };
        }
    }
}
