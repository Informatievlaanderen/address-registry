namespace AddressRegistry.Snapshot.Verifier
{
    public sealed class Scripts
    {
        private readonly string _schema;
        private readonly string _tableName;

        public string SchemaWithTableName => $"{_schema}.{_tableName}";

        public Scripts(string schema, string tableName)
        {
            _schema = schema;
            _tableName = tableName;
        }

        public string GetInitial() => @$"
IF SCHEMA_ID(N'{_schema}') IS NULL EXEC(N'CREATE SCHEMA [{_schema}];');

BEGIN TRANSACTION;

IF OBJECT_ID('[{_schema}].[{_tableName}]', 'U') IS NULL
BEGIN
  CREATE TABLE [{_schema}].[{_tableName}] (
    [SnapshotId] int NOT NULL,
    [Status] nvarchar(450) NOT NULL,
    [Differences] nvarchar(max) NULL,
    CONSTRAINT [PK_{_tableName}] PRIMARY KEY ([SnapshotId])
    );
END;

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE object_id = OBJECT_ID('[{_schema}].[{_tableName}]') AND name = 'IX_{_tableName}_Status')
BEGIN
  CREATE INDEX [IX_{_tableName}_Status] ON [{_schema}].[{_tableName}] ([Status]);
END;

COMMIT;
";

        public string InsertVerificationState() => @$"
INSERT INTO [{_schema}].[{_tableName}] ([SnapshotId], [Status], [Differences])
VALUES (@SnapshotId, @Status, @Differences);";
    }
}
