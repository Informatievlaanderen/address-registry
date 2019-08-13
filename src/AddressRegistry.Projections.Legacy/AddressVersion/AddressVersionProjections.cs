namespace AddressRegistry.Projections.Legacy.AddressVersion
{
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Microsoft.EntityFrameworkCore;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Address.Events.Crab;
    using NetTopologySuite.IO;

    public static class AddressVersionsQueries
    {
        public static async Task<IEnumerable<AddressVersion>> AllVersions(
            this LegacyContext context,
            Guid addressId,
            CancellationToken ct)
        {
            var sqlEntities = await context
                .AddressVersions
                .Where(x => x.AddressId == addressId)
                .ToListAsync(ct);

            var localEntities = context
                .AddressVersions
                .Local
                .Where(x => x.AddressId == addressId)
                .ToList();

            return sqlEntities
                .Union(localEntities)
                .Distinct();
        }
    }

    public class AddressVersionProjections : ConnectedProjection<LegacyContext>
    {
        public AddressVersionProjections(WKBReader wkbReader)
        {
            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .AddressVersions
                    .AddAsync(
                        new AddressVersion
                        {
                            AddressId = message.Message.AddressId,
                            StreetNameId = message.Message.StreetNameId,
                            HouseNumber = message.Message.HouseNumber,
                            StreamPosition = message.Position,
                        });
            });

            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                var entities = await context.AllVersions(message.Message.AddressId, ct);

                foreach (var entity in entities)
                    entity.PersistentLocalId = message.Message.PersistentLocalId;
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.Complete = true,
                    ct);
            });

            When<Envelope<AddressBecameCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.Status = AddressStatus.Current,
                    ct);
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.Complete = false,
                    ct);
            });

            When<Envelope<AddressBecameNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.OfficiallyAssigned = false,
                    ct);
            });

            When<Envelope<AddressHouseNumberWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.HouseNumber = message.Message.HouseNumber,
                    ct);
            });

            When<Envelope<AddressHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.HouseNumber = message.Message.HouseNumber,
                    ct);
            });

            When<Envelope<AddressOfficialAssignmentWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.OfficiallyAssigned = null,
                    ct);
            });

            When<Envelope<AddressPositionWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
                        item.PositionMethod = message.Message.GeometryMethod;
                        item.PositionSpecification = message.Message.GeometrySpecification;
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.Position = null;
                        item.PositionMethod = null;
                        item.PositionSpecification = null;
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.PostalCode = message.Message.PostalCode,
                    ct);
            });

            When<Envelope<AddressPostalCodeWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.PostalCode = message.Message.PostalCode,
                    ct);
            });
            When<Envelope<AddressPostalCodeWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.PostalCode = null,
                    ct);
            });

            When<Envelope<AddressStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.Status = null,
                    ct);
            });

            When<Envelope<AddressStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.Status = null,
                    ct);
            });

            When<Envelope<AddressStreetNameWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.StreetNameId = message.Message.StreetNameId,
                    ct);
            });

            When<Envelope<AddressStreetNameWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.StreetNameId = message.Message.StreetNameId,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.Status = AddressStatus.Current,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.OfficiallyAssigned = false,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.OfficiallyAssigned = true,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.Status = AddressStatus.Proposed,
                    ct);
            });

            When<Envelope<AddressWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.Status = AddressStatus.Retired,
                    ct);
            });

            When<Envelope<AddressWasOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.OfficiallyAssigned = true,
                    ct);
            });

            When<Envelope<AddressWasPositioned>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item =>
                    {
                        item.Position = message.Message.ExtendedWkbGeometry.ToByteArray();
                        item.PositionMethod = message.Message.GeometryMethod;
                        item.PositionSpecification = message.Message.GeometrySpecification;
                    },
                    ct);
            });

            When<Envelope<AddressWasProposed>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.Status = AddressStatus.Proposed,
                    ct);
            });

            When<Envelope<AddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.Removed = true,
                    ct);
            });

            When<Envelope<AddressWasRetired>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.Status = AddressStatus.Retired,
                    ct);
            });

            When<Envelope<AddressBoxNumberWasChanged>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.BoxNumber = message.Message.BoxNumber,
                    ct);
            });

            When<Envelope<AddressBoxNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.BoxNumber = message.Message.BoxNumber,
                    ct);
            });

            When<Envelope<AddressBoxNumberWasRemoved>>(async (context, message, ct) =>
            {
                await context.CreateNewAddressVersion(
                    message.Message.AddressId,
                    message,
                    item => item.BoxNumber = null,
                    ct);
            });

            When<Envelope<AddressHouseNumberWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressHouseNumberMailCantonWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressPositionWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
            When<Envelope<AddressSubaddressStatusWasImportedFromCrab>>(async (context, message, ct) => DoNothing());
        }

        private static void DoNothing() { }
    }
}
