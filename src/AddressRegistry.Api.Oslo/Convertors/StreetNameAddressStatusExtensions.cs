namespace AddressRegistry.Api.Oslo.Convertors
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using StreetName;

    public static class StreetNameAddressStatusExtensions
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

                case AdresStatus.Afgekeurd:
                    return AddressStatus.Rejected;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
}
