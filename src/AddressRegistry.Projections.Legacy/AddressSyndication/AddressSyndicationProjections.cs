namespace AddressRegistry.Projections.Legacy.AddressSyndication
{
    using System;
    using System.Threading.Tasks;
    using StreetName;
    using Address.Events;
    using Address.Events.Crab;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
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

            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressHouseNumberMailCantonWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => await DoNothing());
            #endregion Legacy Events

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

            When<Envelope<AddressWasRemovedV2>>(async (context, message, ct) =>
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
        }

        private static async Task DoNothing()
        {
            await Task.Yield();
        }
    }
}
