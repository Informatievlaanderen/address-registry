namespace AddressRegistry.Api.Backoffice.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.Crab.GeoJson;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using CrabEdit.Client;
    using CrabEdit.Infrastructure;
    using CrabEdit.Infrastructure.Address.Requests;
    using Infrastructure.Mapping.CrabEdit;
    using NodaTime;
    using Projections.Legacy.CrabIdToPersistentLocalId;
    using Requests;
    using AddressPosition = CrabEdit.Infrastructure.Address.AddressPosition;

    public class AddressCrabEditClient
    {
        private readonly CrabEditClient _client;
        private readonly ICrabGeoJsonMapper _geoJsonMapper;

        public AddressCrabEditClient(
            CrabEditClient crabEditClient,
            ICrabGeoJsonMapper geoJsonMapper)
        {
            _client = crabEditClient ?? throw new ArgumentNullException(nameof(crabEditClient));
            _geoJsonMapper = geoJsonMapper ?? throw new ArgumentNullException(nameof(geoJsonMapper));
        }

        public async Task<CrabEditClientResult<CrabHouseNumberId>> AddHouseNumberToCrab(
            AddAddressRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var addCrabHouseNumberResponse = await _client.Add(
                new AddHouseNumber
                {
                    StreetNameId = request.StreetNameId.AsIdentifier().Map(IdentifierMappings.StreetNameId),
                    HouseNumber = request.HouseNumber,
                    PostalCode = request.PostalCode.AsIdentifier().Map(IdentifierMappings.PostalCode),
                    Status = request.Status.AsIdentifier().Map(IdentifierMappings.AddressStatus),
                    OfficiallyAssigned = request.OfficiallyAssigned,
                    Position = MapPosition(request.Position)
                },
                cancellationToken);

            return CrabEditClientResult<CrabHouseNumberId>
                .From(
                    addCrabHouseNumberResponse,
                    identifier => new CrabHouseNumberId(addCrabHouseNumberResponse.AddressId));
        }

        public async Task<CrabEditResponse> Delete(
            CrabIdToPersistentLocalIdItem addressId,
            CancellationToken cancellationToken)
        {
            if (addressId?.HouseNumberId.HasValue ?? false)
                return await _client.Delete(new RemoveHouseNumber { HouseNumberId = addressId.HouseNumberId.Value }, cancellationToken);

            if (addressId?.SubaddressId.HasValue ?? false)
                return await _client.Delete(new RemoveSubaddress { SubaddressId = addressId.SubaddressId.Value }, cancellationToken);

            return new CrabEditResponse(Instant.MinValue);
        }

        public async Task<CrabEditClientResult> ChangeHouseNumber(
            int houseNumberId,
            ChangeAddressRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var updateResponse = await _client.Update(
                new EditHouseNumber
                {
                    HouseNumberId = houseNumberId,
                    OfficiallyAssigned = request.OfficiallyAssigned,
                    Position = MapPosition(request.Position),
                    PostalCode = request.PostalCode.AsIdentifier().Map(IdentifierMappings.PostalCode),
                    Status = request.Status.AsIdentifier().Map(IdentifierMappings.AddressStatus)
                },
                cancellationToken);

            return CrabEditClientResult.From(updateResponse);
        }

        private AddressPosition MapPosition(Requests.AddressPosition position)
        {
            if (position == null)
                throw new ArgumentNullException(nameof(position));

            return new AddressPosition
            {
                Method = position.Method.AsIdentifier().Map(IdentifierMappings.PositionGeometryMethod),
                Specification = position.Specification.AsIdentifier().Map(IdentifierMappings.PositionSpecification),
                Wkt = _geoJsonMapper.ToWkt(position.Point)
            };
        }

        public async Task<CrabEditClientResult> ChangeSubaddress(ChangeAddressRequest request,
            CancellationToken cancellationToken)
            => throw new NotImplementedException();
    }
}
