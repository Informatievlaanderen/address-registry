namespace AddressRegistry.Api.Legacy.Convertors
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using AddressRegistry.Address;

    public static class AddressStatusExtensions
    {
        public static AdresStatus? ConvertFromAddressStatus(this AddressStatus? status)
            => status == null ? (AdresStatus?)null : ConvertFromAddressStatus(status.Value);

        public static AdresStatus ConvertFromAddressStatus(this AddressStatus status)
        {
            switch (status)
            {
                case AddressStatus.Proposed:
                    return AdresStatus.Voorgesteld;

                case AddressStatus.Current:
                    return AdresStatus.InGebruik;

                case AddressStatus.Retired:
                    return AdresStatus.Gehistoreerd;

                case AddressStatus.Rejected:
                    return AdresStatus.Afgekeurd;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

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
