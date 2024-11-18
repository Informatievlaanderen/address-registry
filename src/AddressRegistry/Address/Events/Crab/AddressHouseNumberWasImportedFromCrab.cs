namespace AddressRegistry.Address.Events.Crab
{
    using System;
    using AddressRegistry.Address.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;

    [HideEvent]
    [Obsolete("This is a legacy event and should not be used anymore.")]
    [EventName("AddressHouseNumberWasImportedFromCrab")]
    [EventDescription("Legacy event om tblHuisnummer en tblHuisnummer_hist te importeren.")]
    public class AddressHouseNumberWasImportedFromCrab : IMessage
    {
        [EventPropertyDescription("CRAB-identificator van het huisnummer.")]
        public int HouseNumberId { get; }

        [EventPropertyDescription("CRAB-identificator van de straatnaam die deel uitmaakt van het adres.")]
        public int StreetNameId { get; }

        [EventPropertyDescription("Huisnummer van het adres.")]
        public string HouseNumber { get; }

        [EventPropertyDescription("GRB-notatie van het huisnummer.")]
        public string GrbNotation { get; }

        [EventPropertyDescription("Datum waarop het object is ontstaan in werkelijkheid.")]
        public LocalDateTime? Begin { get; }

        [EventPropertyDescription("Datum waarop het object in werkelijkheid ophoudt te bestaan.")]
        public LocalDateTime? End { get; }

        [EventPropertyDescription("Tijdstip waarop het object werd ingevoerd in de databank.")]
        public Instant Timestamp { get; }

        [EventPropertyDescription("Operator door wie het object werd ingevoerd in de databank.")]
        public string Operator { get; }

        [EventPropertyDescription("Bewerking waarmee het object werd ingevoerd in de databank.")]
        public CrabModification? Modification { get; }

        [EventPropertyDescription("Organisatie die het object heeft ingevoerd in de databank.")]
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
