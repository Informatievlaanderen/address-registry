namespace AddressRegistry.Projections.Elastic.AddressList
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.StreetName;
    using AddressRegistry.StreetName.Events;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Consumer.Read.Municipality;
    using Consumer.Read.Postal;
    using Consumer.Read.StreetName;
    using Consumer.Read.StreetName.Projections;
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;

    [ConnectedProjectionName("API endpoint lijst adressen (elatic)")]
    [ConnectedProjectionDescription("Projectie die de data voor het adressenlijst endpoint in Elastic Search synchroniseert.")]
    public class AddressListProjections : ConnectedProjection<ElasticRunnerContext>
    {
        private readonly IDictionary<string, Municipality> _municipalities = new Dictionary<string, Municipality>();
        private readonly IDictionary<string, PostalInfo> _postalInfos = new Dictionary<string, PostalInfo>();
        private readonly IDictionary<int, StreetNameLatestItem> _streetNames = new Dictionary<int, StreetNameLatestItem>();

        private readonly IAddressListElasticClient _searchElasticClient;
        private readonly IDbContextFactory<MunicipalityConsumerContext> _municipalityConsumerContextFactory;
        private readonly IDbContextFactory<PostalConsumerContext> _postalConsumerContextFactory;
        private readonly IDbContextFactory<StreetNameConsumerContext> _streetNameConsumerContextFactory;
        private readonly WKBReader _wkbReader;

        public AddressListProjections(
            IAddressListElasticClient searchElasticClient,
            IDbContextFactory<MunicipalityConsumerContext> municipalityConsumerContextFactory,
            IDbContextFactory<PostalConsumerContext> postalConsumerContextFactory,
            IDbContextFactory<StreetNameConsumerContext> streetNameConsumerContextFactory)
        {
            _searchElasticClient = searchElasticClient;
            _municipalityConsumerContextFactory = municipalityConsumerContextFactory;
            _postalConsumerContextFactory = postalConsumerContextFactory;
            _streetNameConsumerContextFactory = streetNameConsumerContextFactory;

            _wkbReader = WKBReaderFactory.Create();

            #region StreetName

            When<Envelope<StreetNameNamesWereChanged>>(async (_, message, ct) =>
            {
                await UpdateStreetNameAndFullAddress(message.Message.StreetNamePersistentLocalId,
                    message.Message.AddressPersistentLocalIds.ToArray(),
                    message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (_, message, ct) =>
            {
                await UpdateStreetNameAndFullAddress(message.Message.StreetNamePersistentLocalId,
                    message.Message.AddressPersistentLocalIds.ToArray(),
                    message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (_, message, ct) =>
            {
                await UpdateStreetNameAndFullAddress(message.Message.StreetNamePersistentLocalId,
                    message.Message.AddressPersistentLocalIds.ToArray(),
                    message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (_, message, ct) =>
            {
                await UpdateStreetNameAndFullAddress(message.Message.StreetNamePersistentLocalId,
                    message.Message.AddressPersistentLocalIds.ToArray(),
                    message.Message.Provenance.Timestamp, ct);
            });

            When<Envelope<MigratedStreetNameWasImported>>(DoNothing);
            When<Envelope<StreetNameWasImported>>(DoNothing);
            When<Envelope<StreetNameWasApproved>>(DoNothing);
            When<Envelope<StreetNameWasCorrectedFromApprovedToProposed>>(DoNothing);
            When<Envelope<StreetNameWasCorrectedFromRetiredToCurrent>>(DoNothing);
            When<Envelope<StreetNameWasCorrectedFromRejectedToProposed>>(DoNothing);
            When<Envelope<StreetNameWasRejected>>(DoNothing);
            When<Envelope<StreetNameWasRejectedBecauseOfMunicipalityMerger>>(DoNothing);
            When<Envelope<StreetNameWasRetired>>(DoNothing);
            When<Envelope<StreetNameWasRetiredBecauseOfMunicipalityMerger>>(DoNothing);
            When<Envelope<StreetNameWasRemoved>>(DoNothing);
            When<Envelope<StreetNameWasReaddressed>>(DoNothing);
            When<Envelope<StreetNameWasRenamed>>(DoNothing);

            #endregion StreetName

            // Address
            When<Envelope<AddressWasMigratedToStreetName>>(async (_, message, ct) =>
            {
                var streetName = await GetStreetName(message.Message.StreetNamePersistentLocalId, ct);

                var document = new AddressListDocument(
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

                await searchElasticClient.CreateDocument(document, ct);
            });

            When<Envelope<AddressWasProposedV2>>(async (_, message, ct) =>
            {
                var streetName = await GetStreetName(message.Message.StreetNamePersistentLocalId, ct);

                var document = new AddressListDocument(
                    message.Message.AddressPersistentLocalId,
                    message.Message.ParentPersistentLocalId,
                    message.Message.Provenance.Timestamp,
                    AddressStatus.Proposed,
                    true,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    await GetMunicipality(streetName.NisCode!, ct),
                    await GetPostalInfo(message.Message.PostalCode, ct),
                    StreetName.FromStreetNameLatestItem(streetName),
                    AddressPosition(
                        message.Message.ExtendedWkbGeometry.ToByteArray(),
                        message.Message.GeometryMethod,
                        message.Message.GeometrySpecification));

                await searchElasticClient.CreateDocument(document, ct);
            });

            When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (_, message, ct) =>
            {
                var streetName = await GetStreetName(message.Message.StreetNamePersistentLocalId, ct);

                var document = new AddressListDocument(
                    message.Message.AddressPersistentLocalId,
                    message.Message.ParentPersistentLocalId,
                    message.Message.Provenance.Timestamp,
                    AddressStatus.Proposed,
                    true,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    await GetMunicipality(streetName.NisCode!, ct),
                    await GetPostalInfo(message.Message.PostalCode, ct),
                    StreetName.FromStreetNameLatestItem(streetName),
                    AddressPosition(
                        message.Message.ExtendedWkbGeometry.ToByteArray(),
                        message.Message.GeometryMethod,
                        message.Message.GeometrySpecification));

                await searchElasticClient.CreateDocument(document, ct);
            });

            When<Envelope<AddressWasProposedForMunicipalityMerger>>(async (_, message, ct) =>
            {
                var streetName = await GetStreetName(message.Message.StreetNamePersistentLocalId, ct);

                var document = new AddressListDocument(
                    message.Message.AddressPersistentLocalId,
                    message.Message.ParentPersistentLocalId,
                    message.Message.Provenance.Timestamp,
                    AddressStatus.Proposed,
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

                await searchElasticClient.CreateDocument(document, ct);
            });

            When<Envelope<AddressWasApproved>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Current
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Proposed
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Proposed
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Proposed
                    },
                    ct);
            });

            When<Envelope<AddressWasRejected>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfReaddress>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredV2>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Retired
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Retired
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Retired
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Retired
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfReaddress>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Retired
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Retired
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Current
                    },
                    ct);
            });

            When<Envelope<AddressWasDeregulated>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        OfficiallyAssigned = false,
                        Status = AddressStatus.Current
                    },
                    ct);
            });

            When<Envelope<AddressDeregulationWasCorrected>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        OfficiallyAssigned = true
                    },
                    ct);
            });

            When<Envelope<AddressWasRegularized>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        OfficiallyAssigned = true
                    },
                    ct);
            });

            When<Envelope<AddressRegularizationWasCorrected>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        OfficiallyAssigned = false,
                        Status = AddressStatus.Current
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (_, message, ct) =>
            {
                var postalInfo = await GetPostalInfo(message.Message.PostalCode, ct);

                var addressPersistentLocalIds =
                    new[] { message.Message.AddressPersistentLocalId }.Concat(message.Message.BoxNumberPersistentLocalIds).ToArray();

                await UpdateDocuments(
                    addressPersistentLocalIds,
                    doc => doc.PostalInfo = postalInfo,
                    message.Message.Provenance.Timestamp,
                    ct);
            });

            When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (_, message, ct) =>
            {
                var postalInfo = await GetPostalInfo(message.Message.PostalCode, ct);

                var addressPersistentLocalIds =
                    new[] { message.Message.AddressPersistentLocalId }.Concat(message.Message.BoxNumberPersistentLocalIds).ToArray();

                await UpdateDocuments(
                    addressPersistentLocalIds,
                    doc => doc.PostalInfo = postalInfo,
                    message.Message.Provenance.Timestamp,
                    ct);
            });

            When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (_, message, ct) =>
            {
                var addressPersistentLocalIds =
                    new[] { message.Message.AddressPersistentLocalId }.Concat(message.Message.BoxNumberPersistentLocalIds).ToArray();

                await UpdateDocuments(
                    addressPersistentLocalIds,
                    doc => doc.HouseNumber = message.Message.HouseNumber,
                    message.Message.Provenance.Timestamp,
                    ct);
            });

            When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (_, message, ct) =>
            {
                await UpdateDocuments(
                    [message.Message.AddressPersistentLocalId],
                    doc => doc.BoxNumber = message.Message.BoxNumber,
                    message.Message.Provenance.Timestamp,
                    ct);
            });

            When<Envelope<AddressPositionWasChanged>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        AddressPosition = AddressPosition(
                            message.Message.ExtendedWkbGeometry.ToByteArray(),
                            message.Message.GeometryMethod,
                            message.Message.GeometrySpecification)
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (_, message, ct) =>
            {
                await searchElasticClient.PartialUpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressListPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        AddressPosition = AddressPosition(
                            message.Message.ExtendedWkbGeometry.ToByteArray(),
                            message.Message.GeometryMethod,
                            message.Message.GeometrySpecification)
                    },
                    ct);
            });

            When<Envelope<AddressHouseNumberWasReaddressed>>(async (_, message, ct) =>
            {
                var houseNumberPostalInfo = await GetPostalInfo(message.Message.ReaddressedHouseNumber.SourcePostalCode, ct);
                await UpdateDocuments(
                    [message.Message.AddressPersistentLocalId],
                    doc =>
                    {
                        doc.Status = message.Message.ReaddressedHouseNumber.SourceStatus;
                        doc.HouseNumber = message.Message.ReaddressedHouseNumber.DestinationHouseNumber;
                        doc.PostalInfo = houseNumberPostalInfo;
                        doc.OfficiallyAssigned = message.Message.ReaddressedHouseNumber.SourceIsOfficiallyAssigned;
                        doc.AddressPosition = AddressPosition(
                            message.Message.ReaddressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray(),
                            message.Message.ReaddressedHouseNumber.SourceGeometryMethod,
                            message.Message.ReaddressedHouseNumber.SourceGeometrySpecification);
                    },
                    message.Message.Provenance.Timestamp,
                    ct);

                foreach (var boxNumber in message.Message.ReaddressedBoxNumbers)
                {
                    var boxNumberPostalInfo = await GetPostalInfo(boxNumber.SourcePostalCode, ct);
                    await UpdateDocuments(
                        [boxNumber.DestinationAddressPersistentLocalId],
                        doc =>
                        {
                            doc.Status = boxNumber.SourceStatus;
                            doc.HouseNumber = boxNumber.DestinationHouseNumber;
                            doc.BoxNumber = boxNumber.SourceBoxNumber;
                            doc.PostalInfo = boxNumberPostalInfo;
                            doc.OfficiallyAssigned = boxNumber.SourceIsOfficiallyAssigned;
                            doc.AddressPosition = AddressPosition(
                                boxNumber.SourceExtendedWkbGeometry.ToByteArray(),
                                boxNumber.SourceGeometryMethod,
                                boxNumber.SourceGeometrySpecification);
                        },
                        message.Message.Provenance.Timestamp,
                        ct);
                }
            });

            When<Envelope<AddressWasRemovedV2>>(async (_, message, ct) =>
            {
                await searchElasticClient.DeleteDocument(
                    message.Message.AddressPersistentLocalId,
                    ct);
            });

            When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (_, message, ct) =>
            {
                await searchElasticClient.DeleteDocument(
                    message.Message.AddressPersistentLocalId,
                    ct);
            });

            When<Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (_, message, ct) =>
            {
                await searchElasticClient.DeleteDocument(
                    message.Message.AddressPersistentLocalId,
                    ct);
            });

            When<Envelope<AddressRemovalWasCorrected>>(async (_, message, ct) =>
            {
                var streetName = await GetStreetName(message.Message.StreetNamePersistentLocalId, ct);

                var document = new AddressListDocument(
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

                await searchElasticClient.CreateDocument(document, ct);
            });
        }

        private async Task UpdateDocuments(
            ICollection<int> addressPersistentLocalIds,
            Action<AddressListDocument> update,
            Instant versionTimestamp,
            CancellationToken ct)
        {
            var documents = await _searchElasticClient.GetDocuments(addressPersistentLocalIds, ct);

            foreach (var addressPersistentLocalId in addressPersistentLocalIds)
            {
                var document = documents.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);
                if (document is null)
                {
                    throw new InvalidOperationException($"No document received for {addressPersistentLocalId}");
                }

                update(document);

                document.VersionTimestamp = versionTimestamp.ToBelgianDateTimeOffset();

                await _searchElasticClient.UpdateDocument(document, ct);
            }
        }

        private async Task UpdateStreetNameAndFullAddress(
            int streetNamePersistentLocalId,
            ICollection<int> addressPersistentLocalIds,
            Instant versionTimestamp,
            CancellationToken ct)
        {
            var streetNameLatestItem = await RefreshStreetNames(streetNamePersistentLocalId, ct);

            var documents = await _searchElasticClient.GetDocuments(addressPersistentLocalIds, ct);

            foreach (var addressPersistentLocalId in addressPersistentLocalIds)
            {
                var document = documents.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);
                if (document is null)
                {
                    throw new InvalidOperationException($"No document received for {addressPersistentLocalId}");
                }

                var desiredVersionTimestamp = versionTimestamp.ToBelgianDateTimeOffset() > document.VersionTimestamp
                    ? versionTimestamp.ToBelgianDateTimeOffset()
                    : document.VersionTimestamp;
                var streetName = StreetName.FromStreetNameLatestItem(streetNameLatestItem);

                document.VersionTimestamp = desiredVersionTimestamp;
                document.StreetName = streetName;

                await _searchElasticClient.UpdateDocument(document, ct);
            }
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

        private async Task<Municipality> GetMunicipality(string nisCode, CancellationToken ct)
        {
            if (_municipalities.TryGetValue(nisCode, out var value))
                return value;

            await using var context = await _municipalityConsumerContextFactory.CreateDbContextAsync(ct);
            var municipalityLatestItem = await context.MunicipalityLatestItems
                .AsNoTracking()
                .FirstOrDefaultAsync(m => m.NisCode == nisCode, ct);

            if (municipalityLatestItem == null)
                throw new InvalidOperationException($"Municipality with NisCode {nisCode} not found");

            _municipalities.Add(nisCode, Municipality.FromMunicipalityLatestItem(municipalityLatestItem));

            return _municipalities[nisCode];
        }

        private async Task<PostalInfo?> GetPostalInfo(string? postalCode, CancellationToken ct)
        {
            if (postalCode is null)
                return null;

            if (_postalInfos.TryGetValue(postalCode, out var value))
                return value;

            await using var context = await _postalConsumerContextFactory.CreateDbContextAsync(ct);
            var postalInfoLatestItem = await context.PostalLatestItems
                .Include(x => x.PostalNames)
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.PostalCode == postalCode, ct);

            if (postalInfoLatestItem == null)
                throw new InvalidOperationException($"PostalInfo with postalCode {postalCode} not found");

            _postalInfos.Add(postalCode, PostalInfo.FromPostalLatestItem(postalInfoLatestItem));

            return _postalInfos[postalCode];
        }

        private async Task<StreetNameLatestItem> GetStreetName(int streetNamePersistentLocalId, CancellationToken ct)
        {
            if (_streetNames.TryGetValue(streetNamePersistentLocalId, out var value))
                return value;

            await using var context = await _streetNameConsumerContextFactory.CreateDbContextAsync(ct);
            var streetNameLatestItem = await context.StreetNameLatestItems
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.PersistentLocalId == streetNamePersistentLocalId, ct);

            if (streetNameLatestItem == null)
                throw new InvalidOperationException($"StreetName with id {streetNamePersistentLocalId} not found");

            _streetNames.Add(streetNamePersistentLocalId, streetNameLatestItem);

            return streetNameLatestItem;
        }

        private async Task<StreetNameLatestItem> RefreshStreetNames(int streetNamePersistentLocalId, CancellationToken ct)
        {
            await using var context = await _streetNameConsumerContextFactory.CreateDbContextAsync(ct);
            var streetNameLatestItem = await context.StreetNameLatestItems
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.PersistentLocalId == streetNamePersistentLocalId, ct);

            _streetNames[streetNamePersistentLocalId] =
                streetNameLatestItem ?? throw new InvalidOperationException($"StreetName with id {streetNamePersistentLocalId} not found");

            return streetNameLatestItem;
        }

        private static Task DoNothing<T>(ElasticRunnerContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
