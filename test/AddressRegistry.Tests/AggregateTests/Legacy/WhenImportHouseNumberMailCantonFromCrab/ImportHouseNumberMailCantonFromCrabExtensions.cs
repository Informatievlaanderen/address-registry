namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportHouseNumberMailCantonFromCrab
{
    using Address.Commands.Crab;
    using Address.Events.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Crab;

    public static class ImportHouseNumberMailCantonFromCrabExtensions
    {
        public static AddressHouseNumberMailCantonWasImportedFromCrab ToLegacyEvent(this ImportHouseNumberMailCantonFromCrab command)
        {
            return new AddressHouseNumberMailCantonWasImportedFromCrab(
                command.HouseNumberMailCantonId,
                command.HouseNumberId,
                command.MailCantonId,
                command.MailCantonCode,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberMailCantonFromCrab WithCrabMailCantonCode(this ImportHouseNumberMailCantonFromCrab command, CrabMailCantonCode mailCantonCode)
        {
            return new ImportHouseNumberMailCantonFromCrab(
                command.HouseNumberMailCantonId,
                command.HouseNumberId,
                command.MailCantonId,
                mailCantonCode,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        public static ImportHouseNumberMailCantonFromCrab WithCrabModification(this ImportHouseNumberMailCantonFromCrab command, CrabModification modification)
        {
            return new ImportHouseNumberMailCantonFromCrab(
                command.HouseNumberMailCantonId,
                command.HouseNumberId,
                command.MailCantonId,
                command.MailCantonCode,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                modification,
                command.Organisation);
        }

        public static ImportHouseNumberMailCantonFromCrab WithLifetime(this ImportHouseNumberMailCantonFromCrab command, CrabLifetime lifetime)
        {
            return new ImportHouseNumberMailCantonFromCrab(
                command.HouseNumberMailCantonId,
                command.HouseNumberId,
                command.MailCantonId,
                command.MailCantonCode,
                lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }
    }
}
