namespace AddressRegistry.Address.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects;
    using ValueObjects.Crab;

    [EventName("AddressHouseNumberPositionWasImportedFromCrab")]
    [EventDescription("Legacy event om tblAdrespositie en tblAdrespositie_hist te importeren voor huisnummers.")]
    public class AddressHouseNumberPositionWasImportedFromCrab : ICrabEvent, IHasCrabPosition, IHasCrabKey<int>
    {
        [EventPropertyDescription("CRAB-identificator van de adrespositie.")]
        public int AddressPositionId { get; }
        
        [EventPropertyDescription("CRAB-identificator van het huisnummer.")]
        public int HouseNumberId { get; }
        
        [EventPropertyDescription("Adrespositie.")]
        public string AddressPosition { get; }
        
        [EventPropertyDescription("Herkomst van de adrespositie.")]
        public CrabAddressPositionOrigin AddressPositionOrigin { get; }
        
        [EventPropertyDescription("Aard van het adres waarvoor de positie werd aangemaakt.")]
        public string AddressNature { get; }
        
        [EventPropertyDescription("Datum waarop het object is ontstaan in werkelijkheid.")]
        public LocalDateTime? BeginDateTime { get; }
        
        [EventPropertyDescription("Datum waarop het object in werkelijkheid ophoudt te bestaan.")]
        public LocalDateTime? EndDateTime { get; }
        
        [EventPropertyDescription("Tijdstip waarop het object werd ingevoerd in de databank.")] 
        public Instant Timestamp { get; }
        
        [EventPropertyDescription("Operator door wie het object werd ingevoerd in de databank.")]
        public string Operator { get; }
        
        [EventPropertyDescription("Bewerking waarmee het object werd ingevoerd in de databank.")] 
        public CrabModification? Modification { get; }
        
        [EventPropertyDescription("Organisatie die het object heeft ingevoerd in de databank.")]
        public CrabOrganisation? Organisation { get; }

        [EventPropertyDescription("Unieke sleutel.")]
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
