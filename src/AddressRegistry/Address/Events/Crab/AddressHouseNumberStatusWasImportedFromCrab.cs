namespace AddressRegistry.Address.Events.Crab
{
    using AddressRegistry.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;

    [EventName("AddressHouseNumberStatusWasImportedFromCrab")]
    [EventDescription("Legacy event om tblHuisnummerstatus en tblHuisnummerstatus_hist te importeren.")]
    public class AddressHouseNumberStatusWasImportedFromCrab : ICrabEvent, IHasCrabAddressStatus, IHasCrabKey<int>
    {
        public int HouseNumberStatusId { get; }
        public int HouseNumberId { get; }
        public CrabAddressStatus AddressStatus { get; }
        public LocalDateTime? BeginDateTime { get; }
        public LocalDateTime? EndDateTime { get; }
        public Instant Timestamp { get; }
        public string Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public int Key => HouseNumberStatusId;

        public AddressHouseNumberStatusWasImportedFromCrab(
            CrabHouseNumberStatusId houseNumberStatusId,
            CrabHouseNumberId houseNumberId,
            CrabAddressStatus addressStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            HouseNumberStatusId = houseNumberStatusId;
            HouseNumberId = houseNumberId;
            AddressStatus = addressStatus;
            BeginDateTime = lifetime.BeginDateTime;
            EndDateTime = lifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private AddressHouseNumberStatusWasImportedFromCrab(
            int houseNumberStatusId,
            int houseNumberId,
            CrabAddressStatus addressStatus,
            LocalDateTime? beginDateTime,
            LocalDateTime? endDateTime,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation) :
            this(
                new CrabHouseNumberStatusId(houseNumberStatusId),
                new CrabHouseNumberId(houseNumberId),
                addressStatus,
                new CrabLifetime(beginDateTime, endDateTime),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation)
        { }
    }
}
