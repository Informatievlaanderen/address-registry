namespace AddressRegistry.Tests.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Security.Claims;
    using AddressRegistry.Api.BackOffice.Address;
    using AddressRegistry.Api.BackOffice.Infrastructure.Options;
    using AddressRegistry.StreetName;
    using AddressRegistry.Tests;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Options;
    using StreetName.Commands;
    using Xunit.Abstractions;

    public class AddressRegistryBackOfficeTest : AddressRegistryTest
    {
        internal const string DetailUrl = "https://www.registry.com/address/voorgesteld/{0}";
        protected IOptions<ResponseOptions> ResponseOptions { get; }

        public AddressRegistryBackOfficeTest(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            ResponseOptions = Options.Create<ResponseOptions>(Fixture.Create<ResponseOptions>());
            ResponseOptions.Value.DetailUrl = DetailUrl;
        }

        public void DispatchArrangeCommand<T>(T command) where T : IHasCommandProvenance
        {
            using var scope = Container.BeginLifetimeScope();
            var bus = scope.Resolve<ICommandHandlerResolver>();
            bus.Dispatch(command.CreateCommandId(), command);
        }

        public T CreateApiBusControllerWithUser<T>(string username) where T : ApiBusController
        {
            var bus = Container.Resolve<ICommandHandlerResolver>();
            var controller = Activator.CreateInstance(typeof(T), bus) as T;

            var claims = new List<Claim>()
            {
                new Claim(ClaimTypes.Name, "username"),
                new Claim(ClaimTypes.NameIdentifier, "userId"),
                new Claim("name", username),
            };
            var identity = new ClaimsIdentity(claims, "TestAuthType");
            var claimsPrincipal = new ClaimsPrincipal(identity);
            if (controller != null)
            {
                controller.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

                return controller;
            }
            else
            {
                throw new Exception("Could not find controller type");
            }
        }

        protected void ImportMigratedStreetName(
            StreetNameId streetNameId,
            StreetNamePersistentLocalId streetNamePersistentLocalId)
        {
            var importMunicipality = new ImportMigratedStreetName(
                streetNameId,
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                new NisCode("23002"),
                StreetNameStatus.Current,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(importMunicipality);
        }

        protected void ProposeAddress(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            PostalCode postalCode,
            AddressPersistentLocalId addressPersistentLocalId,
            HouseNumber houseNumber,
            BoxNumber? boxNumber)
        {
            var proposeCommand = new ProposeAddress(
                streetNamePersistentLocalId,
                postalCode,
                addressPersistentLocalId,
                houseNumber,
                boxNumber,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(proposeCommand);
        }
    }
}
