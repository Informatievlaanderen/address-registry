namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberPositionFromCrab
{
    using Address.Commands.Crab;
    using Address.Events.Crab;
    using Address.ValueObjects;
    using Address.ValueObjects.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;

    public static class ImportHouseNumberPositionFromCrabExtensions
    {
        public static AddressHouseNumberPositionWasImportedFromCrab ToLegacyEvent(this ImportHouseNumberPositionFromCrab command)
        {
            return new AddressHouseNumberPositionWasImportedFromCrab(
                command.AddressPositionId,
                command.HouseNumberId,
                command.AddressPosition,
                command.AddressPositionOrigin,
                command.AddressNature,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberPositionFromCrab WithCrabAddressPositionOrigin(this ImportHouseNumberPositionFromCrab command, CrabAddressPositionOrigin addressPositionOrigin)
        {
            return new ImportHouseNumberPositionFromCrab(
                command.AddressPositionId,
                command.HouseNumberId,
                command.AddressPosition,
                command.AddressNature,
                addressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberPositionFromCrab WithCrabModification(this ImportHouseNumberPositionFromCrab command, CrabModification modification)
        {
            return new ImportHouseNumberPositionFromCrab(
                command.AddressPositionId,
                command.HouseNumberId,
                command.AddressPosition,
                command.AddressNature,
                command.AddressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }

        public static ImportHouseNumberPositionFromCrab WithLifetime(this ImportHouseNumberPositionFromCrab command, CrabLifetime lifetime)
        {
            return new ImportHouseNumberPositionFromCrab(
                command.AddressPositionId,
                command.HouseNumberId,
                command.AddressPosition,
                command.AddressNature,
                command.AddressPositionOrigin,
                lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberPositionFromCrab WithWkbGeometry(this ImportHouseNumberPositionFromCrab command, WkbGeometry wkbGeometry)
        {
            return new ImportHouseNumberPositionFromCrab(
                command.AddressPositionId,
                command.HouseNumberId,
                wkbGeometry,
                command.AddressNature,
                command.AddressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberPositionFromCrab WithPositionId(this ImportHouseNumberPositionFromCrab command, CrabAddressPositionId addressPositionId)
        {
            return new ImportHouseNumberPositionFromCrab(
                addressPositionId,
                command.HouseNumberId,
                command.AddressPosition,
                command.AddressNature,
                command.AddressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }
    }
}
