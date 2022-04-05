namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberFromCrab
{
    using Address.Commands.Crab;
    using Address.Events.Crab;
    using Address.ValueObjects;
    using Be.Vlaanderen.Basisregisters.Crab;
    using global::AutoFixture;
    using NodaTime;

    public static class ImportHousenumbeFromCrabExtensions
    {
        public static AddressHouseNumberWasImportedFromCrab ToLegacyEvent(this ImportHouseNumberFromCrab command)
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

        public static ImportHouseNumberFromCrab WithStreetNameId(this ImportHouseNumberFromCrab command, CrabStreetNameId streetNameId)
        {
            return new ImportHouseNumberFromCrab(
                command.HouseNumberId,
                streetNameId,
                command.HouseNumber,
                command.GrbNotation,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberFromCrab WithCrabModification(this ImportHouseNumberFromCrab command, CrabModification modification)
        {
            return new ImportHouseNumberFromCrab(
                command.HouseNumberId,
                command.StreetNameId,
                command.HouseNumber,
                command.GrbNotation,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }

        public static ImportHouseNumberFromCrab WithHouseNumber(this ImportHouseNumberFromCrab command, HouseNumber houseNumber)
        {
            return new ImportHouseNumberFromCrab(
                command.HouseNumberId,
                command.StreetNameId,
                houseNumber,
                command.GrbNotation,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberFromCrab WithLifetime(this ImportHouseNumberFromCrab command, CrabLifetime lifetime)
        {
            return new ImportHouseNumberFromCrab(
                command.HouseNumberId,
                command.StreetNameId,
                command.HouseNumber,
                command.GrbNotation,
                lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberFromCrab WithInfiniteLifetime(this ImportHouseNumberFromCrab command)
        {
            return new ImportHouseNumberFromCrab(
                command.HouseNumberId,
                command.StreetNameId,
                command.HouseNumber,
                command.GrbNotation,
                new CrabLifetime(new Fixture().Create<LocalDateTime>(), null),
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }
    }
}
