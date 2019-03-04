namespace AddressRegistry.Address.Events.Crab
{
    using AddressRegistry.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;

    [EventName("AddressHouseNumberWasImportedFromCrab")]
    [EventDescription("Legacy event om tblHuisnummer en tblHuisnummer_hist te importeren.")]
    public class AddressHouseNumberWasImportedFromCrab
    {
        public int HouseNumberId { get; }
        public int StreetNameId { get; }
        public string HouseNumber { get; }
        public string GrbNotation { get; }
        public LocalDateTime? Begin { get; }
        public LocalDateTime? End { get; }
        public Instant Timestamp { get; }
        public string Operator { get; }
        public CrabModification? Modification { get; }
        public CrabOrganisation? Organisation { get; }

        public AddressHouseNumberWasImportedFromCrab(
            CrabHouseNumberId houseNumberId,
            CrabStreetNameId streetNameId,
            HouseNumber houseNumber,
            GrbNotation grbNotation,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            HouseNumberId = houseNumberId;
            StreetNameId = streetNameId;
            HouseNumber = houseNumber;
            GrbNotation = grbNotation;
            Begin = lifetime.BeginDateTime;
            End = lifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private AddressHouseNumberWasImportedFromCrab(
            int houseNumberId,
            int streetNameId,
            string houseNumber,
            string grbNotation,
            LocalDateTime? begin,
            LocalDateTime? end,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation) :
            this(
                new CrabHouseNumberId(houseNumberId),
                new CrabStreetNameId(streetNameId),
                new HouseNumber(houseNumber),
                new GrbNotation(grbNotation),
                new CrabLifetime(begin, end),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation)
        { }
    }
}
