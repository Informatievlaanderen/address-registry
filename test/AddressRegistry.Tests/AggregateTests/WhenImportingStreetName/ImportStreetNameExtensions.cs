namespace AddressRegistry.Tests.AggregateTests.WhenImportingStreetName
{
    using StreetName;
    using StreetName.Commands;

    public static class ImportStreetNameExtensions
    {
        public static ImportStreetName WithMunicipalityId(
            this ImportStreetName command,
            MunicipalityId municipalityId)
        {
            return new ImportStreetName(
                command.PersistentLocalId,
                municipalityId,
                command.StreetNameStatus,
                command.Provenance);
        }

        public static ImportStreetName WithPersistentLocalId(
            this ImportStreetName command,
            StreetNamePersistentLocalId persistentLocalId)
        {
            return new ImportStreetName(
                persistentLocalId,
                command.MunicipalityId,
                command.StreetNameStatus,
                command.Provenance);
        }
    }
}
