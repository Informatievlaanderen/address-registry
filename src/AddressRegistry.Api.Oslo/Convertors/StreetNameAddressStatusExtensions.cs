namespace AddressRegistry.Api.Oslo.Convertors
{
    using System;
    using StreetName;

    public static class StreetNameAddressStatusExtensions
    {
        public static Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres.AdresStatus? ConvertFromAddressStatus(this AddressStatus? status)
            => status == null ? (Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres.AdresStatus?)null : ConvertFromAddressStatus(status.Value);

        public static Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres.AdresStatusValue? ConvertOsloFromAddressStatus(this AddressStatus? status)
            => status == null ? (Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres.AdresStatusValue?)null : ConvertOsloFromAddressStatus(status.Value);

        public static AddressStatus ConvertFromAdresStatus(this Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres.AdresStatus status)
        {
            switch (status)
            {
                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres.AdresStatus.Voorgesteld:
                    return AddressStatus.Proposed;

                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres.AdresStatus.InGebruik:
                    return AddressStatus.Current;

                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres.AdresStatus.Gehistoreerd:
                    return AddressStatus.Retired;

                case Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres.AdresStatus.Afgekeurd:
                    return AddressStatus.Rejected;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public static AddressStatus ConvertFromAdresStatus(this Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres.AdresStatusValue status)
        {
            switch (status)
            {
                case Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres.AdresStatusValue.Voorgesteld:
                    return AddressStatus.Proposed;

                case Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres.AdresStatusValue.InGebruik:
                    return AddressStatus.Current;

                case Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres.AdresStatusValue.Gehistoreerd:
                    return AddressStatus.Retired;

                case Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres.AdresStatusValue.Afgekeurd:
                    return AddressStatus.Rejected;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public static Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres.AdresStatus ConvertFromAddressStatus(this AddressStatus status)
        {
            switch (status)
            {
                case AddressStatus.Proposed:
                    return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres.AdresStatus.Voorgesteld;

                case AddressStatus.Current:
                    return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres.AdresStatus.InGebruik;

                case AddressStatus.Retired:
                    return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres.AdresStatus.Gehistoreerd;

                case AddressStatus.Rejected:
                    return Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres.AdresStatus.Afgekeurd;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }

        public static Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres.AdresStatusValue ConvertOsloFromAddressStatus(this AddressStatus status)
        {
            switch (status)
            {
                case AddressStatus.Proposed:
                    return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres.AdresStatusValue.Voorgesteld;

                case AddressStatus.Current:
                    return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres.AdresStatusValue.InGebruik;

                case AddressStatus.Retired:
                    return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres.AdresStatusValue.Gehistoreerd;

                case AddressStatus.Rejected:
                    return Be.Vlaanderen.Basisregisters.GrAr.Oslo.Adres.AdresStatusValue.Afgekeurd;

                default:
                    throw new ArgumentOutOfRangeException(nameof(status), status, null);
            }
        }
    }
}
