namespace AddressRegistry.Api.BackOffice.Validators
{
    using System.Text.RegularExpressions;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Microsoft.AspNetCore.Http;
    using StreetName;

    public class BoxNumberValidator
    {
        internal static readonly Regex DecentraleBijwerkerBoxNumberFormatRegex =
            new ("^(?!^[./]|.*[./]$)(?!.*[./]{2,})[a-zA-Z0-9./]{1,10}$", RegexOptions.Compiled);

        private readonly IHttpContextAccessor _httpContextAccessor;

        public BoxNumberValidator(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public bool Validate(string boxNumber)
        {
            return _httpContextAccessor.HttpContext!.IsInterneBijwerker()
                ? BoxNumber.HasValidFormat(boxNumber)
                : BoxNumber.HasValidFormat(boxNumber, DecentraleBijwerkerBoxNumberFormatRegex);
        }
    }
}
