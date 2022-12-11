namespace AddressRegistry.Api.Legacy.Address.BosaRepresentation
{
    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.Exceptions;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Consumer.Read.Municipality;
    using Consumer.Read.StreetName;
    using Convertors;
    using FluentValidation;
    using Infrastructure.Options;
    using MediatR;
    using Microsoft.AspNetCore.Http;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using Projections.Legacy;

    public sealed class AddressRepresentationBosaHandlerV2 : IRequestHandler<AddressRepresentationBosaRequest, AddressRepresentationBosaResponse>
    {
        private readonly LegacyContext _legacyContext;
        private readonly MunicipalityConsumerContext _municipalityConsumerContext;
        private readonly StreetNameConsumerContext _streetNameConsumerContext;
        private readonly IOptions<ResponseOptions> _responseOptions;

        public AddressRepresentationBosaHandlerV2(
            LegacyContext legacyContext,
            MunicipalityConsumerContext municipalityConsumerContext,
            StreetNameConsumerContext streetNameConsumerContext,
            IOptions<ResponseOptions> responseOptions)
        {
            _legacyContext = legacyContext;
            _municipalityConsumerContext = municipalityConsumerContext;
            _streetNameConsumerContext = streetNameConsumerContext;
            _responseOptions = responseOptions;
        }

        public async Task<AddressRepresentationBosaResponse> Handle(AddressRepresentationBosaRequest request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request?.AdresCode?.ObjectId) || !int.TryParse(request.AdresCode.ObjectId, out var addressId))
            {
                throw new ValidationException("Valid objectId is required");
            }

            var addressV2 = await _legacyContext
                .AddressDetailV2
                .FirstOrDefaultAsync(x => x.AddressPersistentLocalId == addressId, cancellationToken);

            if (addressV2 == null)
            {
                throw new ApiException("Onbestaand adres.", StatusCodes.Status404NotFound);
            }

            var streetNameV2 = await _streetNameConsumerContext
                .StreetNameBosaItems
                .SingleAsync(x => x.PersistentLocalId == addressV2.StreetNamePersistentLocalId, cancellationToken);

            var municipalityV2 = await _municipalityConsumerContext
                .MunicipalityLatestItems
                .SingleAsync(x => x.NisCode == streetNameV2.NisCode, cancellationToken);

            var response = new AddressRepresentationBosaResponse
            {
                Identificator = new AdresIdentificator(_responseOptions.Value.Naamruimte, addressV2.AddressPersistentLocalId.ToString(), addressV2.VersionTimestamp.ToBelgianDateTimeOffset())
            };

            if (!request.Taal.HasValue || request.Taal.Value == municipalityV2.PrimaryLanguage.ToTaal())
            {
                response.AdresVoorstellingen = new List<AddressRepresentationBosa>
                    {
                        new AddressRepresentationBosa(
                            municipalityV2.PrimaryLanguage.ToTaal(),
                            addressV2.HouseNumber,
                            addressV2.BoxNumber,
                            AddressMapper.GetVolledigAdres(addressV2.HouseNumber, addressV2.BoxNumber, addressV2.PostalCode, streetNameV2, municipalityV2).GeografischeNaam.Spelling,
                            AddressMapper.GetDefaultMunicipalityName(municipalityV2).Value,
                            AddressMapper.GetDefaultStreetNameName(streetNameV2, municipalityV2.PrimaryLanguage).Value,
                            addressV2.PostalCode)
                    };
            }

            return response;
        }
    }
}
