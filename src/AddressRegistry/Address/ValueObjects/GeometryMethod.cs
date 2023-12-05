namespace AddressRegistry.Address
{
    using System;

    [Obsolete("This is a legacy valueobject and should not be used anymore.")]
    public enum GeometryMethod
    {
        AppointedByAdministrator = 1,
        DerivedFromObject = 2,
        Interpolated = 3
    }
}
