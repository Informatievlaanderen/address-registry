namespace AddressRegistry.Tests.BackOffice.Handlers
{
    using AddressRegistry.Api.BackOffice.Abstractions;
    using StreetName;
    using StreetName.Commands;
    using Tests;
    using Autofac;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using global::AutoFixture;
    using Xunit.Abstractions;

    public class BackOfficeHandlerTest : AddressRegistryTest
    {
        protected BackOfficeHandlerTest(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper)
        { }

        protected void DispatchArrangeCommand<T>(T command) where T : IHasCommandProvenance
        {
            using var scope = Container.BeginLifetimeScope();
            var bus = scope.Resolve<ICommandHandlerResolver>();
            bus.Dispatch(command.CreateCommandId(), command);
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

        protected void RetireAddress(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId)
        {
            var retireAddress = new RetireAddress(
                streetNamePersistentLocalId,
                addressPersistentLocalId,
                Fixture.Create<Provenance>());
            DispatchArrangeCommand(retireAddress);
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
    }
}
