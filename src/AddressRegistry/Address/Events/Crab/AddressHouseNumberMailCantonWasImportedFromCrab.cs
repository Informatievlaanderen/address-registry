namespace AddressRegistry.Address.Events.Crab
{
    using AddressRegistry.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;

    [EventName("AddressHouseNumberMailCantonWasImportedFromCrab")]
    [EventDescription("Legacy event om tblHuisNummer_postKanton en tblHuisNummer_postKanton_hist te importeren.")]
    public class AddressHouseNumberMailCantonWasImportedFromCrab : ICrabEvent, IHasCrabKey<int>
    {
        public int HouseNumberMailCantonId { get; }
        public int HouseNumberId { get; }
        public int MailCantonId { get; }
        public string MailCantonCode { get; }
        public LocalDateTime? BeginDateTime { get; }
        public LocalDateTime? EndDateTime { get; }
        public Instant Timestamp { get; }
        public string Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public int Key => HouseNumberMailCantonId;

        public AddressHouseNumberMailCantonWasImportedFromCrab(
            CrabHouseNumberMailCantonId houseNumberMailCantonId,
            CrabHouseNumberId houseNumberId,
            CrabMailCantonId mailCantonId,
            CrabMailCantonCode mailCantonCode,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            HouseNumberMailCantonId = houseNumberMailCantonId;
            HouseNumberId = houseNumberId;
            MailCantonId = mailCantonId;
            MailCantonCode = mailCantonCode;
            BeginDateTime = lifetime.BeginDateTime;
            EndDateTime = lifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private AddressHouseNumberMailCantonWasImportedFromCrab(
            int houseNumberMailCantonId,
            int houseNumberId,
            int mailCantonId,
            string mailCantonCode,
            LocalDateTime? beginDateTime,
            LocalDateTime? endDateTime,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation) :
            this(
                new CrabHouseNumberMailCantonId(houseNumberMailCantonId),
                new CrabHouseNumberId(houseNumberId),
                new CrabMailCantonId(mailCantonId),
                new CrabMailCantonCode(mailCantonCode),
                new CrabLifetime(beginDateTime, endDateTime),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation)
        { }
    }
}
