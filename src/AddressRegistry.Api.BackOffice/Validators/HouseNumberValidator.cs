namespace AddressRegistry.Api.BackOffice.Validators
{
    using System.Text.RegularExpressions;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Microsoft.AspNetCore.Http;
    using StreetName;

    public class HouseNumberValidator
    {
        internal static readonly Regex DecentraleBijwerkerHouseNumberFormatRegex =
            new("^[1-9]([0-9]{0,8}([A-H]|[K-N]|[P]|[R-T]|[V-Z]){0,1}|[0-9]{0,9})$", RegexOptions.Compiled);

        private readonly IHttpContextAccessor _httpContextAccessor;

        public HouseNumberValidator(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool Validate(string houseNumber)
        {
            return _httpContextAccessor.HttpContext!.IsInterneBijwerker()
                ? HouseNumber.HasValidFormat(houseNumber)
                : DecentraleBijwerkerHouseNumberFormatRegex.IsMatch(houseNumber);
        }
    }
}
