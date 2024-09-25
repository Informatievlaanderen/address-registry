namespace AddressRegistry.Api.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Abstractions;
    using Abstractions.Requests;
    using Abstractions.SqsRequests;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Consumer.Read.StreetName;
    using CsvHelper;
    using CsvHelper.Configuration;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.EntityFrameworkCore;
    using StreetName;
    using Swashbuckle.AspNetCore.Filters;

    public partial class AddressController
    {
        /// <summary>
        /// Stel een adres voor.
        /// Accept a csv file with addresses and their street names.
        /// </summary>
        /// <param name="file"></param>
        /// <param name="nisCode"></param>
        /// <param name="persistentLocalIdGenerator"></param>
        /// <param name="streetNameConsumerContext"></param>
        /// <param name="backOfficeContext"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        [HttpPost("acties/voorstellen/gemeentefusie/{niscode}")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
        [SwaggerRequestExample(typeof(ProposeAddressRequest), typeof(ProposeAddressRequestExamples))]
        [SwaggerResponseExample(StatusCodes.Status400BadRequest, typeof(BadRequestResponseExamples))]
        [SwaggerResponseExample(StatusCodes.Status500InternalServerError, typeof(InternalServerErrorResponseExamples))]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Policy = PolicyNames.Adres.InterneBijwerker)]
        public async Task<IActionResult> ProposeForMunicipalityMerger(
            IFormFile? file,
            [FromRoute(Name = "niscode")] string nisCode,
            [FromServices] IPersistentLocalIdGenerator persistentLocalIdGenerator,
            [FromServices] StreetNameConsumerContext streetNameConsumerContext,
            [FromServices] BackOfficeContext backOfficeContext,
            CancellationToken cancellationToken = default)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Please upload a CSV file.");

            if (!Path.GetExtension(file.FileName).Equals(".csv", StringComparison.CurrentCultureIgnoreCase))
                return BadRequest("Only CSV files are allowed.");

            var errorMessages = new List<string>();
            var records = new List<CsvRecord>();
            using (var stream = new MemoryStream())
            {
                await file.CopyToAsync(stream, cancellationToken);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                using (var csv = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
                       {
                           Delimiter = ";",
                           HasHeaderRecord = true,
                           IgnoreBlankLines = true
                       }))
                {
                    await csv.ReadAsync();
                    csv.ReadHeader();

                    var recordNr = 0;
                    while (await csv.ReadAsync())
                    {
                        recordNr++;

                        var recordErrorMessages = new List<string>();

                        var oldAddressId = csv.GetField<string>("OUD adresid")?.Trim();
                        var streetNameName = csv.GetField<string>("NIEUW straatnaam")?.Trim();
                        var streetNameHomonymAddition = csv.GetField<string?>("NIEUW homoniemtoevoeging")?.Trim();
                        var houseNumber = csv.GetField<string>("NIEUW huisnummer")?.Trim();
                        var boxNumber = csv.GetField<string?>("NIEUW busnummer")?.Trim();
                        var postalCode = csv.GetField<string>("NIEUW postcode")?.Trim();
                        csv.TryGetField<string>("Geen nummer validatie", out var disableValidationAsString);
                        var disableValidation = disableValidationAsString?.Trim().ToLower() == "x"
                                                || disableValidationAsString?.Trim().ToLower() == "true";

                        if (string.IsNullOrWhiteSpace(oldAddressId))
                            recordErrorMessages.Add($"OldAddressId is required at record number {recordNr}");

                        if (!string.IsNullOrWhiteSpace(oldAddressId)
                            & !int.TryParse(oldAddressId, out var oldAddressPersistentLocalId))
                            recordErrorMessages.Add($"OldAddressId is NaN at record number {recordNr}");

                        if (string.IsNullOrWhiteSpace(streetNameName))
                            recordErrorMessages.Add($"StreetNameName is required at record number {recordNr}");

                        if (string.IsNullOrWhiteSpace(houseNumber))
                        {
                            recordErrorMessages.Add($"HouseNumber is required at record number {recordNr}");
                        }
                        else if (!disableValidation && !HouseNumber.HasValidFormat(houseNumber))
                        {
                            recordErrorMessages.Add($"HouseNumber is invalid at record number {recordNr}");
                        }

                        if (!string.IsNullOrWhiteSpace(boxNumber) && !disableValidation && !BoxNumber.HasValidFormat(boxNumber))
                            recordErrorMessages.Add($"BoxNumber is invalid at record number {recordNr}");

                        if (string.IsNullOrWhiteSpace(postalCode))
                            recordErrorMessages.Add($"PostalCode is required at record number {recordNr}");

                        var relation = await backOfficeContext
                            .FindRelationAsync(new AddressPersistentLocalId(oldAddressPersistentLocalId), cancellationToken);

                        if (relation is null)
                            recordErrorMessages.Add($"No streetname relation found for oldAddressId {oldAddressId} at record number {recordNr}");

                        if (recordErrorMessages.Any())
                        {
                            errorMessages.AddRange(recordErrorMessages);
                            continue;
                        }

                        records.Add(new CsvRecord
                        {
                            RecordNumber = recordNr,
                            OldStreetNamePersistentLocalId = relation!.StreetNamePersistentLocalId,
                            OldAddressPersistentLocalId = oldAddressPersistentLocalId,
                            StreetNameName = streetNameName!,
                            StreetNameHomonymAddition =
                                string.IsNullOrWhiteSpace(streetNameHomonymAddition) ? null : streetNameHomonymAddition,
                            HouseNumber = houseNumber!,
                            BoxNumber = string.IsNullOrWhiteSpace(boxNumber) ? null : boxNumber,
                            PostalCode = postalCode!
                        });
                    }
                }
            }

            var nonUniqueOldAddressPersistentLocalIds = records
                .GroupBy(x => x.OldAddressPersistentLocalId)
                .Where(x => x.Count() > 1)
                .Select(x => x.Key)
                .ToList();
            foreach (var oldAddressPersistentLocalId in nonUniqueOldAddressPersistentLocalIds)
            {
                errorMessages.Add($"OldAddressPersistentLocalId {oldAddressPersistentLocalId} is not unique");
            }

            var addressesByStreetNameRecords = records
                .GroupBy(x => (x.StreetNameName, x.StreetNameHomonymAddition))
                .ToDictionary(
                    x => x.Key,
                    x => x.ToList());

            var sqsRequests = new List<ProposeAddressesForMunicipalityMergerSqsRequest>();

            foreach (var (streetName, addressRecords) in addressesByStreetNameRecords)
            {
                var (streetNameName, streetNameHomonymAddition) = streetName;

                var houseNumberAddressRecords = addressRecords.Where(x => x.BoxNumber is null).ToList();
                var nonUniqueHouseNumberAddressRecords = houseNumberAddressRecords
                    .GroupBy(x => x.HouseNumber)
                    .Where(x => x.Count() > 1)
                    .Select(x => x.First())
                    .ToList();
                foreach(var addressRecord in nonUniqueHouseNumberAddressRecords)
                {
                    errorMessages.Add($"House number '{addressRecord.HouseNumber}' is not unique for street (Name={streetNameName}, HomonymAddition={streetNameHomonymAddition})");
                }

                var boxNumberAddressRecords = addressRecords.Where(x => x.BoxNumber is not null).ToList();
                var nonUniqueBoxNumberAddressRecords = boxNumberAddressRecords
                    .GroupBy(x => new { x.HouseNumber, x.BoxNumber })
                    .Where(x => x.Count() > 1)
                    .Select(x => x.First())
                    .ToList();
                foreach(var addressRecord in nonUniqueBoxNumberAddressRecords)
                {
                    errorMessages.Add($"Box number '{addressRecord.BoxNumber}' is not unique for street (Name={streetNameName}, HomonymAddition={streetNameHomonymAddition}) and house number '{addressRecord.HouseNumber}'");
                }

                foreach (var boxNumberAddressRecord in boxNumberAddressRecords)
                {
                    if (houseNumberAddressRecords.All(x => x.HouseNumber != boxNumberAddressRecord.HouseNumber))
                    {
                        errorMessages.Add($"Box number '{boxNumberAddressRecord.BoxNumber}' does not have a corresponding house number '{boxNumberAddressRecord.HouseNumber}' for street '{streetNameName}' at record number {boxNumberAddressRecord.RecordNumber}");
                    }
                }

                var streetNameLatestItem = await streetNameConsumerContext.StreetNameLatestItems.SingleOrDefaultAsync(
                    x =>
                        // String comparisons translate to case-insensitive checks on SQL (=desired behavior)
                        x.NisCode == nisCode
                        && (x.NameDutch == streetNameName || x.NameDutch == $"'{streetNameName}")
                        && x.HomonymAdditionDutch == streetNameHomonymAddition
                        && !x.IsRemoved,
                    cancellationToken: cancellationToken);

                if (streetNameLatestItem is null)
                {
                    errorMessages.Add($"No streetNameLatestItem found for NisCode {nisCode} and street (Name={streetNameName}, HomonymAddition={streetNameHomonymAddition})");
                    continue;
                }

                sqsRequests.Add(new ProposeAddressesForMunicipalityMergerSqsRequest(
                    streetNameLatestItem.PersistentLocalId,
                    addressRecords.Select(x => new ProposeAddressesForMunicipalityMergerSqsRequestItem(
                        x.PostalCode,
                        persistentLocalIdGenerator.GenerateNextPersistentLocalId(),
                        x.HouseNumber,
                        x.BoxNumber,
                        x.OldStreetNamePersistentLocalId,
                        x.OldAddressPersistentLocalId)).ToList(),
                    new ProvenanceData(CreateProvenance(Modification.Insert, $"Fusie {nisCode}"))));
            }

            if (errorMessages.Any())
            {
                return BadRequest(errorMessages);
            }

            var results = await Task.WhenAll(sqsRequests.Select(sqsRequest => _mediator.Send(sqsRequest, cancellationToken)));

            return Ok(results.Select(x =>
                x.Location
                    .ToString()
                    .Replace(_ticketingOptions.InternalBaseUrl, _ticketingOptions.PublicBaseUrl)));
        }
    }

    public sealed class CsvRecord
    {
        public required int RecordNumber { get; init; }
        public required int OldStreetNamePersistentLocalId { get; init; }
        public required int OldAddressPersistentLocalId { get; init; }
        public required string StreetNameName { get; init; }
        public required string? StreetNameHomonymAddition { get; init; }
        public required string HouseNumber { get; init; }
        public required string? BoxNumber { get; init; }
        public required string PostalCode { get; init; }
    }
}
