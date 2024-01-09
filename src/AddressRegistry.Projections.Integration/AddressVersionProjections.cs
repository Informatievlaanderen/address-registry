namespace AddressRegistry.Projections.Integration
{
    using System.Threading;
    using System.Threading.Tasks;
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Convertors;
    using Dapper;
    using Infrastructure;
    using Microsoft.Data.SqlClient;
    using Microsoft.Extensions.Options;
    using NetTopologySuite.Geometries;
    using StreetName;
    using StreetName.Events;

    [ConnectedProjectionName("Integratie adres versie")]
    [ConnectedProjectionDescription("Projectie die de laatste adres data voor de integratie database bijhoudt.")]
    public sealed class AddressVersionProjections : ConnectedProjection<IntegrationContext>
    {
        public AddressVersionProjections(IOptions<IntegrationOptions> options)
        {
            // StreetName
            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context.CreateNewAddressVersion(
                        new PersistentLocalId(addressPersistentLocalId),
                        message,
                        _ => { },
                        ct);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context.CreateNewAddressVersion(
                        new PersistentLocalId(addressPersistentLocalId),
                        message,
                        _ => { },
                        ct);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context.CreateNewAddressVersion(
                        new PersistentLocalId(addressPersistentLocalId),
                        message,
                        _ => { },
                        ct);
                }
            });

            // Address
            When<Envelope<AddressWasMigratedToStreetName>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());
                var nisCode = await FindNisCode(geometry, ct);

                var addressVersion = new AddressVersion()
                {
                    Position = message.Position,
                    PersistentLocalId = message.Message.AddressPersistentLocalId,
                    NisCode = nisCode,
                    PostalCode = message.Message.PostalCode,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    Status = message.Message.Status.Map(),
                    HouseNumber = message.Message.HouseNumber,
                    BoxNumber = message.Message.BoxNumber,
                    Geometry = geometry,
                    PositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode(),
                    PositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie(),
                    OfficiallyAssigned = message.Message.OfficiallyAssigned,
                    Removed = message.Message.IsRemoved,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.Namespace,
                    PuriId = $"{options.Value.Namespace}/{message.Message.AddressPersistentLocalId}",
                };

                await context.AddressVersions.AddAsync(addressVersion, ct);
            });

            When<Envelope<AddressWasProposedV2>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());
                var nisCode = await FindNisCode(geometry, ct);

                var addressVersion = new AddressVersion()
                {
                    Position = message.Position,
                    PersistentLocalId = message.Message.AddressPersistentLocalId,
                    NisCode = nisCode,
                    PostalCode = message.Message.PostalCode,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    Status = AddressStatus.Proposed.Map(),
                    HouseNumber = message.Message.HouseNumber,
                    BoxNumber = message.Message.BoxNumber,
                    Geometry = geometry,
                    PositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode(),
                    PositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie(),
                    OfficiallyAssigned = true,
                    Removed = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.Namespace,
                    PuriId = $"{options.Value.Namespace}/{message.Message.AddressPersistentLocalId}",
                };

                await context.AddressVersions.AddAsync(addressVersion, ct);
            });

            When<Envelope<AddressWasApproved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Current.ToString();
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed.ToString();
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed.ToString();
                    },
                    ct);
            });

            When<Envelope<AddressWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected.ToString();
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected.ToString();
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected.ToString();
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected.ToString();
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired.ToString();
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected.ToString();
                    },
                    ct);
            });

            When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.OfficiallyAssigned = false;
                        item.Status = AddressStatus.Current.ToString();
                    },
                    ct);
            });

            When<Envelope<AddressWasRegularized>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.OfficiallyAssigned = true;
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired.ToString();
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired.ToString();
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired.ToString();
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Current.ToString();
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.PostalCode = message.Message.PostalCode;
                    },
                    ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.CreateNewAddressVersion(
                        new PersistentLocalId(boxNumberPersistentLocalId),
                        message,
                        boxNumberItem =>
                        {
                            boxNumberItem.PostalCode = message.Message.PostalCode;
                        },
                        ct);
                }
            });

            When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.PostalCode = message.Message.PostalCode;
                    },
                    ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.CreateNewAddressVersion(
                        new PersistentLocalId(boxNumberPersistentLocalId),
                        message,
                        boxNumberItem =>
                        {
                            boxNumberItem.PostalCode = message.Message.PostalCode;
                        },
                        ct);
                }
            });

            When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.HouseNumber = message.Message.HouseNumber;
                    },
                    ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.CreateNewAddressVersion(
                        new PersistentLocalId(boxNumberPersistentLocalId),
                        message,
                        boxNumberItem =>
                        {
                            boxNumberItem.HouseNumber = message.Message.HouseNumber;
                        },
                        ct);
                }
            });

            When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.BoxNumber = message.Message.BoxNumber;
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasChanged>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.PositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode();
                        item.PositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie();
                        item.Geometry = geometry;
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.PositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode();
                        item.PositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie();
                        item.Geometry = geometry;
                    },
                    ct);
            });

            When<Envelope<AddressHouseNumberWasReaddressed>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy()
                    .Read(message.Message.ReaddressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray());

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = message.Message.ReaddressedHouseNumber.SourceStatus.Map();
                        item.HouseNumber = message.Message.ReaddressedHouseNumber.DestinationHouseNumber;
                        item.PostalCode = message.Message.ReaddressedHouseNumber.SourcePostalCode;
                        item.OfficiallyAssigned = message.Message.ReaddressedHouseNumber.SourceIsOfficiallyAssigned;
                        item.PositionMethod = message.Message.ReaddressedHouseNumber.SourceGeometryMethod.ToPositieGeometrieMethode();
                        item.PositionSpecification = message.Message.ReaddressedHouseNumber.SourceGeometrySpecification.ToPositieSpecificatie();
                        item.Geometry = geometry;
                    },
                    ct);

                foreach (var readdressedBoxNumber in message.Message.ReaddressedBoxNumbers)
                {
                    var boxNumberGeometry = WKBReaderFactory.CreateForLegacy()
                        .Read(message.Message.ReaddressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray());

                    await context.CreateNewAddressVersion(
                        new PersistentLocalId(readdressedBoxNumber.DestinationAddressPersistentLocalId),
                        message,
                        item =>
                        {
                            item.Status = readdressedBoxNumber.SourceStatus.Map();
                            item.HouseNumber = readdressedBoxNumber.DestinationHouseNumber;
                            item.BoxNumber = readdressedBoxNumber.SourceBoxNumber;
                            item.PostalCode = readdressedBoxNumber.SourcePostalCode;
                            item.OfficiallyAssigned = readdressedBoxNumber.SourceIsOfficiallyAssigned;
                            item.PositionMethod = readdressedBoxNumber.SourceGeometryMethod.ToPositieGeometrieMethode();
                            item.PositionSpecification = readdressedBoxNumber.SourceGeometrySpecification.ToPositieSpecificatie();
                            item.Geometry = boxNumberGeometry;
                        },
                        ct);
                }
            });

            When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());
                var nisCode = await FindNisCode(geometry, ct);

                var addressDetailItemV2 = new AddressVersion
                {
                    Position = message.Position,
                    PersistentLocalId = new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    NisCode = nisCode,
                    PostalCode = message.Message.PostalCode,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    Status = AddressStatus.Proposed.Map(),
                    HouseNumber = message.Message.HouseNumber,
                    BoxNumber = message.Message.BoxNumber,
                    Geometry = geometry,
                    PositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode(),
                    PositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie(),
                    OfficiallyAssigned = true,
                    Removed = false,
                    VersionTimestamp = message.Message.Provenance.Timestamp,
                    Namespace = options.Value.Namespace,
                    PuriId = $"{options.Value.Namespace}/{message.Message.AddressPersistentLocalId}",
                };

                await context
                    .AddressVersions
                    .AddAsync(addressDetailItemV2, ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfReaddress>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Removed = true;
                    },
                    ct);
            });

            When<Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Removed = true;
                    },
                    ct);
            });

            When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Removed = true;
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed.Map();
                    },
                    ct);
            });

            When<Envelope<AddressRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.OfficiallyAssigned = false;
                        item.Status = AddressStatus.Current.Map();
                    },
                    ct);
            });

            When<Envelope<AddressDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.AddressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.OfficiallyAssigned = true;
                    },
                    ct);
            });

            #region Legacy

            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                //TODO: get the streetnamePersistentLocalId?
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.HouseNumber = message.Message.HouseNumber;
                    },
                    ct);
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<AddressBecameCurrent>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Current.Map();
                    },
                    ct);
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<AddressBecameNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.OfficiallyAssigned = false;
                    },
                    ct);
            });

            When<Envelope<AddressHouseNumberWasChanged>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.HouseNumber = message.Message.HouseNumber;
                    },
                    ct);
            });

            When<Envelope<AddressHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.HouseNumber = message.Message.HouseNumber;
                    },
                    ct);
            });

            When<Envelope<AddressOfficialAssignmentWasRemoved>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.OfficiallyAssigned = null;
                    },
                    ct);
            });

            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    new PersistentLocalId(message.Message.PersistentLocalId),
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<AddressPositionWasCorrected>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                var geometry = WKBReaderFactory.CreateForLegacy()
                    .Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.PositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode();
                        item.PositionSpecification =  message.Message.GeometrySpecification.ToPositieSpecificatie();
                        item.Geometry = geometry;
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasRemoved>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Geometry = null;
                        item.PositionMethod = null;
                        item.PositionSpecification = null;
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasChanged>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.PostalCode = message.Message.PostalCode;
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasCorrected>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.PostalCode = message.Message.PostalCode;
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasRemoved>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.PostalCode = null;
                    },
                    ct);
            });

            When<Envelope<AddressStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = null;
                    },
                    ct);
            });

            When<Envelope<AddressStatusWasRemoved>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = null;
                    },
                    ct);
            });

            When<Envelope<AddressStreetNameWasChanged>>(async (context, message, ct) =>
            {
                //TODO: get the streetnamePersistentLocalId?
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<AddressStreetNameWasCorrected>>(async (context, message, ct) =>
            {
                //TODO: get the streetnamePersistentLocalId?
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    _ => { },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Current.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.OfficiallyAssigned = false;
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToOfficiallyAssigned>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.OfficiallyAssigned = true;
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasOfficiallyAssigned>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.OfficiallyAssigned = true;
                    },
                    ct);
            });

            When<Envelope<AddressWasPositioned>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                var geometry = WKBReaderFactory.CreateForLegacy()
                    .Read(message.Message.ExtendedWkbGeometry.ToByteArray());

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.PositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode();
                        item.PositionSpecification =  message.Message.GeometrySpecification.ToPositieSpecificatie();
                        item.Geometry = geometry;
                    },
                    ct);
            });

            When<Envelope<AddressWasProposed>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.PostalCode = AddressStatus.Proposed.Map();
                    },
                    ct);
            });

            When<Envelope<AddressWasRemoved>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Removed = true;
                    },
                    ct);
            });

            When<Envelope<AddressWasRetired>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.Status = AddressStatus.Retired.Map();
                    },
                    ct);
            });

            When<Envelope<AddressBoxNumberWasChanged>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.BoxNumber = message.Message.BoxNumber;
                    },
                    ct);
            });
            When<Envelope<AddressBoxNumberWasCorrected>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.BoxNumber = message.Message.BoxNumber;
                    },
                    ct);
            });
            When<Envelope<AddressBoxNumberWasRemoved>>(async (context, message, ct) =>
            {
                int addressPersistentLocalId;
                using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT Json_Value(JsonData, '$.persistentLocalId') AS ""AddressPersistentLocalId""
                    FROM [address-registry-events].[AddressRegistry].[Streams] as s
                    inner join [address-registry-events].[AddressRegistry].[Messages] as m on s.IdInternal = m.StreamIdInternal and m.[Type] = 'AddressPersistentLocalIdentifierWasAssigned'
                    where IdOriginal = '{message.Message.AddressId}'";

                    addressPersistentLocalId = connection.QuerySingle<int>(sql);
                }

                await context.CreateNewAddressVersion(
                    new PersistentLocalId(addressPersistentLocalId),
                    message,
                    item =>
                    {
                        item.BoxNumber = null;
                    },
                    ct);
            });

            #endregion

            async Task<string?> FindNisCode(Geometry geometry, CancellationToken ct)
            {
                string nisCode;
                await using (var connection = new SqlConnection(options.Value.EventsConnectionString))
                {
                    var sql = @$"SELECT nis_code
                                FROM integration_municipality.municipality_geometries
                                WHERE ST_Within(
                                    ${geometry},
                                    integration_municipality.municipality_geometries.geometry
                                )
                                LIMIT 1;";

                    nisCode = connection.QuerySingle<string>(sql);
                }

                return nisCode;
            }
        }
    }
}
