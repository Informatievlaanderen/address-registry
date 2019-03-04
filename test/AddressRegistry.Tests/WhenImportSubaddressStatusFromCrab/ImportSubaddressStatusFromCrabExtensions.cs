namespace AddressRegistry.Tests.WhenImportSubaddressStatusFromCrab
{
    using Address.Commands.Crab;
    using Address.Events.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Crab;

    public static class ImportSubaddressStatusFromCrabExtensions
    {
        public static AddressSubaddressStatusWasImportedFromCrab ToLegacyEvent(this ImportSubaddressStatusFromCrab command) => new AddressSubaddressStatusWasImportedFromCrab(
            command.SubaddressStatusId,
            command.SubaddressId,
            command.AddressStatus,
            command.Lifetime,
            command.Timestamp,
            command.Operator,
            command.Modification,
            command.Organisation);

        public static ImportSubaddressStatusFromCrab WithCrabModification(this ImportSubaddressStatusFromCrab command,
            CrabModification modification) => new ImportSubaddressStatusFromCrab(
            command.SubaddressStatusId,
            command.SubaddressId,
            command.AddressStatus,
            command.Lifetime,
            command.Timestamp,
            command.Operator,
            modification,
            command.Organisation);

        public static ImportSubaddressStatusFromCrab WithStatus(this ImportSubaddressStatusFromCrab command,
            CrabAddressStatus status) => new ImportSubaddressStatusFromCrab(
            command.SubaddressStatusId,
            command.SubaddressId,
            status,
            command.Lifetime,
            command.Timestamp,
            command.Operator,
            command.Modification,
            command.Organisation);

        public static ImportSubaddressStatusFromCrab WithLifetime(this ImportSubaddressStatusFromCrab command,
            CrabLifetime lifetime) => new ImportSubaddressStatusFromCrab(
            command.SubaddressStatusId,
            command.SubaddressId,
            command.AddressStatus,
            lifetime,
            command.Timestamp,
            command.Operator,
            command.Modification,
            command.Organisation);

        public static ImportSubaddressStatusFromCrab WithStatusId(this ImportSubaddressStatusFromCrab command,
            CrabSubaddressStatusId statusId) => new ImportSubaddressStatusFromCrab(
            statusId,
            command.SubaddressId,
            command.AddressStatus,
            command.Lifetime,
            command.Timestamp,
            command.Operator,
            command.Modification,
            command.Organisation);
    }
}
