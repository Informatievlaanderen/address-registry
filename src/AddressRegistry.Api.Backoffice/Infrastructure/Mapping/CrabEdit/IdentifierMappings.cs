namespace AddressRegistry.Api.Backoffice.Infrastructure.Mapping.CrabEdit
{
    using System;

    public static class IdentifierMappings
    {
        public static readonly IdentifierAddressStatusMapper AddressStatus = new IdentifierAddressStatusMapper();
        public static readonly IdentifierPositionGeometryMethodMapper PositionGeometryMethod = new IdentifierPositionGeometryMethodMapper();
        public static readonly IdentifierPositionSpecificationMapper PositionSpecification = new IdentifierPositionSpecificationMapper();

        public static readonly Func<string, string> PostalCode = s => s;
        public static readonly Func<string, int> StreetNameId = int.Parse;
    }
}
