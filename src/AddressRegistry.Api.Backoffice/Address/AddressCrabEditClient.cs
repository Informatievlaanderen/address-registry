namespace AddressRegistry.Api.Backoffice.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;
    using CrabEdit.Client;
    using CrabEdit.Infrastructure;
    using CrabEdit.Infrastructure.Address;
    using CrabEdit.Infrastructure.Address.Requests;
    using Requests;
    using TODO_MOVE_TO;
    using TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Crab.GeoJsonMapping;

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

            var addCrabHouseNumberResponse = await _client.Add(
                new AddHouseNumber
                {
                    StreetNameId = request.StreetNameId.AsIdentifier().Map(int.Parse),
                    HouseNumber = request.HouseNumber,
                    PostalCode = request.PostalCode.AsIdentifier().Value,
                    Status = request.Status.AsIdentifier().Map(IdentifierMappings.AddressStatus),
                    OfficiallyAssigned = request.OfficiallyAssigned,
                    Position = new AddressPosition
                    {
                        Method = position.Method.AsIdentifier().Map(IdentifierMappings.PositionGeometryMethod),
                        Specification = position.Specification.AsIdentifier().Map(IdentifierMappings.PositionSpecification),
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
