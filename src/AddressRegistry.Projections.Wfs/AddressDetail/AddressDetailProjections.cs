namespace AddressRegistry.Projections.Wfs.AddressDetail
{
    using Address;
    using Address.Events;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Connector;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.SqlStreamStore;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.Geometries;
    using NetTopologySuite.IO;
    using NodaTime;


    [ConnectedProjectionName("WFS adressen")]
    [ConnectedProjectionDescription("Projectie die de adressen data voor het WFS adresregister voorziet.")]
    public class AddressDetailProjections : ConnectedProjection<WfsContext>
    {
        public AddressDetailProjections(WKBReader wkbReader)
        {
            When<Envelope<AddressWasRegistered>>(async (context, message, ct) =>
            {
                await context
                    .AddressDetail
                    .AddAsync(
                        new AddressDetailItem
                        {
                            AddressId = message.Message.AddressId,
                            StreetNameId = message.Message.StreetNameId,
                            HouseNumber = message.Message.HouseNumber,
                        },
                        ct);
            });

            When<Envelope<AddressBecameComplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Complete = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressBecameCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressBecameIncomplete>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Complete = false;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressBecameNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.OfficiallyAssigned = false;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressHouseNumberWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.HouseNumber = message.Message.HouseNumber;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressHouseNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.HouseNumber = message.Message.HouseNumber;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressOfficialAssignmentWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.OfficiallyAssigned = null;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressPersistentLocalIdWasAssigned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.PersistentLocalId = message.Message.PersistentLocalId;
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasCorrected>>(async (context, message, ct) =>
            {
                TryParsePosition(message.Message.ExtendedWkbGeometry, out var position);

                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Position = position;
                        item.PositionMethod = message.Message.GeometryMethod;
                        item.PositionSpecification = message.Message.GeometrySpecification;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressPositionWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Position = null;
                        item.PositionMethod = null;
                        item.PositionSpecification = null;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.PostalCode = message.Message.PostalCode;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.PostalCode = message.Message.PostalCode;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressPostalCodeWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.PostalCode = null;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressStatusWasCorrectedToRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = null;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressStatusWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = null;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressStreetNameWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.StreetNameId = message.Message.StreetNameId;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressStreetNameWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.StreetNameId = message.Message.StreetNameId;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToCurrent>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = AddressStatus.Current;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToNotOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.OfficiallyAssigned = false;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.OfficiallyAssigned = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasCorrectedToRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasOfficiallyAssigned>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.OfficiallyAssigned = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasPositioned>>(async (context, message, ct) =>
            {
                TryParsePosition(message.Message.ExtendedWkbGeometry, out var position);
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Position = position;
                        item.PositionMethod = message.Message.GeometryMethod;
                        item.PositionSpecification = message.Message.GeometrySpecification;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasProposed>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = AddressStatus.Proposed;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Removed = true;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressWasRetired>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.Status = AddressStatus.Retired;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressBoxNumberWasChanged>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.BoxNumber = message.Message.BoxNumber;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });

            When<Envelope<AddressBoxNumberWasCorrected>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.BoxNumber = message.Message.BoxNumber;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });
            When<Envelope<AddressBoxNumberWasRemoved>>(async (context, message, ct) =>
            {
                await context.FindAndUpdateAddressDetail(
                    message.Message.AddressId,
                    item =>
                    {
                        item.BoxNumber = null;
                        UpdateVersionTimestamp(item, message.Message.Provenance.Timestamp);
                    },
                    ct);
            });
        }

        private static bool TryParsePosition(string extendedWkbGeometry, out Point? point)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(extendedWkbGeometry))
                    throw new ParseException("Invalid extendedWkbGeometry");

                point = (Point) WKBReaderFactory.Create().Read(extendedWkbGeometry.ToByteArray());
                return true;
            }
            catch (ParseException e)
            {
                point = null;
                return false;
            }
        }

        private static void UpdateVersionTimestamp(AddressDetailItem addressDetailItem, Instant versionTimestamp)
            => addressDetailItem.VersionTimestamp = versionTimestamp;
    }
}
