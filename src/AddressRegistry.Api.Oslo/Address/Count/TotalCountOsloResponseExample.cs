namespace AddressRegistry.Api.Oslo.Address.Count
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
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
