namespace AddressRegistry.Tests.WhenImportSubaddressPositionFromCrab
{
    using Address.Commands.Crab;
    using Address.Events.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Crab;

    public static class ImportSubaddressPositionFromCrabExtensions
    {
        public static AddressSubaddressPositionWasImportedFromCrab ToLegacyEvent(this ImportSubaddressPositionFromCrab command)
        {
            return new AddressSubaddressPositionWasImportedFromCrab(
                command.AddressPositionId,
                command.SubaddressId,
                command.AddressPosition,
                command.AddressPositionOrigin,
                command.AddressNature,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressPositionFromCrab WithCrabAddressPositionOrigin(this ImportSubaddressPositionFromCrab command, CrabAddressPositionOrigin addressPositionOrigin)
        {
            return new ImportSubaddressPositionFromCrab(
                command.AddressPositionId,
                command.SubaddressId,
                command.AddressPosition,
                command.AddressNature,
                addressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressPositionFromCrab WithCrabModification(this ImportSubaddressPositionFromCrab command, CrabModification modification)
        {
            return new ImportSubaddressPositionFromCrab(
                command.AddressPositionId,
                command.SubaddressId,
                command.AddressPosition,
                command.AddressNature,
                command.AddressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }

        public static ImportSubaddressPositionFromCrab WithLifetime(this ImportSubaddressPositionFromCrab command, CrabLifetime lifetime)
        {
            return new ImportSubaddressPositionFromCrab(
                command.AddressPositionId,
                command.SubaddressId,
                command.AddressPosition,
                command.AddressNature,
                command.AddressPositionOrigin,
                lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressPositionFromCrab WithWkbGeometry(this ImportSubaddressPositionFromCrab command, WkbGeometry wkbGeometry)
        {
            return new ImportSubaddressPositionFromCrab(
                command.AddressPositionId,
                command.SubaddressId,
                wkbGeometry,
                command.AddressNature,
                command.AddressPositionOrigin,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressPositionFromCrab WithPositionId(this ImportSubaddressPositionFromCrab command, CrabAddressPositionId addressPositionId)
        {
            return new ImportSubaddressPositionFromCrab(
                addressPositionId,
                command.SubaddressId,
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
