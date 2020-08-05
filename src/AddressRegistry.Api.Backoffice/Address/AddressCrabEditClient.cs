namespace AddressRegistry.Api.Backoffice.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Crab;
    using CrabEdit.Client;
    using CrabEdit.Client.Contexts.Address;
    using CrabEdit.Client.Contexts.Address.Requests;
    using Requests;
    using TODO_MOVE_TO;
    using TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Grar.Common;

    public class AddressCrabEditClient
    {
        private readonly CrabEditClient _client;

        public AddressCrabEditClient(CrabEditClient crabEditClient)
            => _client = crabEditClient ?? throw new ArgumentNullException(nameof(crabEditClient));

        public async Task<CrabEditClientResult<CrabHouseNumberId>> AddHouseNumberToCrab(
            AddHouseNumberRequest request,
            CancellationToken cancellationToken)
        {
            var streetNameId = int.Parse(request.StreetNameId.AsIdentifier().Value);
            var postalCode = request.PostalCode.AsIdentifier().Value;
            var coordinates = request.Position.Point.Coordinates;

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
                        Method = request.Position.AddressPositionMethod.MapToCrabEditValue(),
                        Specification = request.Position.AddressPositionSpecification.MapToCrabEditValue(),
                        Wkt = $"POINT ({coordinates.Longitude} {coordinates.Latitude})"
                    }
                },
                cancellationToken);

            //await _client.Delete(new RemoveHouseNumber { AddressId = crabHouseNumberId }, cancellationToken);

            return CrabEditClientResult<CrabHouseNumberId>
                .From(
                    addCrabHouseNumberResponse,
                    identifier => new CrabHouseNumberId(addCrabHouseNumberResponse.AddressId));
        }
    }
}
