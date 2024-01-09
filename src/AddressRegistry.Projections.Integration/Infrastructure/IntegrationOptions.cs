namespace AddressRegistry.Projections.Integration.Infrastructure
{
    public class IntegrationOptions
    {
        public string Namespace { get; set; }
        public string EventsConnectionString { get; set; }
        public string MunicipalityGeometriesConnectionString { get; set; }
    }
}
