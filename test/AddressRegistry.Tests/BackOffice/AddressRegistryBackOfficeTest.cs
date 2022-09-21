namespace AddressRegistry.Tests.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Abstractions.Responses;
    using AddressRegistry.Api.BackOffice.Handlers.Sqs.Lambda.Handlers;
    using AddressRegistry.Api.BackOffice.Infrastructure;
    using AddressRegistry.Api.BackOffice.Infrastructure.Options;
    using StreetName;
    using Tests;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using FluentValidation;
    using FluentValidation.Results;
    using global::AutoFixture;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Moq;
    using Newtonsoft.Json;
    using StreetName.Commands;
    using TicketingService.Abstractions;
    using Xunit.Abstractions;

    public class AddressRegistryBackOfficeTest : AddressRegistryTest
    {
        internal const string DetailUrl = "https://www.registry.com/address/voorgesteld/{0}";

        protected const string StraatNaamPuri = $"https://data.vlaanderen.be/id/straatnaam/";
        protected const string PostInfoPuri = $"https://data.vlaanderen.be/id/postinfo/";

        protected IOptions<ResponseOptions> ResponseOptions { get; }

        protected Mock<IMediator> MockMediator { get; }

        public AddressRegistryBackOfficeTest(
            ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        {
            ResponseOptions = Options.Create(Fixture.Create<ResponseOptions>());
            ResponseOptions.Value.DetailUrl = DetailUrl;
            MockMediator = new Mock<IMediator>();
        }

        public void DispatchArrangeCommand<T>(T command) where T : IHasCommandProvenance
        {
            using var scope = Container.BeginLifetimeScope();
            var bus = scope.Resolve<ICommandHandlerResolver>();
            bus.Dispatch(command.CreateCommandId(), command);
        }

        protected IIfMatchHeaderValidator MockIfMatchValidator(bool expectedResult)
        {
            var mockIfMatchHeaderValidator = new Mock<IIfMatchHeaderValidator>();
            mockIfMatchHeaderValidator
                .Setup(x =>
                    x.IsValid(
                        It.IsAny<string>(), It.IsAny<StreetNamePersistentLocalId>(), It.IsAny<AddressPersistentLocalId>(), CancellationToken.None))
                .Returns(Task.FromResult(expectedResult));
            return mockIfMatchHeaderValidator.Object;
        }

        protected IValidator<TRequest> MockValidRequestValidator<TRequest>()
        {
            var mockRequestValidator = new Mock<IValidator<TRequest>>();
            mockRequestValidator.Setup(x => x.ValidateAsync(It.IsAny<TRequest>(), CancellationToken.None))
                .Returns(Task.FromResult(new ValidationResult()));
            return mockRequestValidator.Object;
        }

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

        public T CreateApiBusControllerWithUser<T>() where T : ApiController
        {
            var controller = Activator.CreateInstance(typeof(T), MockMediator.Object) as T;

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, "userId"),
                new Claim("name", "John Doe"),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);

            if (controller == null)
            {
                throw new Exception("Could not find controller type");
            }

            controller.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            return controller;
        }

        protected void ImportMigratedStreetName(
            StreetNameId streetNameId,
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            NisCode niscode,
            StreetNameStatus status = StreetNameStatus.Current)
        {
            var importMunicipality = new ImportMigratedStreetName(
                streetNameId,
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                niscode,
                status,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(importMunicipality);
        }

        protected void ProposeAddress(
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

        protected void RemoveStreetName(StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            var command = new RemoveStreetName(
                streetNamePersistentLocalId,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(command);
        }

        protected void MigrateAddresToStreetName(
            Address.AddressId addressId,
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            Address.StreetNameId streetNameId,
            Address.PersistentLocalId addressPersistentLocalId,
            Address.AddressStatus status,
            Address.HouseNumber houseNumber,
            Address.BoxNumber? boxNumber,
            Address.AddressGeometry geometry,
            bool? officiallyAssigned,
            Address.PostalCode postalCode,
            bool isComplete,
            bool isRemoved,
            Address.AddressId? parentAddressId)
        {
            DispatchArrangeCommand(new MigrateAddressToStreetName(
                addressId,
                streetNamePersistentLocalId,
                streetNameId,
                addressPersistentLocalId,
                status,
                houseNumber,
                boxNumber,
                geometry,
                officiallyAssigned,
                postalCode,
                isComplete,
                isRemoved,
                parentAddressId,
                Fixture.Create<Provenance>()));
        }
    }
}
