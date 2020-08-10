namespace AddressRegistry.Api.Backoffice.TODO_MOVE_TO
{
    using Infrastructure.Mapping.CrabEdit;

    public static class IdentifierMappings
    {
        public static readonly IdentifierAddressStatusMapper AddressStatus = new IdentifierAddressStatusMapper();
        public static readonly IdentifierPositionGeometryMethodMapper PositionGeometryMethod = new IdentifierPositionGeometryMethodMapper();
        public static readonly IdentifierPositionSpecificationMapper PositionSpecification = new IdentifierPositionSpecificationMapper();
    }
}
