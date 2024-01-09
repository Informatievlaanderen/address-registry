// namespace AddressRegistry.Projections.Integration
// {
//     using System.Threading;
//     using System.Threading.Tasks;
//     using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
//     using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
//     using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
//     using Convertors;
//     using Dapper;
//     using Infrastructure;
//     using Microsoft.Data.SqlClient;
//     using Microsoft.Extensions.Options;
//     using NetTopologySuite.Geometries;
//     using NodaTime;
//     using Npgsql;
//     using StreetName;
//     using StreetName.Events;
//
//     [ConnectedProjectionName("Integratie adres latest item")]
//     [ConnectedProjectionDescription("Projectie die de laatste adres data voor de integratie database bijhoudt.")]
//     public sealed class AddressLatestItemProjections : ConnectedProjection<IntegrationContext>
//     {
//         public AddressLatestItemProjections(IOptions<IntegrationOptions> options)
//         {
//             // StreetName
//             When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
//             {
//                 foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
//                 {
//                     await context.FindAndUpdateAddressLatestItem(
//                         addressPersistentLocalId,
//                         message.Position,
//                         item => { UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp); },
//                         ct);
//                 }
//             });
//
//             When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
//             {
//                 foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
//                 {
//                     await context.FindAndUpdateAddressLatestItem(
//                         addressPersistentLocalId,
//                         message.Position,
//                         item => { UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp); },
//                         ct);
//                 }
//             });
//
//             When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
//             {
//                 foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
//                 {
//                     await context.FindAndUpdateAddressLatestItem(
//                         addressPersistentLocalId,
//                         message.Position,
//                         item => { UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp); },
//                         ct);
//                 }
//             });
//
//             // Address
//             When<Envelope<AddressWasMigratedToStreetName>>(async (context, message, ct) =>
//             {
//                 var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());
//                 var nisCode = await FindNisCode(geometry);
//
//                 var addressLatestItem = new AddressLatestItem()
//                 {
//                     PersistentLocalId = message.Message.AddressPersistentLocalId,
//                     NisCode = nisCode,
//                     PostalCode = message.Message.PostalCode,
//                     StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
//                     Status = message.Message.Status.Map(),
//                     HouseNumber = message.Message.HouseNumber,
//                     BoxNumber = message.Message.BoxNumber,
//                     Geometry = geometry,
//                     PositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode(),
//                     PositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie(),
//                     OfficiallyAssigned = message.Message.OfficiallyAssigned,
//                     Removed = message.Message.IsRemoved,
//                     VersionTimestamp = message.Message.Provenance.Timestamp,
//                     Namespace = options.Value.Namespace,
//                     PuriId = $"{options.Value.Namespace}/{message.Message.AddressPersistentLocalId}",
//                     IdempotenceKey = message.Position
//                 };
//
//                 await context
//                     .AddressLatestItems
//                     .AddAsync(addressLatestItem, ct);
//             });
//
//             When<Envelope<AddressWasProposedV2>>(async (context, message, ct) =>
//             {
//                 var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());
//                 var nisCode = await FindNisCode(geometry);
//
//                 var addressLatestItem = new AddressLatestItem()
//                 {
//                     PersistentLocalId = message.Message.AddressPersistentLocalId,
//                     NisCode = nisCode,
//                     PostalCode = message.Message.PostalCode,
//                     StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
//                     Status = AddressStatus.Proposed.Map(),
//                     HouseNumber = message.Message.HouseNumber,
//                     BoxNumber = message.Message.BoxNumber,
//                     Geometry = geometry,
//                     PositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode(),
//                     PositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie(),
//                     OfficiallyAssigned = true,
//                     Removed = false,
//                     VersionTimestamp = message.Message.Provenance.Timestamp,
//                     Namespace = options.Value.Namespace,
//                     PuriId = $"{options.Value.Namespace}/{message.Message.AddressPersistentLocalId}",
//                     IdempotenceKey = message.Position
//                 };
//
//                 await context
//                     .AddressLatestItems
//                     .AddAsync(addressLatestItem, ct);
//             });
//
//             When<Envelope<AddressWasApproved>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Current.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Proposed.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Proposed.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasRejected>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Rejected.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Rejected.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Rejected.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Rejected.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Retired.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Rejected.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.OfficiallyAssigned = false;
//                         item.Status = AddressStatus.Current.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasRegularized>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.OfficiallyAssigned = true;
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasRetiredV2>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Retired.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Retired.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Retired.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Current.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressPostalCodeWasChangedV2>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.PostalCode = message.Message.PostalCode;
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//
//                 foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
//                 {
//                     await context.FindAndUpdateAddressLatestItem(
//                         boxNumberPersistentLocalId,
//                         message.Position,
//                         boxNumberItem =>
//                         {
//                             boxNumberItem.PostalCode = message.Message.PostalCode;
//                             UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
//                         },
//                         ct);
//                 }
//             });
//
//             When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.PostalCode = message.Message.PostalCode;
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//
//                 foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
//                 {
//                     await context.FindAndUpdateAddressLatestItem(
//                         boxNumberPersistentLocalId,
//                         message.Position,
//                         boxNumberItem =>
//                         {
//                             boxNumberItem.PostalCode = message.Message.PostalCode;
//                             UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
//                         },
//                         ct);
//                 }
//             });
//
//             When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.HouseNumber = message.Message.HouseNumber;
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//
//                 foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
//                 {
//                     await context.FindAndUpdateAddressLatestItem(
//                         boxNumberPersistentLocalId,
//                         message.Position,
//                         boxNumberItem =>
//                         {
//                             boxNumberItem.HouseNumber = message.Message.HouseNumber;
//                             UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
//                         },
//                         ct);
//                 }
//             });
//
//             When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.BoxNumber = message.Message.BoxNumber;
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressPositionWasChanged>>(async (context, message, ct) =>
//             {
//                 var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());
//
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.PositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode();
//                         item.PositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie();
//                         item.Geometry = geometry;
//
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
//             {
//                 var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());
//
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.PositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode();
//                         item.PositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie();
//                         item.Geometry = geometry;
//
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressHouseNumberWasReaddressed>>(async (context, message, ct) =>
//             {
//                 var geometry = WKBReaderFactory.CreateForLegacy()
//                     .Read(message.Message.ReaddressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray());
//
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = message.Message.ReaddressedHouseNumber.SourceStatus.Map();
//                         item.HouseNumber = message.Message.ReaddressedHouseNumber.DestinationHouseNumber;
//                         item.PostalCode = message.Message.ReaddressedHouseNumber.SourcePostalCode;
//                         item.OfficiallyAssigned = message.Message.ReaddressedHouseNumber.SourceIsOfficiallyAssigned;
//                         item.PositionMethod = message.Message.ReaddressedHouseNumber.SourceGeometryMethod.ToPositieGeometrieMethode();
//                         item.PositionSpecification = message.Message.ReaddressedHouseNumber.SourceGeometrySpecification.ToPositieSpecificatie();
//                         item.Geometry = geometry;
//
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//
//                 foreach (var readdressedBoxNumber in message.Message.ReaddressedBoxNumbers)
//                 {
//                     var boxNumberGeometry = WKBReaderFactory.CreateForLegacy()
//                         .Read(message.Message.ReaddressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray());
//
//                     await context.FindAndUpdateAddressLatestItem(
//                         readdressedBoxNumber.DestinationAddressPersistentLocalId,
//                         message.Position,
//                         item =>
//                         {
//                             item.Status = readdressedBoxNumber.SourceStatus.Map();
//                             item.HouseNumber = readdressedBoxNumber.DestinationHouseNumber;
//                             item.BoxNumber = readdressedBoxNumber.SourceBoxNumber;
//                             item.PostalCode = readdressedBoxNumber.SourcePostalCode;
//                             item.OfficiallyAssigned = readdressedBoxNumber.SourceIsOfficiallyAssigned;
//                             item.PositionMethod = readdressedBoxNumber.SourceGeometryMethod.ToPositieGeometrieMethode();
//                             item.PositionSpecification = readdressedBoxNumber.SourceGeometrySpecification.ToPositieSpecificatie();
//                             item.Geometry = boxNumberGeometry;
//
//                             UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                         },
//                         ct);
//                 }
//             });
//
//             When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (context, message, ct) =>
//             {
//                 var geometry = WKBReaderFactory.CreateForLegacy().Read(message.Message.ExtendedWkbGeometry.ToByteArray());
//                 var nisCode = await FindNisCode(geometry);
//
//                 var addressLatestItem = new AddressLatestItem
//                 {
//                     PersistentLocalId = message.Message.AddressPersistentLocalId,
//                     NisCode = nisCode,
//                     PostalCode = message.Message.PostalCode,
//                     StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
//                     Status = AddressStatus.Proposed.Map(),
//                     HouseNumber = message.Message.HouseNumber,
//                     BoxNumber = message.Message.BoxNumber,
//                     Geometry = geometry,
//                     PositionMethod = message.Message.GeometryMethod.ToPositieGeometrieMethode(),
//                     PositionSpecification = message.Message.GeometrySpecification.ToPositieSpecificatie(),
//                     OfficiallyAssigned = true,
//                     Removed = false,
//                     VersionTimestamp = message.Message.Provenance.Timestamp,
//                     Namespace = options.Value.Namespace,
//                     PuriId = $"{options.Value.Namespace}/{message.Message.AddressPersistentLocalId}",
//                     IdempotenceKey = message.Position
//                 };
//
//                 await context
//                     .AddressLatestItems
//                     .AddAsync(addressLatestItem, ct);
//             });
//
//             When<Envelope<AddressWasRejectedBecauseOfReaddress>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Rejected.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasRetiredBecauseOfReaddress>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Retired.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Removed = true;
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Removed = true;
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Removed = true;
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.Status = AddressStatus.Proposed.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressRegularizationWasCorrected>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.OfficiallyAssigned = false;
//                         item.Status = AddressStatus.Current.Map();
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             When<Envelope<AddressDeregulationWasCorrected>>(async (context, message, ct) =>
//             {
//                 await context.FindAndUpdateAddressLatestItem(
//                     message.Message.AddressPersistentLocalId,
//                     message.Position,
//                     item =>
//                     {
//                         item.OfficiallyAssigned = true;
//                         UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
//                     },
//                     ct);
//             });
//
//             async Task<string?> FindNisCode(Geometry geometry)
//             {
//                 string nisCode;
//                 await using var connection = new NpgsqlConnection(options.Value.MunicipalityGeometriesConnectionString);
//                 var sql = @"SELECT nis_code
//                                 FROM integration_municipality.municipality_geometries
//                                 WHERE ST_Within(
//                                     @Geometry,
//                                     integration_municipality.municipality_geometries.geometry
//                                 )
//                                 LIMIT 1;";
//
//                 nisCode = await connection.QuerySingleAsync(sql, new {Geometry = geometry});
//
//                 return nisCode;
//             }
//         }
//
//         private static void UpdateVersionTimestampIfNewer(AddressLatestItem addressLatestItem, Instant versionTimestamp)
//         {
//             if (versionTimestamp > addressLatestItem.VersionTimestamp)
//             {
//                 addressLatestItem.VersionTimestamp = versionTimestamp;
//             }
//         }
//
//         private static void UpdateVersionTimestamp(AddressLatestItem addressLatestItem, Instant versionTimestamp)
//             => addressLatestItem.VersionTimestamp = versionTimestamp;
//     }
// }
