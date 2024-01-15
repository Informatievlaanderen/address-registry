namespace AddressRegistry.Infrastructure
{
    public static class Schema
    {
        public const string Default = "AddressRegistry";
        public const string Import = "AddressRegistryImport";
        public const string Legacy = "AddressRegistryLegacy";
        public const string Extract = "AddressRegistryExtract";
        public const string Syndication = "AddressRegistrySyndication";
        public const string Sequence = "AddressRegistrySequence";
        public const string Consumer = "AddressRegistryConsumerStreetName";
        public const string ConsumerProjections = "AddressRegistryConsumer";
        public const string ConsumerReadMunicipality = "AddressRegistryConsumerReadMunicipality";
        public const string ConsumerReadStreetName = "AddressRegistryConsumerReadStreetName";
        public const string MigrateAddress = "AddressRegistryMigration";
        public const string BackOffice = "AddressRegistryBackOffice";
        public const string BackOfficeProjections = "AddressRegistryBackOfficeProjections";
        public const string Wfs = "wfs.address";
        public const string Wms = "wms.address";
        public const string Producer = "AddressRegistryProducer";
        public const string ProducerSnapshotOslo = "AddressRegistryProducerSnapshotOslo";
        public const string Integration = "integration_address";
    }

    public static class MigrationTables
    {
        public const string Legacy = "__EFMigrationsHistoryLegacy";
        public const string Extract = "__EFMigrationsHistoryExtract";
        public const string Syndication = "__EFMigrationsHistorySyndication";
        public const string RedisDataMigration = "__EFMigrationsHistoryDataMigration";
        public const string Sequence = "__EFMigrationsHistorySequence";
        public const string Consumer = "__EFMigrationsHistoryConsumerStreetName";
        public const string ConsumerProjections = "__EFMigrationsHistoryConsumer";
        public const string ConsumerReadMunicipality = "__EFMigrationsHistoryConsumerReadMunicipality";
        public const string ConsumerReadStreetName = "__EFMigrationsHistoryConsumerReadStreetName";
        public const string BackOffice = "__EFMigrationsHistoryBackOffice";
        public const string BackOfficeProjections = "__EFMigrationsHistoryBackOfficeProjections";
        public const string Wfs = "__EFMigrationsHistoryWfsAddress";
        public const string Wms = "__EFMigrationsHistoryWmsAddress";
        public const string Producer = "__EFMigrationsHistoryProducer";
        public const string ProducerSnapshotOslo = "__EFMigrationsHistoryProducerSnapshotOslo";
        public const string Integration = "__EFMigrationsHistory";
    }
}

