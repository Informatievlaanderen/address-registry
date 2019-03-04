namespace AddressRegistry.Address.Events.Crab
{
    using AddressRegistry.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Newtonsoft.Json;
    using NodaTime;

    [EventName("AddressHouseNumberPositionWasImportedFromCrab")]
    [EventDescription("Legacy event om tblAdrespositie en tblAdrespositie_hist te importeren voor huisnummers.")]
    public class AddressHouseNumberPositionWasImportedFromCrab : ICrabEvent, IHasCrabPosition, IHasCrabKey<int>
    {
        public int AddressPositionId { get; }
        public int HouseNumberId { get; }
        public string AddressPosition { get; }
        public CrabAddressPositionOrigin AddressPositionOrigin { get; }
        public string AddressNature { get; }
        public LocalDateTime? BeginDateTime { get; }
        public LocalDateTime? EndDateTime { get; }
        public Instant Timestamp { get; }
        public string Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public int Key => AddressPositionId;

        public AddressHouseNumberPositionWasImportedFromCrab(
            CrabAddressPositionId addressPositionId,
            CrabHouseNumberId houseNumberId,
            WkbGeometry addressPosition,
            CrabAddressPositionOrigin addressPositionOrigin,
            CrabAddressNature addressNature,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            AddressPositionId = addressPositionId;
            HouseNumberId = houseNumberId;
            AddressPosition = addressPosition;
            AddressPositionOrigin = addressPositionOrigin;
            AddressNature = addressNature;
            BeginDateTime = lifetime.BeginDateTime;
            EndDateTime = lifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private AddressHouseNumberPositionWasImportedFromCrab(
            int addressPositionId,
            int houseNumberId,
            string addressPosition,
            CrabAddressPositionOrigin addressPositionOrigin,
            string addressNature,
            LocalDateTime? beginDateTime,
            LocalDateTime? endDateTime,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation) :
            this(
                new CrabAddressPositionId(addressPositionId),
                new CrabHouseNumberId(houseNumberId),
                new WkbGeometry(addressPosition.ToByteArray()),
                addressPositionOrigin,
                new CrabAddressNature(addressNature),
                new CrabLifetime(beginDateTime, endDateTime),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation)
        { }
    }
}
