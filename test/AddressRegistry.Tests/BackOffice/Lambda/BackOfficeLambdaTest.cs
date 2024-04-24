namespace AddressRegistry.Tests.BackOffice.Lambda
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using StreetName;
    using StreetName.Commands;
    using Tests;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.CommandHandling.Idempotency;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Sqs.Responses;
    using global::AutoFixture;
    using Moq;
    using Newtonsoft.Json;
    using TicketingService.Abstractions;
    using Xunit.Abstractions;

    public class BackOfficeLambdaTest : AddressRegistryTest
    {
        protected const string StraatNaamPuri = "https://data.vlaanderen.be/id/straatnaam/";
        protected const string AdresPuri = "https://data.vlaanderen.be/id/adres/";
        protected const string PostInfoPuri = "https://data.vlaanderen.be/id/postinfo/";

        protected BackOfficeLambdaTest(
            ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        { }

        public static string StreetNamePuriFor(int id) => StraatNaamPuri + id;
        public static string AddressPuriFor(int id) => AdresPuri + id;

        protected Mock<ITicketing> MockTicketing(Action<ETagResponse> ticketingCompleteCallback)
        {
            var ticketing = new Mock<ITicketing>();

            ticketing
                .Setup(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None))
                .Callback<Guid, TicketResult, CancellationToken>((_, ticketResult, _) =>
                {
                    var eTagResponse = JsonConvert.DeserializeObject<ETagResponse>(ticketResult.ResultAsJson!)!;
                    ticketingCompleteCallback(eTagResponse);
                });

            return ticketing;
        }

        protected Mock<ITicketing> MockTicketing(Action<List<ETagResponse>> ticketingCompleteCallback)
        {
            var ticketing = new Mock<ITicketing>();

            ticketing
                .Setup(x => x.Complete(It.IsAny<Guid>(), It.IsAny<TicketResult>(), CancellationToken.None))
                .Callback<Guid, TicketResult, CancellationToken>((_, ticketResult, _) =>
                {
                    var eTagResponse = JsonConvert.DeserializeObject<List<ETagResponse>>(ticketResult.ResultAsJson!)!;
                    ticketingCompleteCallback(eTagResponse);
                });

            return ticketing;
        }

        protected Mock<IIdempotentCommandHandler> MockExceptionIdempotentCommandHandler<TException>()
            where TException : Exception, new()
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            idempotentCommandHandler
                .Setup(x => x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(),
                    It.IsAny<IDictionary<string, object>>(), CancellationToken.None))
                .Throws<TException>();

            return idempotentCommandHandler;
        }

        protected Mock<IIdempotentCommandHandler> MockExceptionIdempotentCommandHandler<TException>(Func<TException> exceptionFactory)
            where TException : Exception
        {
            var idempotentCommandHandler = new Mock<IIdempotentCommandHandler>();

            idempotentCommandHandler
                .Setup(x => x.Dispatch(It.IsAny<Guid>(), It.IsAny<object>(),
                    It.IsAny<IDictionary<string, object>>(), CancellationToken.None))
                .Throws(exceptionFactory());

            return idempotentCommandHandler;
        }

        protected void DispatchArrangeCommand<T>(T command) where T : IHasCommandProvenance
        {
            using var scope = Container.BeginLifetimeScope();
            var bus = scope.Resolve<ICommandHandlerResolver>();
            bus.Dispatch(command.CreateCommandId(), command).GetAwaiter().GetResult();
        }

        protected void ImportMigratedStreetName(
            StreetNameId streetNameId,
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            NisCode nisCode,
            StreetNameStatus status = StreetNameStatus.Current)
        {
            var importMunicipality = new ImportMigratedStreetName(
                streetNameId,
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                nisCode,
                status,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(importMunicipality);
        }

        protected ProposeAddress ProposeAddress(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            PostalCode postalCode,
            MunicipalityId municipalityId,
            HouseNumber houseNumber,
            BoxNumber? boxNumber)
        {
            var proposeCommand = new ProposeAddress(
                streetNamePersistentLocalId,
                postalCode,
                municipalityId,
                addressPersistentLocalId,
                houseNumber,
                boxNumber,
                GeometryMethod.AppointedByAdministrator,
                GeometrySpecification.Entry,
                GeometryHelpers.GmlPointGeometry.ToExtendedWkbGeometry(),
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(proposeCommand);
            return proposeCommand;
        }

        protected void ApproveAddress(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var approveAddress = new ApproveAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(approveAddress);
        }

        protected void RetireAddress(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var approveAddress = new RetireAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(approveAddress);
        }

        protected void RemoveAddress(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var approveAddress = new RemoveAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(approveAddress);
        }
    }
}
