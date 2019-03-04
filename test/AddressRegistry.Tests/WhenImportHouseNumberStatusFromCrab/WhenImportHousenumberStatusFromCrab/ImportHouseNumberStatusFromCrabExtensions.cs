namespace AddressRegistry.Tests.WhenImportHouseNumberStatusFromCrab
{
    using Address.Commands.Crab;
    using Address.Events.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Crab;

    public static class ImportHouseNumberStatusFromCrabExtensions
    {
        public static AddressHouseNumberStatusWasImportedFromCrab ToLegacyEvent(this ImportHouseNumberStatusFromCrab command)
        {
            return new AddressHouseNumberStatusWasImportedFromCrab(
                command.HouseNumberStatusId,
                command.HouseNumberId,
                command.AddressStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberStatusFromCrab WithCrabModification(this ImportHouseNumberStatusFromCrab command, CrabModification modification)
        {
            return new ImportHouseNumberStatusFromCrab(
                command.HouseNumberStatusId,
                command.HouseNumberId,
                command.AddressStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }

        public static ImportHouseNumberStatusFromCrab WithStatus(this ImportHouseNumberStatusFromCrab command, CrabAddressStatus status)
        {
            return new ImportHouseNumberStatusFromCrab(
                command.HouseNumberStatusId,
                command.HouseNumberId,
                status,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberStatusFromCrab WithLifetime(this ImportHouseNumberStatusFromCrab command, CrabLifetime lifetime)
        {
            return new ImportHouseNumberStatusFromCrab(
                command.HouseNumberStatusId,
                command.HouseNumberId,
                command.AddressStatus,
                lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberStatusFromCrab WithStatusId(this ImportHouseNumberStatusFromCrab command, CrabHouseNumberStatusId statusId)
        {
            return new ImportHouseNumberStatusFromCrab(
                statusId,
                command.HouseNumberId,
                command.AddressStatus,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }
    }
}
