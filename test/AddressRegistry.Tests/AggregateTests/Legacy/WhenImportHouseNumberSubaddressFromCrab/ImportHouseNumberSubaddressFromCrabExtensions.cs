namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberSubaddressFromCrab
{
    using Address.Commands.Crab;
    using Address.Events.Crab;
    using Address.ValueObjects;
    using Be.Vlaanderen.Basisregisters.Crab;

    public static class ImportHouseNumberSubaddressFromCrabExtensions
    {
        public static AddressHouseNumberWasImportedFromCrab ToLegacyEvent(this ImportHouseNumberSubaddressFromCrab command)
        {
            return new AddressHouseNumberWasImportedFromCrab(
                command.HouseNumberId,
                command.StreetNameId,
                command.HouseNumber,
                command.GrbNotation,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }
        
        public static ImportHouseNumberSubaddressFromCrab WithHouseNumber(this ImportHouseNumberSubaddressFromCrab command, HouseNumber houseNumber)
        {
            return new ImportHouseNumberSubaddressFromCrab(
                command.HouseNumberId,
                command.SubaddressId,
                command.StreetNameId,
                houseNumber,
                command.GrbNotation,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberSubaddressFromCrab WithLifetime(this ImportHouseNumberSubaddressFromCrab command, CrabLifetime lifetime)
        {
            return new ImportHouseNumberSubaddressFromCrab(
                command.HouseNumberId,
                command.SubaddressId,
                command.StreetNameId,
                command.HouseNumber,
                command.GrbNotation,
                lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberSubaddressFromCrab WithCrabModification(this ImportHouseNumberSubaddressFromCrab command, CrabModification modification)
        {
            return new ImportHouseNumberSubaddressFromCrab(
                command.HouseNumberId,
                command.SubaddressId,
                command.StreetNameId,
                command.HouseNumber,
                command.GrbNotation,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }
    }
}
