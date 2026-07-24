namespace AddressRegistry.Api.Oslo.Address.V3.Count
{
    using Be.Vlaanderen.Basisregisters.GrAr.Oslo;
    using Swashbuckle.AspNetCore.Filters;

    public class TotalCountOsloResponseExample : IExamplesProvider<TotaalAantalResponse>
    {
        public TotaalAantalResponse GetExamples()
        {
            return new()
            {
                Aantal = 574512
            };
        }
    }
}
