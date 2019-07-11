namespace AddressRegistry.Projections.LastChangedList
{
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.LastChangedList;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;

    public class LastChangedListProjections : LastChangedListConnectedProjection
    {
        protected override string CacheKeyFormat => "legacy/address:{{0}}.{1}";
        protected override string UriFormat => "/v1/adressen/{{0}}";

        private static readonly AcceptType[] SupportedAcceptTypes = { AcceptType.Json, AcceptType.Xml };

        public LastChangedListProjections()
            : base(SupportedAcceptTypes)
        {
            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var attachedRecords = await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);

                foreach (var record in attachedRecords)
                {
                    record.CacheKey = string.Format(record.CacheKey, message.Message.PersistentLocalId);
                    record.Uri = string.Format(record.Uri, message.Message.PersistentLocalId);
                }
            });

            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBecameCurrent>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBecameNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBoxNumberWasChanged>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBoxNumberWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressBoxNumberWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressHouseNumberWasChanged>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressOfficialAssignmentWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressPositionWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressPositionWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressPostalCodeWasChanged>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressPostalCodeWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressPostalCodeWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressStatusWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressStreetNameWasChanged>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressStreetNameWasCorrected>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasCorrectedToNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasCorrectedToOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasPositioned>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasProposed>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasRemoved>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });

            When<Envelope<AddressWasRetired>>(async (context, message, ct) =>
            {
                await GetLastChangedRecordsAndUpdatePosition(message.Message.AddressId.ToString(), message.Position, context, ct);
            });
        }
    }
}
