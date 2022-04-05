namespace AddressRegistry.Address.Events.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Newtonsoft.Json;
    using NodaTime;
    using ValueObjects.Crab;

    [EventName("AddressSubaddressStatusWasImportedFromCrab")]
    [EventDescription("Legacy event om tblSubadresstatus en tblSubadresstatus_hist te importeren.")]
    public class AddressSubaddressStatusWasImportedFromCrab : ICrabEvent, IHasCrabAddressStatus, IHasCrabKey<int>
    {
        [EventPropertyDescription("CRAB-identificator van de subadresstatus.")]
        public int SubaddressStatusId { get; }
        
        [EventPropertyDescription("CRAB-identificator van het subadres (bus- of appartementsnummer).")]
        public int SubaddressId { get; }
        
        [EventPropertyDescription("Subadresstatus.")]
        public CrabAddressStatus AddressStatus { get; }
        
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
        public int Key => SubaddressStatusId;

        public AddressSubaddressStatusWasImportedFromCrab(
            CrabSubaddressStatusId subaddressStatusId,
            CrabSubaddressId subaddressId,
            CrabAddressStatus addressStatus,
            CrabLifetime lifetime,
            CrabTimestamp timestamp,
            CrabOperator @operator,
            CrabModification? modification,
            CrabOrganisation? organisation)
        {
            SubaddressStatusId = subaddressStatusId;
            SubaddressId = subaddressId;
            AddressStatus = addressStatus;
            BeginDateTime = lifetime.BeginDateTime;
            EndDateTime = lifetime.EndDateTime;
            Timestamp = timestamp;
            Operator = @operator;
            Modification = modification;
            Organisation = organisation;
        }

        [JsonConstructor]
        private AddressSubaddressStatusWasImportedFromCrab(
            int subaddressStatusId,
            int subaddressId,
            CrabAddressStatus addressStatus,
            LocalDateTime? beginDateTime,
            LocalDateTime? endDateTime,
            Instant timestamp,
            string @operator,
            CrabModification? modification,
            CrabOrganisation? organisation) :
            this(
                new CrabSubaddressStatusId(subaddressStatusId),
                new CrabSubaddressId(subaddressId),
                addressStatus,
                new CrabLifetime(beginDateTime, endDateTime),
                new CrabTimestamp(timestamp),
                new CrabOperator(@operator),
                modification,
                organisation)
        { }
    }
}
