namespace AddressRegistry.Api.Backoffice.Address
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Crab;
    using CrabEdit.Client;
    using CrabEdit.Client.Contexts.Address;
    using CrabEdit.Client.Contexts.Address.Requests;
    using Requests;

    public class AddressCrabEditClient
    {
        private readonly CrabEditClient _client;

        public AddressCrabEditClient(CrabEditClient crabEditClient)
            => _client = crabEditClient ?? throw new ArgumentNullException(nameof(crabEditClient));

        public async Task<CrabHouseNumberId> AddHouseNumberToCrab(
            AddHouseNumberRequest request,
            CancellationToken cancellationToken)
        {
            // ToDo: refactor
            var streetNameId =
                int.Parse(request.StreetNameId.Replace("https://data.vlaanderen.be/id/straatnaam/", string.Empty));

            // ToDo: refactor
            var postalCode = request.PostalCode.Replace("https://data.vlaanderen.be/id/postinfo/", string.Empty);

            var crabHouseNumberId = await _client.Add(
                new AddHouseNumber
                {
                    StreetNameId = streetNameId,
                    HouseNumber = request.HouseNumber,
                    PostalCode = postalCode,
                    Status = request.Status.Map(),
                    OfficiallyAssigned = request.OfficiallyAssigned,
                    Position = new AddressPosition
                    {
                        Method = request.Position.Method.Map(),
                        Specification = request.Position.Specification.Map(),
                        Wkt = $"POINT ({string.Join(" ", request.Position.Point.Coordinates.Take(2))})"
                    }
                },
                cancellationToken);

            //await _client.Delete(new RemoveHouseNumber { AddressId = crabHouseNumberId }, cancellationToken);

            return new CrabHouseNumberId(crabHouseNumberId);
        }
    }
}
