namespace AddressRegistry.Api.Backoffice.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Address.Commands;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;
    using Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using Requests;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("adressen")]
    [ApiExplorerSettings(GroupName = "Adressen")]
    public class AddressController : EditApiController
    {
        private static AddHouseNumberRequest DummyRequest
            => new AddHouseNumberRequest
            {
                StreetNameId = "https://data.vlaanderen.be/id/straatnaam/3",
                HouseNumber = "2C",
                PostalCode = "https://data.vlaanderen.be/id/postinfo/1005",
                Status = AdresStatus.InGebruik,
                OfficiallyAssigned = true,
                Position = new AddressPositionRequest
                {
                    Method = PositieGeometrieMethode.AangeduidDoorBeheerder,
                    Specification = PositieSpecificatie.Lot,
                    Point = new GeoJSONPoint { Coordinates = new [] { 150647.25, 200188.74 } }
                }
            };

        [HttpPost("huisnummer")]
        public async Task<IActionResult> AddHouseNumber(
            [FromServices] ICommandHandlerResolver bus,
            [FromServices] AddressCrabEditClient editClient,
            [FromBody] AddHouseNumberRequest request,
            CancellationToken cancellationToken)
        {
            // override request with dummy
            request = DummyRequest;

            var crabHouseNumberId = await editClient.AddHouseNumberToCrab(request, cancellationToken);
            var addressId = AddressId.CreateFor(crabHouseNumberId);

            var command = new RegisterAddress(
                addressId,
                StreetNameId.CreateForPersistentId(request.StreetNameId),
                PostalCode.CreateForPersistentId(request.PostalCode),
                new HouseNumber(request.HouseNumber),
                null);

            var position = await bus.Dispatch(
                Guid.NewGuid(),
                command,
                GetMetadata(),
                cancellationToken);

            // TODO: Insert into Operations - Guid + Position + Url Placeholder

            return Accepted($"/v1/operaties/{position}");
        }
    }
}
