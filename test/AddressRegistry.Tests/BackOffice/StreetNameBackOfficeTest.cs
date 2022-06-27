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
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            StreetNameStatus status = StreetNameStatus.Current)
        {
            var importMunicipality = new ImportMigratedStreetName(
                streetNameId,
                streetNamePersistentLocalId,
                Fixture.Create<MunicipalityId>(),
                new NisCode("23002"),
                status,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(importMunicipality);
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
    }
}
