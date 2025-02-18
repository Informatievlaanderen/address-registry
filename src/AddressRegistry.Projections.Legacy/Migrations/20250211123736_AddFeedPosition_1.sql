-- Part 1 to create table and filling up data
CREATE TABLE [AddressRegistryLegacy].[AddressSyndication_2](
  [Position] [bigint] NOT NULL,
  [AddressId] [uniqueidentifier] NULL,
  [PersistentLocalId] [int] NULL,
  [ChangeType] [nvarchar](max) NULL,
  [StreetNameId] [uniqueidentifier] NULL,
  [PostalCode] [nvarchar](max) NULL,
  [HouseNumber] [nvarchar](max) NULL,
  [BoxNumber] [nvarchar](max) NULL,
  [PointPosition] [varbinary](max) NULL,
  [PositionMethod] [int] NULL,
  [PositionSpecification] [int] NULL,
  [Status] [int] NULL,
  [IsComplete] [bit] NOT NULL,
  [IsOfficiallyAssigned] [bit] NOT NULL,
  [RecordCreatedAt] [datetimeoffset](7) NOT NULL,
  [LastChangedOn] [datetimeoffset](7) NOT NULL,
  [Application] [int] NULL,
  [Modification] [int] NULL,
  [Operator] [nvarchar](max) NULL,
  [Organisation] [int] NULL,
  [Reason] [nvarchar](max) NULL,
  [EventDataAsXml] [nvarchar](max) NULL,
  [SyndicationItemCreatedAt] [datetimeoffset](7) NOT NULL,
  [StreetNamePersistentLocalId] [int] NULL,
  CONSTRAINT [PK_AddressSyndication_2] PRIMARY KEY CLUSTERED
(
[Position] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
  ) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
  GO

-- NEW
ALTER TABLE [AddressRegistryLegacy].[AddressSyndication_2] ADD [FeedPosition] bigint NOT NULL IDENTITY;
GO

SET IDENTITY_INSERT AddressRegistryLegacy.AddressSyndication_2 ON;
GO

ALTER TABLE [AddressRegistryLegacy].[AddressSyndication_2] DROP CONSTRAINT [PK_AddressSyndication_2];
GO

ALTER TABLE [AddressRegistryLegacy].[AddressSyndication_2] ADD CONSTRAINT [PK_AddressSyndication_2] PRIMARY KEY CLUSTERED ([FeedPosition]);
GO

truncate table[AddressRegistryLegacy].[AddressSyndication_2];
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
SELECT *, Position FROM [AddressRegistryLegacy].AddressSyndication;
GO

CREATE NONCLUSTERED INDEX [IX_AddressSyndication_AddressId_2] ON [AddressRegistryLegacy].[AddressSyndication_2]
(
	[AddressId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_AddressSyndication_PersistentLocalId_2] ON [AddressRegistryLegacy].[AddressSyndication_2]
(
	[PersistentLocalId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
GO

CREATE COLUMNSTORE INDEX [CI_AddressSyndication_FeedPosition] ON [AddressRegistryLegacy].[AddressSyndication_2] ([FeedPosition]);
GO

CREATE INDEX [IX_AddressSyndication_Position_2] ON [AddressRegistryLegacy].[AddressSyndication_2] ([Position]);
GO
