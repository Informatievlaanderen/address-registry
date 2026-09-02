namespace AddressRegistry.Projections.Legacy.AddressSyndication
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Address.Events;
    using Address.Events.Crab;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using StreetName;
    using StreetName.Events;

    [ConnectedProjectionName("Feed endpoint adressen")]
    [ConnectedProjectionDescription("Projectie die de adressen data voor de adressen feed voorziet.")]
    public class AddressSyndicationProjections : ConnectedProjection<LegacyContext>
    {
        public AddressSyndicationProjections()
        {
            #region Legacy Events
            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = new AddressSyndicationItem
                {
                    Position = message.Position,
                    AddressId = message.Message.AddressId,
                    StreetNameId = message.Message.StreetNameId,
                    HouseNumber = message.Message.HouseNumber,
                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                    SyndicationItemCreatedAt = DateTimeOffset.UtcNow
                };

                addressSyndicationItem.ApplyProvenance(message.Message.Provenance);
                addressSyndicationItem.SetEventData(message.Message, message.EventName);

                await context
                    .AddressSyndication
                    .AddAsync(addressSyndicationItem, ct);
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.IsComplete = true,
                    ct);
            });

            When<Envelope<AddressBecameCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = AddressStatus.Current,
                    ct);
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.IsComplete = false,
                    ct);
            });

            When<Envelope<AddressBecameNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.IsOfficiallyAssigned = false,
                    ct);
            });

            When<Envelope<AddressBoxNumberWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.BoxNumber = message.Message.BoxNumber,
                    ct);
            });

            When<Envelope<AddressBoxNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.BoxNumber = message.Message.BoxNumber,
                    ct);
            });

            When<Envelope<AddressBoxNumberWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.BoxNumber = null,
                    ct);
            });

            When<Envelope<AddressHouseNumberWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.HouseNumber = message.Message.HouseNumber,
                    ct);
            });

            When<Envelope<AddressHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.HouseNumber = message.Message.HouseNumber,
                    ct);
            });

            When<Envelope<AddressOfficialAssignmentWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.IsOfficiallyAssigned = false,
                    ct);
            });

            When<Envelope<AddressPositionWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x =>
                    {
                        x.PositionMethod = (GeometryMethod?)message.Message.GeometryMethod;
                        x.PositionSpecification = (GeometrySpecification?)message.Message.GeometrySpecification;
                        x.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x =>
                    {
                        x.PositionMethod = null;
                        x.PositionSpecification = null;
                        x.PointPosition = null;
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.PostalCode = message.Message.PostalCode,
                    ct);
            });

            When<Envelope<AddressPostalCodeWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.PostalCode = message.Message.PostalCode,
                    ct);
            });

            When<Envelope<AddressPostalCodeWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.PostalCode = null,
                    ct);
            });

            When<Envelope<AddressStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = null,
                    ct);
            });

            When<Envelope<AddressStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = null,
                    ct);
            });

            When<Envelope<AddressStreetNameWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.StreetNameId = message.Message.StreetNameId,
                    ct);
            });

            When<Envelope<AddressStreetNameWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.StreetNameId = message.Message.StreetNameId,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = AddressStatus.Current,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.IsOfficiallyAssigned = true,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.IsOfficiallyAssigned = false,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = AddressStatus.Proposed,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = AddressStatus.Retired,
                    ct);
            });

            When<Envelope<AddressWasOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.IsOfficiallyAssigned = true,
                    ct);
            });

            When<Envelope<AddressWasPositioned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x =>
                    {
                        x.PositionMethod = (GeometryMethod?)message.Message.GeometryMethod;
                        x.PositionSpecification = (GeometrySpecification?)message.Message.GeometrySpecification;
                        x.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();
                    },
                    ct);
            });

            When<Envelope<AddressWasProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = AddressStatus.Proposed,
                    ct);
            });

            When<Envelope<AddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => { },
                    ct);
            });

            When<Envelope<AddressWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.Status = AddressStatus.Retired,
                    ct);
            });

            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressId,
                    message,
                    x => x.PersistentLocalId = message.Message.PersistentLocalId,
                    ct);
            });

            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(DoNothing);
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(DoNothing);
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(DoNothing);
            When<Envelope<AddressHouseNumberMailCantonWasImportedFromCrab>>(DoNothing);
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(DoNothing);
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(DoNothing);
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(DoNothing);
            #endregion Legacy Events

            #region StreetName

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
            When<Envelope<StreetNameHomonymAdditionsWereCorrected>>(DoNothing);
            When<Envelope<StreetNameHomonymAdditionsWereRemoved>>(DoNothing);
            When<Envelope<StreetNameNamesWereChanged>>(DoNothing);
            When<Envelope<StreetNameNamesWereCorrected>>(DoNothing);

            #endregion

            When<Envelope<AddressWasMigratedToStreetName>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = new AddressSyndicationItem
                {
                    Position = message.Position,
                    AddressId = null, //while we have the information, we shouldn't identify this resource with its old guid id
                    PersistentLocalId = message.Message.AddressPersistentLocalId,
                    StreetNameId = null, //while we have the information, we shouldn't identify this resource with its old guid id
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    PostalCode = message.Message.PostalCode,
                    HouseNumber = message.Message.HouseNumber,
                    BoxNumber = message.Message.BoxNumber,
                    PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray(),
                    PositionMethod = message.Message.GeometryMethod,
                    PositionSpecification = message.Message.GeometrySpecification,
                    Status = message.Message.Status,
                    IsComplete = true,
                    IsOfficiallyAssigned = message.Message.OfficiallyAssigned,

                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                    SyndicationItemCreatedAt = DateTimeOffset.UtcNow
                };

                addressSyndicationItem.ApplyProvenance(message.Message.Provenance);
                addressSyndicationItem.SetEventData(message.Message, message.EventName);

                await context
                    .AddressSyndication
                    .AddAsync(addressSyndicationItem, ct);

                if (!string.IsNullOrWhiteSpace(addressSyndicationItem.BoxNumber))
                {
                    await context
                        .AddressBoxNumberSyndicationHelper
                        .AddAsync(new AddressBoxNumberSyndicationHelper
                        {
                            PersistentLocalId = addressSyndicationItem.PersistentLocalId.Value,
                            PostalCode = addressSyndicationItem.PostalCode,
                            HouseNumber = addressSyndicationItem.HouseNumber,
                            BoxNumber = addressSyndicationItem.BoxNumber,
                            PointPosition = addressSyndicationItem.PointPosition,
                            PositionMethod = addressSyndicationItem.PositionMethod,
                            PositionSpecification = addressSyndicationItem.PositionSpecification,
                            Status = addressSyndicationItem.Status,
                            IsComplete = addressSyndicationItem.IsComplete,
                            IsOfficiallyAssigned = addressSyndicationItem.IsOfficiallyAssigned,
                        });
                }
            });

            When<Envelope<AddressWasProposedV2>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = new AddressSyndicationItem
                {
                    Position = message.Position,
                    PersistentLocalId = message.Message.AddressPersistentLocalId,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    PostalCode = message.Message.PostalCode,
                    HouseNumber = message.Message.HouseNumber,
                    BoxNumber = message.Message.BoxNumber,
                    PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray(),
                    PositionMethod = message.Message.GeometryMethod,
                    PositionSpecification = message.Message.GeometrySpecification,
                    Status = AddressStatus.Proposed,
                    IsComplete = true,
                    IsOfficiallyAssigned = true,

                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                    SyndicationItemCreatedAt = DateTimeOffset.UtcNow
                };

                addressSyndicationItem.ApplyProvenance(message.Message.Provenance);
                addressSyndicationItem.SetEventData(message.Message, message.EventName);

                await context
                    .AddressSyndication
                    .AddAsync(addressSyndicationItem, ct);

                if (!string.IsNullOrWhiteSpace(addressSyndicationItem.BoxNumber))
                {
                    var addressBoxNumberSyndicationHelper = new AddressBoxNumberSyndicationHelper
                    {
                        PersistentLocalId = addressSyndicationItem.PersistentLocalId.Value,
                        PostalCode = addressSyndicationItem.PostalCode,
                        HouseNumber = addressSyndicationItem.HouseNumber,
                        BoxNumber = addressSyndicationItem.BoxNumber,
                        PointPosition = addressSyndicationItem.PointPosition,
                        PositionMethod = addressSyndicationItem.PositionMethod,
                        PositionSpecification = addressSyndicationItem.PositionSpecification,
                        Status = addressSyndicationItem.Status,
                        IsComplete = addressSyndicationItem.IsComplete,
                        IsOfficiallyAssigned = addressSyndicationItem.IsOfficiallyAssigned,
                    };

                    await context
                        .AddressBoxNumberSyndicationHelper
                        .AddAsync(addressBoxNumberSyndicationHelper);

                }
            });

            When<Envelope<AddressWasProposedForMunicipalityMerger>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = new AddressSyndicationItem
                {
                    Position = message.Position,
                    PersistentLocalId = message.Message.AddressPersistentLocalId,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    PostalCode = message.Message.PostalCode,
                    HouseNumber = message.Message.HouseNumber,
                    BoxNumber = message.Message.BoxNumber,
                    PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray(),
                    PositionMethod = message.Message.GeometryMethod,
                    PositionSpecification = message.Message.GeometrySpecification,
                    Status = AddressStatus.Proposed,
                    IsComplete = true,
                    IsOfficiallyAssigned = message.Message.OfficiallyAssigned,

                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                    SyndicationItemCreatedAt = DateTimeOffset.UtcNow
                };

                addressSyndicationItem.ApplyProvenance(message.Message.Provenance);
                addressSyndicationItem.SetEventData(message.Message, message.EventName);

                await context
                    .AddressSyndication
                    .AddAsync(addressSyndicationItem, ct);

                if (!string.IsNullOrWhiteSpace(addressSyndicationItem.BoxNumber))
                {
                    var addressBoxNumberSyndicationHelper = new AddressBoxNumberSyndicationHelper
                    {
                        PersistentLocalId = addressSyndicationItem.PersistentLocalId.Value,
                        PostalCode = addressSyndicationItem.PostalCode,
                        HouseNumber = addressSyndicationItem.HouseNumber,
                        BoxNumber = addressSyndicationItem.BoxNumber,
                        PointPosition = addressSyndicationItem.PointPosition,
                        PositionMethod = addressSyndicationItem.PositionMethod,
                        PositionSpecification = addressSyndicationItem.PositionSpecification,
                        Status = addressSyndicationItem.Status,
                        IsComplete = addressSyndicationItem.IsComplete,
                        IsOfficiallyAssigned = addressSyndicationItem.IsOfficiallyAssigned,
                    };

                    await context
                        .AddressBoxNumberSyndicationHelper
                        .AddAsync(addressBoxNumberSyndicationHelper);

                }
            });

            When<Envelope<AddressWasApproved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Current,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Current,
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Proposed,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Proposed,
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromApprovedToProposedBecauseHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Proposed,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Proposed,
                    ct);
            });

            When<Envelope<AddressWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Rejected,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Rejected,
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Rejected,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Rejected,
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Rejected,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Rejected,
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Rejected,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Rejected,
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Rejected,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Rejected,
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRejected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Retired,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Retired,
                    ct);
            });

            When<Envelope<AddressWasRejectedBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Rejected,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Rejected,
                    ct);
            });

            When<Envelope<AddressWasDeregulated>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x =>
                    {
                        x.IsOfficiallyAssigned = false;
                        x.Status = AddressStatus.Current;
                    },
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x =>
                    {
                        x.IsOfficiallyAssigned = false;
                        x.Status = AddressStatus.Current;
                    },
                    ct);
            });

            When<Envelope<AddressWasRegularized>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.IsOfficiallyAssigned = true,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.IsOfficiallyAssigned = true,
                    ct);
            });

            When<Envelope<AddressWasRetiredV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Retired,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Retired,
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfMunicipalityMerger>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Retired,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Retired,
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseHouseNumberWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Retired,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Retired,
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseStreetNameWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Retired,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Retired,
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromRetiredToCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Current,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Current,
                    ct);
            });

            When<Envelope<AddressPostalCodeWasChangedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.PostalCode = message.Message.PostalCode,
                    ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.UpdateAddressBoxNumberSyndicationHelper(
                        boxNumberPersistentLocalId,
                        x => x.PostalCode = message.Message.PostalCode,
                        ct);
                }
            });

            When<Envelope<AddressPostalCodeWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.PostalCode = message.Message.PostalCode,
                    ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.UpdateAddressBoxNumberSyndicationHelper(
                        boxNumberPersistentLocalId,
                        x => x.PostalCode = message.Message.PostalCode,
                        ct);
                }
            });

            When<Envelope<AddressHouseNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.HouseNumber = message.Message.HouseNumber,
                    ct);

                foreach (var boxNumberPersistentLocalId in message.Message.BoxNumberPersistentLocalIds)
                {
                    await context.UpdateAddressBoxNumberSyndicationHelper(
                        boxNumberPersistentLocalId,
                        x => x.HouseNumber = message.Message.HouseNumber,
                        ct);
                }
            });

            When<Envelope<AddressBoxNumberWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.BoxNumber = message.Message.BoxNumber,
                    ct);
                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.BoxNumber = message.Message.BoxNumber,
                    ct);
            });

            When<Envelope<AddressBoxNumbersWereCorrected>>(async (context, message, ct) =>
            {
                foreach (var (addressPersistentLocalId, boxNumber) in message.Message.AddressBoxNumbers)
                {
                    await context.CreateNewAddressSyndicationItem(
                        addressPersistentLocalId,
                        message,
                        x => x.BoxNumber = boxNumber,
                        ct);
                }
            });

            When<Envelope<AddressPositionWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x =>
                    {
                        x.PositionMethod = message.Message.GeometryMethod;
                        x.PositionSpecification = message.Message.GeometrySpecification;
                        x.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();
                    },
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x =>
                    {
                        x.PositionMethod = message.Message.GeometryMethod;
                        x.PositionSpecification = message.Message.GeometrySpecification;
                        x.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();
                    },
                    ct);
            });

            // The event is published in the feed, carrying the position the event store now holds, but the
            // transformation is not a change to the address: LastChangedOn keeps the value the address's last
            // real change gave it. A removed address is not published at all. See ADR 0005.
            When<Envelope<AddressPositionCrsWasChanged>>(async (context, message, ct) =>
            {
                // Updated even for a removed address: the helper is not published, it is the row a box number
                // address's next item is cloned from — including after AddressRemovalWasCorrected — so leaving
                // it in Lambert 72 would resurrect the old position.
                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x =>
                    {
                        x.PositionMethod = message.Message.GeometryMethod;
                        x.PositionSpecification = message.Message.GeometrySpecification;
                        x.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();
                    },
                    ct);

                // Read up front for two things: whether the address is still in the feed, and the version to
                // carry over — CloneAndApplyEventInfo sets LastChangedOn to the event's timestamp and the edit
                // below is what puts it back.
                var previous = await context.AddressSyndication.LatestPosition(
                    message.Message.AddressPersistentLocalId, ct);

                if (previous is null || IsRemovedInFeed(previous))
                {
                    return;
                }

                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x =>
                    {
                        x.PositionMethod = message.Message.GeometryMethod;
                        x.PositionSpecification = message.Message.GeometrySpecification;
                        x.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();
                        x.LastChangedOn = previous.LastChangedOn;
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasCorrectedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x =>
                    {
                        x.PositionMethod = message.Message.GeometryMethod;
                        x.PositionSpecification = message.Message.GeometrySpecification;
                        x.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();
                    },
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x =>
                    {
                        x.PositionMethod = message.Message.GeometryMethod;
                        x.PositionSpecification = message.Message.GeometrySpecification;
                        x.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();
                    },
                    ct);
            });

            When<Envelope<AddressHouseNumberWasReaddressed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x =>
                    {
                        x.Status = message.Message.ReaddressedHouseNumber.SourceStatus;
                        x.HouseNumber = message.Message.ReaddressedHouseNumber.DestinationHouseNumber;
                        x.PostalCode = message.Message.ReaddressedHouseNumber.SourcePostalCode;
                        x.IsOfficiallyAssigned = message.Message.ReaddressedHouseNumber.SourceIsOfficiallyAssigned;
                        x.PositionMethod = message.Message.ReaddressedHouseNumber.SourceGeometryMethod;
                        x.PositionSpecification = message.Message.ReaddressedHouseNumber.SourceGeometrySpecification;
                        x.PointPosition = message.Message.ReaddressedHouseNumber.SourceExtendedWkbGeometry.ToByteArray();
                    },
                    ct);

                foreach (var readdressedBoxNumber in message.Message.ReaddressedBoxNumbers)
                {
                    await context.UpdateAddressBoxNumberSyndicationHelper(
                        readdressedBoxNumber.DestinationAddressPersistentLocalId,
                        x =>
                        {
                            x.Status = readdressedBoxNumber.SourceStatus;
                            x.HouseNumber = readdressedBoxNumber.DestinationHouseNumber;
                            x.BoxNumber = readdressedBoxNumber.SourceBoxNumber;
                            x.PostalCode = readdressedBoxNumber.SourcePostalCode;
                            x.IsOfficiallyAssigned = readdressedBoxNumber.SourceIsOfficiallyAssigned;
                            x.PositionMethod = readdressedBoxNumber.SourceGeometryMethod;
                            x.PositionSpecification = readdressedBoxNumber.SourceGeometrySpecification;
                            x.PointPosition = readdressedBoxNumber.SourceExtendedWkbGeometry.ToByteArray();
                        },
                        ct);
                }
            });

            When<Envelope<AddressWasProposedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = new AddressSyndicationItem
                {
                    Position = message.Position,
                    PersistentLocalId = message.Message.AddressPersistentLocalId,
                    StreetNamePersistentLocalId = message.Message.StreetNamePersistentLocalId,
                    PostalCode = message.Message.PostalCode,
                    HouseNumber = message.Message.HouseNumber,
                    BoxNumber = message.Message.BoxNumber,
                    PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray(),
                    PositionMethod = message.Message.GeometryMethod,
                    PositionSpecification = message.Message.GeometrySpecification,
                    Status = AddressStatus.Proposed,
                    IsComplete = true,
                    IsOfficiallyAssigned = true,

                    RecordCreatedAt = message.Message.Provenance.Timestamp,
                    LastChangedOn = message.Message.Provenance.Timestamp,
                    ChangeType = message.EventName,
                    SyndicationItemCreatedAt = DateTimeOffset.UtcNow
                };

                addressSyndicationItem.ApplyProvenance(message.Message.Provenance);
                addressSyndicationItem.SetEventData(message.Message, message.EventName);

                await context
                    .AddressSyndication
                    .AddAsync(addressSyndicationItem, ct);

                if (!string.IsNullOrWhiteSpace(addressSyndicationItem.BoxNumber))
                {
                    var addressBoxNumberSyndicationHelper = new AddressBoxNumberSyndicationHelper
                    {
                        PersistentLocalId = addressSyndicationItem.PersistentLocalId.Value,
                        PostalCode = addressSyndicationItem.PostalCode,
                        HouseNumber = addressSyndicationItem.HouseNumber,
                        BoxNumber = addressSyndicationItem.BoxNumber,
                        PointPosition = addressSyndicationItem.PointPosition,
                        PositionMethod = addressSyndicationItem.PositionMethod,
                        PositionSpecification = addressSyndicationItem.PositionSpecification,
                        Status = addressSyndicationItem.Status,
                        IsComplete = addressSyndicationItem.IsComplete,
                        IsOfficiallyAssigned = addressSyndicationItem.IsOfficiallyAssigned,
                    };

                    await context
                        .AddressBoxNumberSyndicationHelper
                        .AddAsync(addressBoxNumberSyndicationHelper);

                }
            });

            When<Envelope<AddressWasRejectedBecauseOfReaddress>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Rejected,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Rejected,
                    ct);
            });

            When<Envelope<AddressWasRetiredBecauseOfReaddress>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Retired,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Retired,
                    ct);
            });

            When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => { },
                    ct);
            });

             When<Envelope<AddressWasRemovedBecauseStreetNameWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => { },
                    ct);
            });

            When<Envelope<AddressWasRemovedBecauseHouseNumberWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => { },
                    ct);
            });

            When<Envelope<AddressWasCorrectedFromRejectedToProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x => x.Status = AddressStatus.Proposed,
                    ct);

                await context.UpdateAddressBoxNumberSyndicationHelper(
                    message.Message.AddressPersistentLocalId,
                    x => x.Status = AddressStatus.Proposed,
                    ct);
            });

            When<Envelope<AddressRegularizationWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x =>
                    {
                        x.Status = AddressStatus.Current;
                        x.IsOfficiallyAssigned = false;
                    },
                    ct);
            });

            When<Envelope<AddressDeregulationWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x =>
                    {
                        x.IsOfficiallyAssigned = true;
                    },
                    ct);
            });

            When<Envelope<AddressRemovalWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressSyndicationItem(
                    message.Message.AddressPersistentLocalId,
                    message,
                    x =>
                    {
                        x.Status = message.Message.Status;
                        x.PostalCode = message.Message.PostalCode;
                        x.HouseNumber = message.Message.HouseNumber;
                        x.BoxNumber = message.Message.BoxNumber;
                        x.PointPosition = message.Message.ExtendedWkbGeometry.ToByteArray();
                        x.PositionMethod = message.Message.GeometryMethod;
                        x.PositionSpecification = message.Message.GeometrySpecification;
                        x.IsOfficiallyAssigned = message.Message.OfficiallyAssigned;
                    },
                    ct);
            });
        }

        /// <summary>
        /// Whether the address's latest entry in the feed says it was removed.
        /// </summary>
        /// <remarks>
        /// The item carries no removed flag, so the entry's own <see cref="AddressSyndicationItem.ChangeType"/>
        /// is the signal the table has — and it is a reliable one: every event that can follow a removal
        /// either un-removes the address (<c>AddressRemovalWasCorrected</c>, which produces an entry of its
        /// own) or is guarded against removed addresses in the aggregate. The Lambert 2008 transformation is
        /// the single exception, and it is what this guards, so it cannot mask a removal either.
        ///
        /// Deriving it beats adding a column: a new flag would read false for every address removed before it
        /// was introduced, which is exactly the set this has to recognise, and the feed is far too large to
        /// rebuild for it. See ADR 0005.
        /// </remarks>
        private static bool IsRemovedInFeed(AddressSyndicationItem item)
            => item.ChangeType is nameof(AddressWasRemoved)
                or nameof(AddressWasRemovedV2)
                or nameof(AddressWasRemovedBecauseStreetNameWasRemoved)
                or nameof(AddressWasRemovedBecauseHouseNumberWasRemoved);

        private static Task DoNothing<T>(LegacyContext context, Envelope<T> envelope, CancellationToken ct) where T: IMessage => Task.CompletedTask;
    }
}
