SET IDENTITY_INSERT AddressRegistryLegacy.AddressSyndication_2 ON;
GO

INSERT INTO [AddressRegistryLegacy].[AddressSyndication_2]
([Position]
  ,[AddressId]
  ,[PersistentLocalId]
  ,[ChangeType]
  ,[StreetNameId]
  ,[PostalCode]
  ,[HouseNumber]
  ,[BoxNumber]
  ,[PointPosition]
  ,[PositionMethod]
  ,[PositionSpecification]
  ,[Status]
  ,[IsComplete]
  ,[IsOfficiallyAssigned]
  ,[RecordCreatedAt]
  ,[LastChangedOn]
  ,[Application]
  ,[Modification]
  ,[Operator]
  ,[Organisation]
  ,[Reason]
  ,[EventDataAsXml]
  ,[SyndicationItemCreatedAt]
  ,[StreetNamePersistentLocalId]
  ,[FeedPosition])
SELECT *, Position FROM [AddressRegistryLegacy].AddressSyndication
WHERE Position > (SELECT MAX(Position) FROM [AddressRegistryLegacy].[AddressSyndication_2]);

BEGIN TRAN
GO

DECLARE @maxId BIGINT = (SELECT MAX(Position) FROM AddressRegistryLegacy.AddressSyndication);
DBCC CHECKIDENT ('AddressRegistryLegacy.AddressSyndication_2', RESEED, @maxId);
GO

SET IDENTITY_INSERT AddressRegistryLegacy.AddressSyndication_2 OFF;
GO

DROP TABLE [AddressRegistryLegacy].[AddressSyndication];
GO

EXEC sp_rename 'AddressRegistryLegacy.AddressSyndication_2', 'AddressSyndication';
EXEC sp_rename N'AddressRegistryLegacy.AddressSyndication.IX_AddressSyndication_AddressId_2', N'IX_AddressSyndication_AddressId', N'INDEX';
EXEC sp_rename N'AddressRegistryLegacy.AddressSyndication.IX_AddressSyndication_PersistentLocalId_2', N'IX_AddressSyndication_PersistentLocalId', N'INDEX';
EXEC sp_rename N'AddressRegistryLegacy.AddressSyndication.IX_AddressSyndication_Position_2', N'IX_AddressSyndication_Position', N'INDEX';
EXEC sp_rename N'AddressRegistryLegacy.AddressSyndication.PK_AddressSyndication_2', N'PK_AddressSyndication', N'INDEX';

INSERT INTO [AddressRegistryLegacy].[__EFMigrationsHistoryLegacy] ([MigrationId], [ProductVersion])
VALUES (N'20250211123736_AddFeedPosition', N'8.0.3');
GO

COMMIT;
GO
