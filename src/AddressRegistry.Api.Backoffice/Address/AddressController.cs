namespace AddressRegistry.Api.Backoffice.Address
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api;
    using CrabEdit.Client;
    using CrabEdit.Client.Contexts.Address;
    using CrabEdit.Client.Contexts.Address.Requests;
    using Microsoft.AspNetCore.Mvc;

    [ApiVersion("1.0")]
    [AdvertiseApiVersions("1.0")]
    [ApiRoute("adressen")]
    [ApiExplorerSettings(GroupName = "Adressen")]
    public class AddressController : ApiController
    {
        [HttpPost("huisnummer")]
        public async Task<IActionResult> AddHouseNumber(
            [FromServices] CrabEditClient editClient,
            //[FromBody] AddHouseNumberRequest addHouseNumber,
            CancellationToken cancellationToken)
        {
            // todo, don't hard-code this
            var addHouseNumber = new AddHouseNumber
            {
                StreetNameId = 1,
                HouseNumber = "5x",
                PostalCode = "2630",
                Status = AddressStatus.InUse,
                OfficiallyAssigned = true,
                Position = new AddressPosition
                {
                    Method = AddressPositionMethod.AppointedByAdministrator,
                    Specification = AddressPositionSpecification.Lot,
                    Wkt = "POINT (150647.25 200188.74)"
                }
            };

            var crabAddressId = await editClient.Add(addHouseNumber, cancellationToken);
            await editClient.Delete(new RemoveHouseNumber { AddressId = crabAddressId }, cancellationToken);

            return new OkObjectResult("Congrats, we made you think you added a house number.");
        }
    }
}
