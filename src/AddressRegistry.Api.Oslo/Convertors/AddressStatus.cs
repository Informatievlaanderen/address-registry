namespace AddressRegistry.Api.Oslo.Convertors
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using System;

    public static class AddressStatusExtensions
    {
        public static AddressStatus ConvertFromAdresStatus(this AdresStatus status)
        {
            switch (status)
            {
                case AdresStatus.Voorgesteld:
                    return AddressStatus.Proposed;

                case AdresStatus.InGebruik:
                    return AddressStatus.Current;

                case AdresStatus.Gehistoreerd:
                    return AddressStatus.Retired;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
}
