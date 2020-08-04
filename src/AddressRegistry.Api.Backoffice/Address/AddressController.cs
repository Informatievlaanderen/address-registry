namespace AddressRegistry.Api.Backoffice.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Address.Commands;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using Infrastructure;
    using Microsoft.AspNetCore.Mvc;
    using Requests;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("adressen")]
    [ApiExplorerSettings(GroupName = "Adressen")]
    public class AddressController : EditApiController
    {
        /* Post-huisnummer example:
{
    "heeftStraatnaam": "https://data.vlaanderen.be/id/straatnaam/3",
    "huisnummer": "2C",
    "busnummer": null,
    "heeftPostinfo": "https://data.vlaanderen.be/id/postinfo/2630",
    "status": "https://data.vlaanderen.be/id/concept/adresstatus/inGebruik",
    "officieelToegekend": true,
    "positie": {
        "methode": "https://data.vlaanderen.be/id/concept/geometriemethode/aangeduidDoorBeheerder",
        "specificatie": "https://data.vlaanderen.be/id/concept/geometriespecificatie/lot",
        "geometrie": {
            "coordinates": [150647.25, 200188.74],
            "type": "Point"
        }
    }
}
         */

        [HttpPost("huisnummer")]
        public async Task<IActionResult> AddHouseNumber(
            [FromServices] ICommandHandlerResolver bus,
            [FromServices] AddressCrabEditClient editClient,
            [FromBody] AddHouseNumberRequest request,
            CancellationToken cancellationToken)
        {
            // TODO: Turn this into proper VBR API Validation
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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
