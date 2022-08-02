namespace AddressRegistry.Tests.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Infrastructure;
    using AddressRegistry.Api.BackOffice.Infrastructure.Options;
    using StreetName;
    using Tests;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using Moq;
    using StreetName.Commands;
    using Xunit.Abstractions;

    public class AddressRegistryBackOfficeTest : AddressRegistryTest
    {
        internal const string DetailUrl = "https://www.registry.com/address/voorgesteld/{0}";
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
