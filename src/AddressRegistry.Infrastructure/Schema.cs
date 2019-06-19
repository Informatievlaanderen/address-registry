namespace AddressRegistry.Infrastructure
{
    public class Schema
    {
        public const string Default = "AddressRegistry";
        public const string Import = "AddressRegistryImport";

        public const string Legacy = "AddressRegistryLegacy";
        public const string Extract = "AddressRegistryExtract";
        public const string Syndication = "AddressRegistrySyndication";
        public const string Sequence = "AddressRegistrySequence";
    }

    public class MigrationTables
    {
        public const string Legacy = "__EFMigrationsHistoryLegacy";
        public const string Extract = "__EFMigrationsHistoryExtract";
        public const string Syndication = "__EFMigrationsHistorySyndication";
        public const string Sequence = "__EFMigrationsHistorySequence";
    }
}

