namespace AddressRegistry.Address
{
    using System;

    public enum AddressStatus
    {
        Unknown = 0,
        Proposed = 1,
        Current = 2,
        Retired = 3
    }
    
    public static class AddressStatusHelpers
    {
        public static StreetName.AddressStatus ToStreetNameAddressStatus(this AddressStatus status)
        {
            return status switch
            {
                AddressStatus.Current => StreetName.AddressStatus.Current,
                AddressStatus.Proposed => StreetName.AddressStatus.Proposed,
                AddressStatus.Retired => StreetName.AddressStatus.Retired,
                AddressStatus.Unknown => StreetName.AddressStatus.Unknown,
                _ => throw new ArgumentOutOfRangeException(nameof(status), status, $"Non existing status '{status}'.")
            };
        }
    }
}
