namespace AddressRegistry.Projections.Legacy.AddressSyndication
{
    using System;
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using NodaTime;

    public class AddressSyndicationProjections : ConnectedProjection<LegacyContext>
    {
        public AddressSyndicationProjections()
        {
            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = new AddressSyndicationItem
                {
                    Position = message.Position,
                    AddressId = message.Message.AddressId,
                    StreetNameId = message.Message.StreetNameId,
                    HouseNumber = message.Message.HouseNumber,
                    RecordCreatedAt = Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    LastChangedOn = Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    ChangeType = message.EventName
                };

                await context
                    .AddressSyndication
                    .AddAsync(addressSyndicationItem, ct);
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.IsComplete = true);

                newAddressSyndicationItem.IsComplete = true;

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressBecameCurrent>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.Status = AddressStatus.Current);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.IsComplete = false);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressBoxNumberWasChanged>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.BoxNumber = message.Message.BoxNumber);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressBoxNumberWasCorrected>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.BoxNumber = message.Message.BoxNumber);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressBoxNumberWasRemoved>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.BoxNumber = null);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressHouseNumberWasChanged>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.HouseNumber = message.Message.HouseNumber);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.HouseNumber = message.Message.HouseNumber);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressPostalCodeWasChanged>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.PostalCode = message.Message.PostalCode);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressPostalCodeWasCorrected>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.PostalCode = message.Message.PostalCode);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressPostalCodeWasRemoved>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.PostalCode = null);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.Status = null);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressStatusWasRemoved>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.Status = null);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressStreetNameWasChanged>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.StreetNameId = message.Message.StreetNameId);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressStreetNameWasCorrected>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.StreetNameId = message.Message.StreetNameId);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.Status = AddressStatus.Current);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.Status = AddressStatus.Proposed);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.Status = AddressStatus.Retired);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressWasProposed>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.Status = AddressStatus.Proposed);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressWasRemoved>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => { });

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressWasRetired>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.Status = AddressStatus.Retired);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressPostalCodeWasChanged>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.PostalCode = message.Message.PostalCode);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressPostalCodeWasCorrected>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.PostalCode = message.Message.PostalCode);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressPostalCodeWasRemoved>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    message.Message.Provenance.Timestamp,
                    x => x.PostalCode = null);

                ApplyProvenance(newAddressSyndicationItem, message.Message.Provenance);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });

            When<Envelope<AddressOsloIdWasAssigned>>(async (context, message, ct) =>
            {
                var addressSyndicationItem = await context.LatestPosition(message.Message.AddressId, ct);

                if (addressSyndicationItem == null)
                    throw DatabaseItemNotFound(message.Message.AddressId);

                if (addressSyndicationItem.Position >= message.Position)
                    return;

                var newAddressSyndicationItem = addressSyndicationItem.CloneAndApplyEventInfo(
                    message.Position,
                    message.EventName,
                    Instant.FromDateTimeUtc(message.CreatedUtc.ToUniversalTime()),
                    x => x.OsloId = message.Message.OsloId);

                await context
                    .AddressSyndication
                    .AddAsync(newAddressSyndicationItem, ct);
            });
        }

        private static ProjectionItemNotFoundException<AddressSyndicationProjections> DatabaseItemNotFound(Guid addressId)
            => new ProjectionItemNotFoundException<AddressSyndicationProjections>(addressId.ToString("D"));

        private static void ApplyProvenance(AddressSyndicationItem item, ProvenanceData provenance)
        {
            item.Application = provenance.Application;
            item.Modification = provenance.Modification;
            item.Operator = provenance.Operator;
            item.Organisation = provenance.Organisation;
            item.Plan = provenance.Plan;
        }
    }
}
