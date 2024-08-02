namespace AddressRegistry.Projections.Elastic.AddressSearch
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Consumer.Read.Municipality;
    using Consumer.Read.Postal;
    using Consumer.Read.StreetName;
    using Consumer.Read.StreetName.Projections;
    using global::Elastic.Clients.Elasticsearch;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;

    [ConnectedProjectionName("API endpoint search adressen")]
    [ConnectedProjectionDescription("Projectie die de adressen data in Elastic Search synchroniseert.")]
    public class AddressSearchProjections : ConnectedProjection<ElasticRunnerContext>
    {
        private readonly IDictionary<string, Municipality> _municipalities = new Dictionary<string, Municipality>();
        private readonly IDictionary<string, PostalInfo> _postalInfos = new Dictionary<string, PostalInfo>();
        private readonly IDictionary<int, StreetNameLatestItem> _streetNames = new Dictionary<int, StreetNameLatestItem>();

        private readonly IDbContextFactory<MunicipalityConsumerContext> _municipalityConsumerContextFactory;
        private readonly IDbContextFactory<PostalConsumerContext> _postalConsumerContextFactory;
        private readonly IDbContextFactory<StreetNameConsumerContext> _streetNameConsumerContextFactory;
        private readonly WKBReader _wkbReader;

        public AddressSearchProjections(
            ElasticsearchClient elasticClient,
            IndexName indexName,
            IDbContextFactory<MunicipalityConsumerContext> municipalityConsumerContextFactory,
            IDbContextFactory<PostalConsumerContext> postalConsumerContextFactory,
            IDbContextFactory<StreetNameConsumerContext> streetNameConsumerContextFactory)
        {
            _municipalityConsumerContextFactory = municipalityConsumerContextFactory;
            _postalConsumerContextFactory = postalConsumerContextFactory;
            _streetNameConsumerContextFactory = streetNameConsumerContextFactory;

            _wkbReader = WKBReaderFactory.Create();

            #region StreetName

            When<Envelope<StreetNameNamesWereChanged>>(async (_, message, ct) =>
            {
                await RefreshStreetNames(message.Message.StreetNamePersistentLocalId, ct);

                // todo-rik update streetnames + full address

                // foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                // {
                //     var item = await context.FindAndUpdateAddressDetailV2(
                //         addressPersistentLocalId,
                //         item =>
                //         {
                //             UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
                //         },
                //         ct);
                //
                //     UpdateHash(item, message);
                // }
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (_, message, ct) =>
            {
                await RefreshStreetNames(message.Message.StreetNamePersistentLocalId, ct);

                // todo-rik update streetnames + full address

                // foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                // {
                //     var item = await context.FindAndUpdateAddressDetailV2(
                //         addressPersistentLocalId,
                //         item =>
                //         {
                //             UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
                //         },
                //         ct);
                //
                //     UpdateHash(item, message);
                // }
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (_, message, ct) =>
            {
                await RefreshStreetNames(message.Message.StreetNamePersistentLocalId, ct);

                // todo-rik update streetnames

                // foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                // {
                //     var item = await context.FindAndUpdateAddressDetailV2(
                //         addressPersistentLocalId,
                //         item =>
                //         {
                //             UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
                //         },
                //         ct);
                //
                //     UpdateHash(item, message);
                // }
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (_, message, ct) =>
            {
                await RefreshStreetNames(message.Message.StreetNamePersistentLocalId, ct);

                // todo-rik update streetnames

                // foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                // {
                //     var item = await context.FindAndUpdateAddressDetailV2(
                //         addressPersistentLocalId,
                //         item =>
                //         {
                //             UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
                //         },
                //         ct);
                //
                //     UpdateHash(item, message);
                // }
            });

            #endregion StreetName

            // Address
            When<Envelope<AddressWasMigratedToStreetName>>(async (_, message, ct) =>
            {
                var streetName = await GetStreetName(message.Message.StreetNamePersistentLocalId, ct);

                var document = new AddressSearchDocument(
                    message.Message.AddressPersistentLocalId,
                    message.Message.ParentPersistentLocalId,
                    message.Message.Provenance.Timestamp,
                    message.Message.Status,
                    message.Message.OfficiallyAssigned,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    await GetMunicipality(streetName.NisCode!, ct),
                    await GetPostalInfo(message.Message.PostalCode, ct),
                    StreetName.FromStreetNameLatestItem(streetName),
                    AddressPosition(
                        message.Message.ExtendedWkbGeometry.ToByteArray(),
                        message.Message.GeometryMethod,
                        message.Message.GeometrySpecification));

                await elasticClient.CreateAsync(
                    new CreateRequest<AddressSearchDocument>(document, indexName, Id.From(document.AddressPersistentLocalId)), ct);
            });

            //     When<Envelope<AddressWasProposedV2>>(async (_, message, ct) =>
            //     {
            //         var addressDetailItemV2 = new AddressDetailItemV2WithParent(
            //             message.Message.AddressPersistentLocalId,
            //             message.Message.StreetNamePersistentLocalId,
            //             message.Message.ParentPersistentLocalId,
            //             message.Message.PostalCode,
            //             message.Message.HouseNumber,
            //             message.Message.BoxNumber,
            //             AddressStatus.Proposed,
            //             officiallyAssigned: true,
            //             position: message.Message.ExtendedWkbGeometry.ToByteArray(),
            //             positionMethod: message.Message.GeometryMethod,
            //             positionSpecification: message.Message.GeometrySpecification,
            //             removed: false,
            //             message.Message.Provenance.Timestamp);
            //
            //         UpdateHash(addressDetailItemV2, message);
            //
            //         await context
            //             .AddressDetailV2WithParent
            //             .AddAsync(addressDetailItemV2, ct);
            //     });
            //
            //     When<Envelope<AddressWasProposedForMunicipalityMerger>>(async (_, message, ct) =>
            //     {
            //         var addressDetailItemV2 = new AddressDetailItemV2WithParent(
            //             message.Message.AddressPersistentLocalId,
            //             message.Message.StreetNamePersistentLocalId,
            //             message.Message.ParentPersistentLocalId,
            //             message.Message.PostalCode,
            //             message.Message.HouseNumber,
            //             message.Message.BoxNumber,
            //             AddressStatus.Proposed,
            //             officiallyAssigned: message.Message.OfficiallyAssigned,
            //             position: message.Message.ExtendedWkbGeometry.ToByteArray(),
            //             positionMethod: message.Message.GeometryMethod,
            //             positionSpecification: message.Message.GeometrySpecification,
            //             removed: false,
            //             message.Message.Provenance.Timestamp);
            //
            //         UpdateHash(addressDetailItemV2, message);
            //
            //         await context
            //             .AddressDetailV2WithParent
            //             .AddAsync(addressDetailItemV2, ct);
            //     });
            //
            //     When<Envelope<AddressWasApproved>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Current;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Proposed;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Proposed;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRejected>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Rejected;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Rejected;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Rejected;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Rejected;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Rejected;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Retired;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Rejected;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasDeregulated>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.OfficiallyAssigned = false;
            //                 item.Status = AddressStatus.Current;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRegularized>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.OfficiallyAssigned = true;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRetiredV2>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Retired;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Retired;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Retired;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Retired;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Current;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressPostalCodeWasChangedV2>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.PostalCode = message.Message.PostalCode;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //
            //         foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
            //         {
            //             var boxNumberItem = await context.FindAndUpdateAddressDetailV2(
            //                 boxNumberPersistentLocalId,
            //                 boxNumberItem =>
            //                 {
            //                     boxNumberItem.PostalCode = message.Message.PostalCode;
            //                     UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
            //                 },
            //                 ct);
            //
            //             UpdateHash(boxNumberItem, message);
            //         }
            //     });
            //
            //     When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.PostalCode = message.Message.PostalCode;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //
            //         foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
            //         {
            //             var boxNumberItem = await context.FindAndUpdateAddressDetailV2(
            //                 boxNumberPersistentLocalId,
            //                 boxNumberItem =>
            //                 {
            //                     boxNumberItem.PostalCode = message.Message.PostalCode;
            //                     UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
            //                 },
            //                 ct);
            //
            //             UpdateHash(boxNumberItem, message);
            //         }
            //     });
            //
            //     When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.HouseNumber = message.Message.HouseNumber;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //
            //         foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
            //         {
            //             var boxNumberItem = await context.FindAndUpdateAddressDetailV2(
            //                 boxNumberPersistentLocalId,
            //                 boxNumberItem =>
            //                 {
            //                     boxNumberItem.HouseNumber = message.Message.HouseNumber;
            //                     UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
            //                 },
            //                 ct);
            //
            //             UpdateHash(boxNumberItem, message);
            //         }
            //     });
            //
            //     When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.BoxNumber = message.Message.BoxNumber;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressPositionWasChanged>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.PositionMethod = message.Message.GeometryMethod;
            //                 item.PositionSpecification = message.Message.GeometrySpecification;
            //                 item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
            //
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressPositionWasCorrectedV2>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.PositionMethod = message.Message.GeometryMethod;
            //                 item.PositionSpecification = message.Message.GeometrySpecification;
            //                 item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
            //
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressHouseNumberWasReaddressed>>(async (_, message, ct) =>
            //     {
            //         var houseNumberItem = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = message.Message.ReaddressedHouseNumber.SourceStatus;
            //                 item.HouseNumber = message.Message.ReaddressedHouseNumber.DestinationHouseNumber;
            //                 item.PostalCode = message.Message.ReaddressedHouseNumber.SourcePostalCode;
            //                 item.OfficiallyAssigned = message.Message.ReaddressedHouseNumber.SourceIsOfficiallyAssigned;
            //                 item.PositionMethod = message.Message.ReaddressedHouseNumber.SourceGeometryMethod;
            //                 item.PositionSpecification = message.Message.ReaddressedHouseNumber.SourceGeometrySpecification;
            //                 item.Position = message.Message.ReaddressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray();
            //
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(houseNumberItem, message);
            //
            //         foreach (var readdressedBoxNumber in message.Message.ReaddressedBoxNumbers)
            //         {
            //             var boxNumberItem = await context.FindAndUpdateAddressDetailV2(
            //                 readdressedBoxNumber.DestinationAddressPersistentLocalId,
            //                 item =>
            //                 {
            //                     item.Status = readdressedBoxNumber.SourceStatus;
            //                     item.HouseNumber = readdressedBoxNumber.DestinationHouseNumber;
            //                     item.BoxNumber = readdressedBoxNumber.SourceBoxNumber;
            //                     item.PostalCode = readdressedBoxNumber.SourcePostalCode;
            //                     item.OfficiallyAssigned = readdressedBoxNumber.SourceIsOfficiallyAssigned;
            //                     item.PositionMethod = readdressedBoxNumber.SourceGeometryMethod;
            //                     item.PositionSpecification = readdressedBoxNumber.SourceGeometrySpecification;
            //                     item.Position = readdressedBoxNumber.SourceExtendedWkbGeometry.ToByteArray();
            //
            //                     UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //                 },
            //                 ct);
            //
            //             UpdateHash(boxNumberItem, message);
            //         }
            //     });
            //
            //     When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (_, message, ct) =>
            //     {
            //         var addressDetailItemV2 = new AddressDetailItemV2WithParent(
            //             message.Message.AddressPersistentLocalId,
            //             message.Message.StreetNamePersistentLocalId,
            //             message.Message.ParentPersistentLocalId,
            //             message.Message.PostalCode,
            //             message.Message.HouseNumber,
            //             message.Message.BoxNumber,
            //             AddressStatus.Proposed,
            //             officiallyAssigned: true,
            //             position: message.Message.ExtendedWkbGeometry.ToByteArray(),
            //             positionMethod: message.Message.GeometryMethod,
            //             positionSpecification: message.Message.GeometrySpecification,
            //             removed: false,
            //             message.Message.Provenance.Timestamp);
            //
            //         UpdateHash(addressDetailItemV2, message);
            //
            //         await context
            //             .AddressDetailV2WithParent
            //             .AddAsync(addressDetailItemV2, ct);
            //     });
            //
            //     When<Envelope<AddressWasRejectedBecauseOfReaddress>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Rejected;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRetiredBecauseOfReaddress>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Retired;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRemovedV2>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Removed = true;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Removed = true;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Removed = true;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status = AddressStatus.Proposed;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressRegularizationWasCorrected>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.OfficiallyAssigned = false;
            //                 item.Status = AddressStatus.Current;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressDeregulationWasCorrected>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.OfficiallyAssigned = true;
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
            //
            //     When<Envelope<AddressRemovalWasCorrected>>(async (_, message, ct) =>
            //     {
            //         var item = await context.FindAndUpdateAddressDetailV2(
            //             message.Message.AddressPersistentLocalId,
            //             item =>
            //             {
            //                 item.Status =  message.Message.Status;
            //                 item.PostalCode = message.Message.PostalCode;
            //                 item.HouseNumber = message.Message.HouseNumber;
            //                 item.BoxNumber = message.Message.BoxNumber;
            //                 item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
            //                 item.PositionMethod = message.Message.GeometryMethod;
            //                 item.PositionSpecification = message.Message.GeometrySpecification;
            //                 item.OfficiallyAssigned = message.Message.OfficiallyAssigned;
            //                 item.Removed = false;
            //
            //                 UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
            //             },
            //             ct);
            //
            //         UpdateHash(item, message);
            //     });
        }

        private AddressPosition AddressPosition(
            byte[] extendedWkbAsBytes,
            GeometryMethod geometryMethod,
            GeometrySpecification geometrySpecification)
        {
            return new AddressPosition(
                (_wkbReader.Read(extendedWkbAsBytes) as Point)!,
                geometryMethod,
                geometrySpecification);
        }

        private static void UpdateVersionTimestamp(AddressSearchDocument address, Instant versionTimestamp)
            => address.VersionTimestamp = versionTimestamp.ToBelgianDateTimeOffset();

        private static void UpdateVersionTimestampIfNewer(AddressSearchDocument address, Instant versionTimestamp)
        {
            if (versionTimestamp.ToBelgianDateTimeOffset() > address.VersionTimestamp)
            {
                address.VersionTimestamp = versionTimestamp.ToBelgianDateTimeOffset();
            }
        }

        private async Task<Municipality> GetMunicipality(string nisCode, CancellationToken ct)
        {
            if (_municipalities.TryGetValue(nisCode, out var value))
                return value;

            await using var context = await _municipalityConsumerContextFactory.CreateDbContextAsync(ct);
            var municipality = await context.MunicipalityLatestItems
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.NisCode == nisCode, ct);

            if (municipality == null)
                throw new InvalidOperationException($"Municipality with NisCode {nisCode} not found");

            _municipalities.Add(nisCode, Municipality.FromMunicipalityLatestItem(municipality));

            return _municipalities[nisCode];
        }

        private async Task<PostalInfo?> GetPostalInfo(string? postalCode, CancellationToken ct)
        {
            if (postalCode is null)
                return null;

            if (_postalInfos.TryGetValue(postalCode, out var value))
                return value;

            await using var context = await _postalConsumerContextFactory.CreateDbContextAsync(ct);
            var postalInfo = await context.PostalLatestItems
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PostalCode == postalCode, ct);

            if (postalInfo == null)
                throw new InvalidOperationException($"PostalInfo with postalCode {postalCode} not found");

            _postalInfos.Add(postalCode, PostalInfo.FromPostalLatestItem(postalInfo));

            return _postalInfos[postalCode];
        }

        private async Task<StreetNameLatestItem> GetStreetName(int streetNamePersistentLocalId, CancellationToken ct)
        {
            if (_streetNames.TryGetValue(streetNamePersistentLocalId, out var value))
                return value;

            await using var context = await _streetNameConsumerContextFactory.CreateDbContextAsync(ct);
            var streetName = await context.StreetNameLatestItems
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.PersistentLocalId == streetNamePersistentLocalId, ct);

            if (streetName == null)
                throw new InvalidOperationException($"StreetName with id {streetNamePersistentLocalId} not found");

            _streetNames.Add(streetNamePersistentLocalId, streetName);

            return streetName;
        }

        private async Task<StreetNameLatestItem> RefreshStreetNames(int streetNamePersistentLocalId, CancellationToken ct)
        {
            await using var context = await _streetNameConsumerContextFactory.CreateDbContextAsync(ct);
            var streetName = await context.StreetNameLatestItems
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.PersistentLocalId == streetNamePersistentLocalId, ct);

            if (streetName == null)
                throw new InvalidOperationException($"StreetName with id {streetNamePersistentLocalId} not found");

            _streetNames[streetNamePersistentLocalId] = streetName;

            return streetName;
        }
    }
}
