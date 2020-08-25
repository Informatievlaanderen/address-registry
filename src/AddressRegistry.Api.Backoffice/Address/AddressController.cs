namespace AddressRegistry.Api.Backoffice.Address
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using AddressRegistry.Address;
    using AddressRegistry.Address.Commands;
    using Be.Vlaanderen.Basisregisters.Api;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.CommandHandling;
    using CrabEdit.Infrastructure;
    using Infrastructure;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Projections.Legacy;
    using Requests;
    using SqlStreamStore;

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

        [HttpPost("")]
        public async Task<IActionResult> AddHouseNumber(
            [FromServices] ICommandHandlerResolver bus,
            [FromServices] AddressCrabEditClient editClient,
            [FromServices] Func<IAddresses> getAddresses,
            [FromBody] AddAddressRequest request,
            CancellationToken cancellationToken)
        {
            // TODO: Turn this into proper VBR API Validation
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var addHouseNumberResult = await editClient.AddHouseNumberToCrab(request, cancellationToken);
            var addressId = AddressId.CreateFor(addHouseNumberResult.Result);

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

            // Because we don't use the addressId as an identifier, we are stuck with the mess of retrieving our aggregate
            // and getting the surrogate identifier from it.... PersistentLocalIdentifier
            var addresses = getAddresses();

            var address = await addresses.GetOptionalAsync(addressId, cancellationToken);
            if (!address.HasValue)
                throw new ApiException("Er is een fout opgetreden.", StatusCodes.Status500InternalServerError);

            return CreatedWithPosition(
                $"/v1/adressen/{address.Value.PersistentLocalId}",
                position,
                addHouseNumberResult.ExecutionTime);
        }

        [HttpDelete("{lokaleIdentificator}")]
        public async Task<IActionResult> Delete(
            [FromServices] AddressCrabEditClient editClient,
            [FromServices] IStreamStore streamStore,
            [FromServices] LegacyContext context,
            [FromRoute] string lokaleIdentificator,
            CancellationToken cancellationToken)
        {
            // TODO: Turn this into proper VBR API Validation
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var persistentLocalId = int.Parse(lokaleIdentificator);

            // todo: should become position from bus.Dispatch
            var position = await streamStore.ReadHeadPosition(cancellationToken);

            var addressId = context
                .CrabIdToPersistentLocalIds
                .SingleOrDefault(item => item.PersistentLocalId == persistentLocalId);

            CrabEditResponse deleteResponse = addressId != null
                ? await editClient.Delete(addressId, cancellationToken)
                : CrabEditResponse.NothingExecuted;

            return AcceptedWithPosition(
                position,
                deleteResponse.ExecutionTime);
        }


        // TODO: For updates, we do insert into Operations - Guid + Position + Url Placeholder
        // New Guid, Last observed position,
        // If you request /operaties/guid/ and it is imported/ 200 ok + location where to find it
        // If you request /operaties/guid/ and it is not imported - 412 etag stuff
        //return Accepted($"/v1/operaties/{position}");
    }
}
