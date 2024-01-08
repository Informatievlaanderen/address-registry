namespace AddressRegistry.Api.BackOffice.Validators
{
    using System.Text.RegularExpressions;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using StreetName;

    public class HouseNumberValidator
    {
        private static readonly Regex DecentraleBijwerkerHouseNumberFormatRegex =
            new("^[1-9]([0-9]{0,8}([A-H]|[K-N]|[P]|[R-T]|[V-Z]){0,1}|[0-9]{0,9})$", RegexOptions.Compiled);

        private readonly IActionContextAccessor _actionContextAccessor;

        public HouseNumberValidator(IActionContextAccessor actionContextAccessor)
        {
            _actionContextAccessor = actionContextAccessor;
        }

        public bool Validate(string houseNumber)
        {
            return _actionContextAccessor.ActionContext!.HttpContext.IsInterneBijwerker()
                ? HouseNumber.HasValidFormat(houseNumber)
                : DecentraleBijwerkerHouseNumberFormatRegex.IsMatch(houseNumber);
        }
    }
}
