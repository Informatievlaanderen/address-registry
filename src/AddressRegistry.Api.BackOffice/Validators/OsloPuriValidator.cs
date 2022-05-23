namespace AddressRegistry.Api.BackOffice.Validators
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.Oslo.Extensions;

    public static class OsloPuriValidator
    {
        public static bool TryParseIdentifier(string url, out string identifier)
        {
            identifier = string.Empty;
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    return false;
                }

                identifier = url
                    .AsIdentifier()
                    .Map(x => x);

                return true;
            }
            catch (UriFormatException)
            {
                return false;
            }
        }
    }
}
