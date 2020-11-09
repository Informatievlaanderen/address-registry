namespace AddressRegistry.Api.Legacy.Convertors
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using System;

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

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
}
