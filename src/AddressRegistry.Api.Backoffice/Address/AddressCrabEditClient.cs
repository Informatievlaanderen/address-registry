namespace AddressRegistry.Api.Backoffice.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Crab;
    using CrabEdit.Client;
    using CrabEdit.Infrastructure;
    using CrabEdit.Infrastructure.Address;
    using CrabEdit.Infrastructure.Address.Requests;
    using GeoJSON.Net.Geometry;
    using Requests;
    using TODO_MOVE_TO;
    using TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Crab.GeoJsonMapping;
    using TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Grar.Common;

    public class AddressCrabEditClient
    {
        private readonly CrabEditClient _client;
        private readonly GeoJsonMapper _geoJsonMapper;

        public AddressCrabEditClient(
            CrabEditClient crabEditClient,
            GeoJsonMapper geoJsonMapper)
        {
            _client = crabEditClient ?? throw new ArgumentNullException(nameof(crabEditClient));
            _geoJsonMapper = geoJsonMapper ?? throw new ArgumentNullException(nameof(geoJsonMapper));
        }

        public async Task<CrabEditClientResult<CrabHouseNumberId>> AddHouseNumberToCrab(
            AddHouseNumberRequest request,
            CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var position = request.Position;
            if (position == null)
                throw new ArgumentNullException(nameof(request));

            var streetNameId = int.Parse(request.StreetNameId.AsIdentifier().Value);
            var postalCode = request.PostalCode.AsIdentifier().Value;

            var addCrabHouseNumberResponse = await _client.Add(
                new AddHouseNumber
                {
                    StreetNameId = streetNameId,
                    HouseNumber = request.HouseNumber,
                    PostalCode = postalCode,
                    Status = request.AddressStatus.MapToCrabEditValue(),
                    OfficiallyAssigned = request.OfficiallyAssigned,
                    Position = new AddressPosition
                    {
                        Method = position.AddressPositionMethod.MapToCrabEditValue(),
                        Specification = position.AddressPositionSpecification.MapToCrabEditValue(),
                        Wkt = _geoJsonMapper.ToWkt(position.Point)
                    }
                },
                cancellationToken);

            //await _client.Delete(new RemoveHouseNumber { AddressId = addCrabHouseNumberResponse.AddressId }, cancellationToken);

            return CrabEditClientResult<CrabHouseNumberId>
                .From(
                    addCrabHouseNumberResponse,
                    identifier => new CrabHouseNumberId(addCrabHouseNumberResponse.AddressId));
        }
    }
}
