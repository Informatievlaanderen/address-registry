CREATE SINK CONNECTOR `AddressIntegrationDbConnector` with (
    "topics"= 'address.snapshot.oslo.flatten.integrationdb',
    "input.data.format"= 'JSON_SR',
    "input.key.format"= 'JSON_SR',
    "delete.enabled"= false,
    "connector.class"= 'PostgresSink',
    "name"= 'AddressIntegrationDbConnector',
    "kafka.auth.mode"= 'KAFKA_API_KEY',
    "kafka.api.key"= '***', --clear value
    "kafka.api.secret"= '***', --clear value
    "connection.host"= '***', --clear value
    "connection.port"= '5432',
    "connection.user"= '***', --clear value
    "connection.password"= '***', --clear value
    "db.name"= 'postgres',
    "ssl.mode"= 'require',
    "insert.mode"= 'UPSERT',
    "table.name.format"= 'Integration.Addresses',
    "table.types"= 'TABLE',
    "db.timezone"= 'UTC',
    "pk.mode"= 'record_key',
    "pk.fields"= 'PersistentLocalId',
    "auto.create"= false,
    "auto.evolve"= false,
    "quote.sql.identifiers"= 'ALWAYS',
    "batch.sizes"= 3000,
    "tasks.max"= 1
  );
