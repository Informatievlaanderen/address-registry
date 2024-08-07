namespace AddressRegistry.Projections.Elastic.AddressSearch
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
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
    using Microsoft.EntityFrameworkCore;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;

    [ConnectedProjectionName("API endpoint search adressen")]
    [ConnectedProjectionDescription("Projectie die de adressen data in Elastic Search synchroniseert.")]
    public class AddressSearchProjections : ConnectedProjection<ElasticRunnerContext>
    {
        //TODO-rik hoelang gaan we dat in de cache houden?
        private readonly IDictionary<string, Municipality> _municipalities = new Dictionary<string, Municipality>();
        private readonly IDictionary<string, PostalInfo> _postalInfos = new Dictionary<string, PostalInfo>();
        private readonly IDictionary<int, StreetNameLatestItem> _streetNames = new Dictionary<int, StreetNameLatestItem>();

        private readonly IAddressElasticsearchClient _elasticsearchClient;
        private readonly IDbContextFactory<MunicipalityConsumerContext> _municipalityConsumerContextFactory;
        private readonly IDbContextFactory<PostalConsumerContext> _postalConsumerContextFactory;
        private readonly IDbContextFactory<StreetNameConsumerContext> _streetNameConsumerContextFactory;
        private readonly WKBReader _wkbReader;

        public AddressSearchProjections(
            IAddressElasticsearchClient elasticsearchClient,
            IDbContextFactory<MunicipalityConsumerContext> municipalityConsumerContextFactory,
            IDbContextFactory<PostalConsumerContext> postalConsumerContextFactory,
            IDbContextFactory<StreetNameConsumerContext> streetNameConsumerContextFactory)
        {
            _elasticsearchClient = elasticsearchClient;
            _municipalityConsumerContextFactory = municipalityConsumerContextFactory;
            _postalConsumerContextFactory = postalConsumerContextFactory;
            _streetNameConsumerContextFactory = streetNameConsumerContextFactory;

            _wkbReader = WKBReaderFactory.Create();

            #region StreetName

            When<Envelope<StreetNameNamesWereChanged>>(async (_, message, ct) =>
            {
                await UpdateStreetNameAndFullAddress(
                    message.Message.Provenance.Timestamp,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.AddressPersistentLocalIds.ToArray(),
                    ct);
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (_, message, ct) =>
            {
                await UpdateStreetNameAndFullAddress(
                    message.Message.Provenance.Timestamp,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.AddressPersistentLocalIds.ToArray(),
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (_, message, ct) =>
            {
                await UpdateStreetNameAndFullAddress(
                    message.Message.Provenance.Timestamp,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.AddressPersistentLocalIds.ToArray(),
                    ct);
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (_, message, ct) =>
            {
                await UpdateStreetNameAndFullAddress(
                    message.Message.Provenance.Timestamp,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.AddressPersistentLocalIds.ToArray(),
                    ct);
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

                await elasticsearchClient.CreateDocument(document, ct);
            });

            When<Envelope<AddressWasProposedV2>>(async (_, message, ct) =>
            {
                var streetName = await GetStreetName(message.Message.StreetNamePersistentLocalId, ct);

                var document = new AddressSearchDocument(
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

                await elasticsearchClient.CreateDocument(document, ct);
            });

            When<Envelope<AddressWasProposedForMunicipalityMerger>>(async (_, message, ct) =>
            {
                var streetName = await GetStreetName(message.Message.StreetNamePersistentLocalId, ct);

                var document = new AddressSearchDocument(
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

                await elasticsearchClient.CreateDocument(document, ct);
            });

            When<Envelope<AddressWasApproved>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Current
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Proposed
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Proposed
                    },
                    ct);
            });

            When<Envelope<AddressWasRejected>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Retired
                    },
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<AddressWasDeregulated>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        OfficiallyAssigned = false,
                        Status = AddressStatus.Current
                    },
                    ct);
            });

            When<Envelope<AddressWasRegularized>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        OfficiallyAssigned = true
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredV2>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Retired
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Retired
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Retired
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Retired
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Current
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        PostalCode = message.Message.PostalCode
                    },
                    ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await elasticsearchClient.UpdateDocument(
                        boxNumberPersistentLocalId,
                        new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                        {
                            PostalCode = message.Message.PostalCode
                        },
                        ct);
                }
            });

            When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        PostalCode = message.Message.PostalCode
                    },
                    ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await elasticsearchClient.UpdateDocument(
                        boxNumberPersistentLocalId,
                        new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                        {
                            PostalCode = message.Message.PostalCode
                        },
                        ct);
                }
            });

            When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        HouseNumber = message.Message.HouseNumber
                    },
                    ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await elasticsearchClient.UpdateDocument(
                        boxNumberPersistentLocalId,
                        new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                        {
                            HouseNumber = message.Message.HouseNumber
                        },
                        ct);
                }
            });

            When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        BoxNumber = message.Message.BoxNumber
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasChanged>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
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
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
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
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = message.Message.ReaddressedHouseNumber.SourceStatus,
                        HouseNumber = message.Message.ReaddressedHouseNumber.DestinationHouseNumber,
                        PostalCode = message.Message.ReaddressedHouseNumber.SourcePostalCode,
                        OfficiallyAssigned = message.Message.ReaddressedHouseNumber.SourceIsOfficiallyAssigned,
                        AddressPosition = AddressPosition(
                            message.Message.ReaddressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray(),
                            message.Message.ReaddressedHouseNumber.SourceGeometryMethod,
                            message.Message.ReaddressedHouseNumber.SourceGeometrySpecification)
                    },
                    ct);


                foreach (var readdressedBoxNumber in message.Message.ReaddressedBoxNumbers)
                {
                    await elasticsearchClient.UpdateDocument(
                        readdressedBoxNumber.DestinationAddressPersistentLocalId,
                        new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                        {
                            Status = readdressedBoxNumber.SourceStatus,
                            HouseNumber = readdressedBoxNumber.DestinationHouseNumber,
                            BoxNumber = readdressedBoxNumber.SourceBoxNumber,
                            PostalCode = readdressedBoxNumber.SourcePostalCode,
                            OfficiallyAssigned = readdressedBoxNumber.SourceIsOfficiallyAssigned,
                            AddressPosition = AddressPosition(
                                readdressedBoxNumber.SourceExtendedWkbGeometry.ToByteArray(),
                                readdressedBoxNumber.SourceGeometryMethod,
                                readdressedBoxNumber.SourceGeometrySpecification)
                        },
                        ct);
                }
            });

            When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (_, message, ct) =>
            {
                var streetName = await GetStreetName(message.Message.StreetNamePersistentLocalId, ct);

                var document = new AddressSearchDocument(
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

                await elasticsearchClient.CreateDocument(document, ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfReaddress>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Rejected
                    },
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfReaddress>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Retired
                    },
                    ct);
            });

            When<Envelope<AddressWasRemovedV2>>(async (_, message, ct) =>
            {
                await elasticsearchClient.DeleteDocument(
                    message.Message.AddressPersistentLocalId,
                    ct);
            });

            When<Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (_, message, ct) =>
            {
                await elasticsearchClient.DeleteDocument(
                    message.Message.AddressPersistentLocalId,
                    ct);
            });

            When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (_, message, ct) =>
            {
                await elasticsearchClient.DeleteDocument(
                    message.Message.AddressPersistentLocalId,
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        Status = AddressStatus.Proposed
                    },
                    ct);
            });

            When<Envelope<AddressRegularizationWasCorrected>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        OfficiallyAssigned = false,
                        Status = AddressStatus.Current
                    },
                    ct);
            });

            When<Envelope<AddressDeregulationWasCorrected>>(async (_, message, ct) =>
            {
                await elasticsearchClient.UpdateDocument(
                    message.Message.AddressPersistentLocalId,
                    new AddressSearchPartialDocument(message.Message.Provenance.Timestamp)
                    {
                        OfficiallyAssigned = true
                    },
                    ct);
            });

            When<Envelope<AddressRemovalWasCorrected>>(async (_, message, ct) =>
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

                await elasticsearchClient.CreateDocument(document, ct);
            });
        }

        private async Task UpdateStreetNameAndFullAddress(
            Instant versionTimestamp,
            int streetNamePersistentLocalId,
            ICollection<int> addressPersistentLocalIds,
            CancellationToken ct)
        {
            var streetNameLatestItem = await RefreshStreetNames(streetNamePersistentLocalId, ct);

            var documents = await _elasticsearchClient.GetDocuments(addressPersistentLocalIds, ct);

            foreach (var addressPersistentLocalId in addressPersistentLocalIds)
            {
                var document = documents.SingleOrDefault(x => x.AddressPersistentLocalId == addressPersistentLocalId);
                if (document is null)
                {
                    throw new NullReferenceException($"No document received for {addressPersistentLocalId}");
                }

                var desiredVersionTimestamp = versionTimestamp.ToBelgianDateTimeOffset() > document.VersionTimestamp
                    ? versionTimestamp.ToBelgianDateTimeOffset()
                    : document.VersionTimestamp;
                var streetName = StreetName.FromStreetNameLatestItem(streetNameLatestItem);

                await _elasticsearchClient.UpdateDocument(
                    addressPersistentLocalId,
                    new AddressSearchPartialDocument(desiredVersionTimestamp)
                    {
                        StreetName = streetName,
                        FullAddress = AddressSearchDocument.BuildFullAddress(
                            document.Municipality,
                            document.PostalInfo,
                            streetName,
                            document.HouseNumber,
                            document.BoxNumber)
                    }, ct);
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
                .Include(x => x.PostalNames)
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
