namespace AddressRegistry.Producer.Ldes
{
    using System;
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka;
    using Be.Vlaanderen.Basisregisters.MessageHandling.Kafka.Producer;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Consumer.Read.StreetName;
    using Microsoft.EntityFrameworkCore;
    using Newtonsoft.Json;
    using NodaTime;
    using StreetName;
    using StreetName.Events;

    [ConnectedProjectionName("Kafka producer ldes")]
    [ConnectedProjectionDescription("Projectie die berichten naar de kafka broker stuurt.")]
    public sealed class ProducerProjections : ConnectedProjection<ProducerContext>
    {
        public const string TopicKey = "AddressTopic";

        private readonly IProducer _producer;
        private readonly string _osloNamespace;
        private readonly JsonSerializerSettings _jsonSerializerSettings;
        private readonly IDbContextFactory<StreetNameConsumerContext> _streetNameConsumerContextFactory;

        public ProducerProjections(
            IProducer producer,
            string osloNamespace,
            JsonSerializerSettings jsonSerializerSettings,
            IDbContextFactory<StreetNameConsumerContext> streetNameConsumerContextFactory)
        {
            _producer = producer;
            _osloNamespace = osloNamespace;
            _jsonSerializerSettings = jsonSerializerSettings;
            _streetNameConsumerContextFactory = streetNameConsumerContextFactory;

            #region StreetName

            When<Envelope<StreetNameNamesWereChanged>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetail(
                        addressPersistentLocalId,
                        item =>
                        {
                            UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    await Produce(context, addressPersistentLocalId, message.Position, ct);
                }
            });

            When<Envelope<StreetNameNamesWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetail(
                        addressPersistentLocalId,
                        item =>
                        {
                            UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    await Produce(context, addressPersistentLocalId, message.Position, ct);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetail(
                        addressPersistentLocalId,
                        item =>
                        {
                            UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    await Produce(context, addressPersistentLocalId, message.Position, ct);
                }
            });

            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(async (context, message, ct) =>
            {
                foreach (var addressPersistentLocalId in message.Message.AddressPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetail(
                        addressPersistentLocalId,
                        item =>
                        {
                            UpdateVersionTimestampIfNewer(item, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    await Produce(context, addressPersistentLocalId, message.Position, ct);
                }
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
            When<Envelope<AddressWasMigratedToStreetName>>(async (context, message, ct) =>
            {
                var addressDetail = new AddressDetail(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.ParentPersistentLocalId,
                    await GetNisCode(message.Message.StreetNamePersistentLocalId, ct),
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    message.Message.Status,
                    message.Message.OfficiallyAssigned,
                    message.Message.ExtendedWkbGeometry.ToByteArray(),
                    message.Message.GeometryMethod,
                    message.Message.GeometrySpecification,
                    message.Message.IsRemoved,
                    message.Message.Provenance.Timestamp);

                await context
                    .Addresses
                    .AddAsync(addressDetail, ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasProposedV2>>(async (context, message, ct) =>
            {
                var addressDetail = new AddressDetail(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.ParentPersistentLocalId,
                    await GetNisCode(message.Message.StreetNamePersistentLocalId, ct),
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    AddressStatus.Proposed,
                    officiallyAssigned: true,
                    position: message.Message.ExtendedWkbGeometry.ToByteArray(),
                    positionMethod: message.Message.GeometryMethod,
                    positionSpecification: message.Message.GeometrySpecification,
                    removed: false,
                    message.Message.Provenance.Timestamp);

                await context
                    .Addresses
                    .AddAsync(addressDetail, ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var addressDetail = new AddressDetail(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.ParentPersistentLocalId,
                    await GetNisCode(message.Message.StreetNamePersistentLocalId, ct),
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    AddressStatus.Proposed,
                    officiallyAssigned: message.Message.OfficiallyAssigned,
                    position: message.Message.ExtendedWkbGeometry.ToByteArray(),
                    positionMethod: message.Message.GeometryMethod,
                    positionSpecification: message.Message.GeometrySpecification,
                    removed: false,
                    message.Message.Provenance.Timestamp);

                await context
                    .Addresses
                    .AddAsync(addressDetail, ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasApproved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.OfficiallyAssigned = false;
                        item.Status = AddressStatus.Current;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRegularized>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.OfficiallyAssigned = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.PostalCode = message.Message.PostalCode;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetail(
                        boxNumberPersistentLocalId,
                        boxNumberItem =>
                        {
                            boxNumberItem.PostalCode = message.Message.PostalCode;
                            UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    await Produce(context, boxNumberPersistentLocalId, message.Position, ct);
                }
            });

            When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.PostalCode = message.Message.PostalCode;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetail(
                        boxNumberPersistentLocalId,
                        boxNumberItem =>
                        {
                            boxNumberItem.PostalCode = message.Message.PostalCode;
                            UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    await Produce(context, boxNumberPersistentLocalId, message.Position, ct);
                }
            });

            When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.HouseNumber = message.Message.HouseNumber;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.FindAndUpdateAddressDetail(
                        boxNumberPersistentLocalId,
                        boxNumberItem =>
                        {
                            boxNumberItem.HouseNumber = message.Message.HouseNumber;
                            UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    await Produce(context, boxNumberPersistentLocalId, message.Position, ct);
                }
            });

            When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.BoxNumber = message.Message.BoxNumber;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressBoxNumbersWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var (addressPersistentLocalId, boxNumber) in message.Message.AddressBoxNumbers)
                {
                    await context.FindAndUpdateAddressDetail(
                        addressPersistentLocalId,
                        boxNumberItem =>
                        {
                            boxNumberItem.BoxNumber = boxNumber;
                            UpdateVersionTimestamp(boxNumberItem, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    await Produce(context, addressPersistentLocalId, message.Position, ct);
                }
            });

            When<Envelope<AddressPositionWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.PositionMethod = message.Message.GeometryMethod;
                        item.PositionSpecification = message.Message.GeometrySpecification;
                        item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();

                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.PositionMethod = message.Message.GeometryMethod;
                        item.PositionSpecification = message.Message.GeometrySpecification;
                        item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();

                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressHouseNumberWasReaddressed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = message.Message.ReaddressedHouseNumber.SourceStatus;
                        item.HouseNumber = message.Message.ReaddressedHouseNumber.DestinationHouseNumber;
                        item.PostalCode = message.Message.ReaddressedHouseNumber.SourcePostalCode;
                        item.OfficiallyAssigned = message.Message.ReaddressedHouseNumber.SourceIsOfficiallyAssigned;
                        item.PositionMethod = message.Message.ReaddressedHouseNumber.SourceGeometryMethod;
                        item.PositionSpecification = message.Message.ReaddressedHouseNumber.SourceGeometrySpecification;
                        item.Position = message.Message.ReaddressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray();

                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);

                foreach (var readdressedBoxNumber in message.Message.ReaddressedBoxNumbers)
                {
                    await context.FindAndUpdateAddressDetail(
                        readdressedBoxNumber.DestinationAddressPersistentLocalId,
                        item =>
                        {
                            item.Status = readdressedBoxNumber.SourceStatus;
                            item.HouseNumber = readdressedBoxNumber.DestinationHouseNumber;
                            item.BoxNumber = readdressedBoxNumber.SourceBoxNumber;
                            item.PostalCode = readdressedBoxNumber.SourcePostalCode;
                            item.OfficiallyAssigned = readdressedBoxNumber.SourceIsOfficiallyAssigned;
                            item.PositionMethod = readdressedBoxNumber.SourceGeometryMethod;
                            item.PositionSpecification = readdressedBoxNumber.SourceGeometrySpecification;
                            item.Position = readdressedBoxNumber.SourceExtendedWkbGeometry.ToByteArray();

                            UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                        },
                        ct);

                    await Produce(context, readdressedBoxNumber.DestinationAddressPersistentLocalId, message.Position, ct);
                }
            });

            When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var addressDetail = new AddressDetail(
                    message.Message.AddressPersistentLocalId,
                    message.Message.StreetNamePersistentLocalId,
                    message.Message.ParentPersistentLocalId,
                    await GetNisCode(message.Message.StreetNamePersistentLocalId, ct),
                    message.Message.PostalCode,
                    message.Message.HouseNumber,
                    message.Message.BoxNumber,
                    AddressStatus.Proposed,
                    officiallyAssigned: true,
                    position: message.Message.ExtendedWkbGeometry.ToByteArray(),
                    positionMethod: message.Message.GeometryMethod,
                    positionSpecification: message.Message.GeometrySpecification,
                    removed: false,
                    message.Message.Provenance.Timestamp);

                await context
                    .Addresses
                    .AddAsync(addressDetail, ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Rejected;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfReaddress>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Removed = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Removed = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Removed = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.OfficiallyAssigned = false;
                        item.Status = AddressStatus.Current;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.OfficiallyAssigned = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });

            When<Envelope<AddressRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressPersistentLocalId,
                    item =>
                    {
                        item.Status =  message.Message.Status;
                        item.PostalCode = message.Message.PostalCode;
                        item.HouseNumber = message.Message.HouseNumber;
                        item.BoxNumber = message.Message.BoxNumber;
                        item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
                        item.PositionMethod = message.Message.GeometryMethod;
                        item.PositionSpecification = message.Message.GeometrySpecification;
                        item.OfficiallyAssigned = message.Message.OfficiallyAssigned;
                        item.Removed = false;

                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);

                await Produce(context, message.Message.AddressPersistentLocalId, message.Position, ct);
            });
        }

        private async Task<string> GetNisCode(int streetNamePersistentLocalId, CancellationToken cancellationToken)
        {
            await using var context = await _streetNameConsumerContextFactory.CreateDbContextAsync(cancellationToken);

            var streetName = await context.StreetNameLatestItems.FindAsync(streetNamePersistentLocalId, cancellationToken: cancellationToken)
                ?? throw new ProjectionItemNotFoundException<StreetNameConsumerContext>(streetNamePersistentLocalId.ToString());

            return streetName.NisCode
                ?? throw new InvalidOperationException($"StreetName {streetNamePersistentLocalId} NisCode is null");
        }

        private static void UpdateVersionTimestamp(AddressDetail addressDetailItem, Instant versionTimestamp)
            => addressDetailItem.VersionTimestamp = versionTimestamp;

        private static void UpdateVersionTimestampIfNewer(AddressDetail addressDetailItem, Instant versionTimestamp)
        {
            if(versionTimestamp > addressDetailItem.VersionTimestamp)
            {
                addressDetailItem.VersionTimestamp = versionTimestamp;
            }
        }

        private async Task Produce(
            ProducerContext context,
            int addressPersistentLocalId,
            long storePosition,
            CancellationToken cancellationToken = default)
        {
            var address = await context.Addresses.FindAsync(addressPersistentLocalId, cancellationToken: cancellationToken)
                             ?? throw new ProjectionItemNotFoundException<ProducerProjections>(addressPersistentLocalId.ToString());

            if (!RegionFilter.IsFlemishRegion(address.NisCode))
            {
                return;
            }

            var addressLdes = new AddressLdes(address, _osloNamespace);

            await Produce(
                $"{_osloNamespace}/{address.AddressPersistentLocalId}",
                address.AddressPersistentLocalId.ToString(),
                JsonConvert.SerializeObject(addressLdes, _jsonSerializerSettings),
                storePosition,
                cancellationToken);
        }

        private async Task Produce(string puri, string objectId, string jsonContent, long storePosition, CancellationToken cancellationToken = default)
        {
            var result = await _producer.Produce(
                new MessageKey(puri),
                jsonContent,
                new List<MessageHeader> { new(MessageHeader.IdempotenceKey, $"{objectId}-{storePosition.ToString()}") },
                cancellationToken);

            if (!result.IsSuccess)
            {
                throw new InvalidOperationException(result.Error + Environment.NewLine + result.ErrorReason); //TODO: create custom exception
            }
        }

        private static Task DoNothing<T>(ProducerContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
