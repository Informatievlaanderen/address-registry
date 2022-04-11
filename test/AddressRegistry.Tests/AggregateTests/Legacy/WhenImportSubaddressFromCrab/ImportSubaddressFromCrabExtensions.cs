namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressFromCrab
{
    using Address;
    using Address.Commands.Crab;
    using Address.Events.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;

    public static class ImportSubaddressFromCrabExtensions
    {
        public static AddressSubaddressWasImportedFromCrab ToLegacyEvent(this ImportSubaddressFromCrab command)
        {
            return new AddressSubaddressWasImportedFromCrab(
                command.SubaddressId,
                command.HouseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressFromCrab WithBoxNumber(this ImportSubaddressFromCrab command, BoxNumber boxNumber)
        {
            return new ImportSubaddressFromCrab(
                command.SubaddressId,
                command.HouseNumberId,
                boxNumber,
                command.BoxNumberType,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportSubaddressFromCrab WithCrabModification(this ImportSubaddressFromCrab command, CrabModification modification)
        {
            return new ImportSubaddressFromCrab(
                command.SubaddressId,
                command.HouseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }

        public static ImportSubaddressFromCrab WithLifetime(this ImportSubaddressFromCrab command, CrabLifetime lifetime)
        {
            return new ImportSubaddressFromCrab(
                command.SubaddressId,
                command.HouseNumberId,
                command.BoxNumber,
                command.BoxNumberType,
                lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }
    }
}
