namespace AddressRegistry.Api.BackOffice.Validators
{
    using System.Text.RegularExpressions;
    using Be.Vlaanderen.Basisregisters.Auth.AcmIdm;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using StreetName;

    public class BoxNumberValidator
    {
        private static readonly Regex DecentraleBijwerkerBoxNumberFormatRegex =
            new ("^(?!^[./]|.*[./]$)(?!.*[./]{2,})[a-zA-Z0-9./]{1,10}$", RegexOptions.Compiled);

        private readonly IActionContextAccessor _actionContextAccessor;

        public BoxNumberValidator(IActionContextAccessor actionContextAccessor)
        {
            _actionContextAccessor = actionContextAccessor;
        }

        public bool Validate(string boxNumber)
        {
            return _actionContextAccessor.ActionContext!.HttpContext.IsInterneBijwerker()
                ? BoxNumber.HasValidFormat(boxNumber)
                : BoxNumber.HasValidFormat(boxNumber, DecentraleBijwerkerBoxNumberFormatRegex);
        }
    }
}
