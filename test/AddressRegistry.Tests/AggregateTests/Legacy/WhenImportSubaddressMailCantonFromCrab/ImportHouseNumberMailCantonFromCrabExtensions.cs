namespace AddressRegistry.Tests.AggregateTests.Legacy.WhenImportSubaddressMailCantonFromCrab
{
    using Address.Commands.Crab;
    using Address.Crab;
    using Address.Events.Crab;

    public static class ImportSubaddressMailCantonFromCrabExtensions
    {
        public static AddressHouseNumberMailCantonWasImportedFromCrab ToLegacyEvent(this ImportSubaddressMailCantonFromCrab command)
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

        public static ImportSubaddressMailCantonFromCrab WithCrabMailCantonCode(this ImportSubaddressMailCantonFromCrab command, CrabMailCantonCode mailCantonCode)
        {
            return new ImportSubaddressMailCantonFromCrab(
                command.HouseNumberMailCantonId,
                command.HouseNumberId,
                command.SubaddressId,
                command.MailCantonId,
                mailCantonCode,
                command.Lifetime,
                command.Timestamp,
                command.Operator,
                command.Modification,
                command.Organisation);
        }

        //public static ImportHouseNumberMailCantonFromCrab WithCrabModification(this ImportHouseNumberMailCantonFromCrab command, CrabModification modification)
        //{
        //    return new ImportHouseNumberMailCantonFromCrab(
        //        command.HouseNumberMailCantonId,
        //        command.HouseNumberId,
        //        command.MailCantonId,
        //        command.MailCantonCode,
        //        command.Lifetime,
        //        command.Timestamp,
        //        command.Operator,
        //        modification,
        //        command.Organisation);
        //}

        //public static ImportHouseNumberMailCantonFromCrab WithLifetime(this ImportHouseNumberMailCantonFromCrab command, CrabLifetime lifetime)
        //{
        //    return new ImportHouseNumberMailCantonFromCrab(
        //        command.HouseNumberMailCantonId,
        //        command.HouseNumberId,
        //        command.MailCantonId,
        //        command.MailCantonCode,
        //        lifetime,
        //        command.Timestamp,
        //        command.Operator,
        //        command.Modification,
        //        command.Organisation);
        //}
    }
}
