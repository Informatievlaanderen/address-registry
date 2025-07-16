namespace AddressRegistry.Api.Oslo.Address.Count
{
    using System.Threading;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Infrastructure.Elastic.List;
    using MediatR;

    public sealed class AddressCountElasticHandler : IRequestHandler<AddressCountRequest, TotaalAantalResponse>
    {
        private readonly IAddressApiListElasticsearchClient _addressApiListElasticsearchClient;

        public AddressCountElasticHandler(IAddressApiListElasticsearchClient addressApiListElasticsearchClient)
        {
            _addressApiListElasticsearchClient = addressApiListElasticsearchClient;
        }

        public async Task<TotaalAantalResponse> Handle(AddressCountRequest request, CancellationToken cancellationToken)
        {
            var filtering = request.Filtering;

            var addressCountResult = await _addressApiListElasticsearchClient.CountAddresses(
                filtering.Filter?.StreetNameId,
                filtering.Filter?.StreetName,
                filtering.Filter?.HomonymAddition,
                filtering.Filter?.HouseNumber,
                filtering.Filter?.BoxNumber,
                filtering.Filter?.PostalCode,
                filtering.Filter?.NisCode,
                filtering.Filter?.MunicipalityName,
                filtering.Filter?.Status);

            return new TotaalAantalResponse
            {
                Aantal = (int)addressCountResult
            };
        }
    }
}
