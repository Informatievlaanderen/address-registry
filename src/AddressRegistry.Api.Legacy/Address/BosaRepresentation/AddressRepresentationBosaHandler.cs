namespace AddressRegistry.Api.Legacy.Address.BosaRepresentation
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using FluentValidation;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;
    using Projections.Syndication;

    public sealed class AddressRepresentationBosaHandler : IRequestHandler<AddressRepresentationBosaRequest, AddressRepresentationBosaResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly SyndicationContext _syndicationContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public AddressRepresentationBosaHandler(
            LegacyContext legacyContext,
            SyndicationContext syndicationContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _legacyContext = legacyContext;
            _syndicationContext = syndicationContext;
            _responseOptions = responseOptions;
        }

        public async Task<AddressRepresentationBosaResponse> Handle(AddressRepresentationBosaRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request?.AdresCode?.ObjectId) || !int.TryParse(request.AdresCode.ObjectId, out var addressId))
            {
                throw new ValidationException("Valid objectId is required");
            }

            var address = await _legacyContext
                .AddressDetail
                .FirstOrDefaultAsync(x => x.PersistentLocalId == addressId, cancellationToken);

            if (address == null)
            {
                throw new ApiException("Onbestaand adres.", StatusCodes.Status404NotFound);
            }

            var streetName = await _syndicationContext
                .StreetNameBosaItems
                .FirstOrDefaultAsync(x => x.StreetNameId == address.StreetNameId, cancellationToken);

            var municipality = await _syndicationContext
                .MunicipalityBosaItems
                .FirstOrDefaultAsync(x => x.NisCode == streetName.NisCode, cancellationToken);

            var response = new AddressRepresentationBosaResponse
            {
                Identificator = new AdresIdentificator(_responseOptions.Value.Naamruimte, address.PersistentLocalId.ToString(), address.VersionTimestamp.ToBelgianDateTimeOffset())
            };

            if (!request.Taal.HasValue || request.Taal.Value == municipality.PrimaryLanguage)
            {
                response.AdresVoorstellingen = new List<AddressRepresentationBosa>
                {
                    new AddressRepresentationBosa(
                        municipality.PrimaryLanguage.Value,
                        address.HouseNumber,
                        address.BoxNumber,
                        AddressMapper.GetVolledigAdres(address.HouseNumber, address.BoxNumber, address.PostalCode, streetName, municipality).GeografischeNaam.Spelling,
                        AddressMapper.GetDefaultMunicipalityName(municipality).Value,
                        AddressMapper.GetDefaultStreetNameName(streetName, municipality.PrimaryLanguage).Value,
                        address.PostalCode)
                };
            }

            return response;
        }
    }
}
