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
        [EventPropertyDescription("CRAB-identificator van de relatie huisnummer-postkanton.")]
        public int HouseNumberMailCantonId { get; }
        
        [EventPropertyDescription("CRAB-identificator van het huisnummer.")]
        public int HouseNumberId { get; }
        
        [EventPropertyDescription("CRAB-identificator van het postkanton.")]
        public int MailCantonId { get; }
        
        [EventPropertyDescription("Postkantoncode.")]
        public string MailCantonCode { get; }
        
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
