namespace AddressRegistry.Projections.Feed.Contract
{
    public static class AddressEventTypes
    {
        public const string CreateV1 = "basisregisters.address.create.v1";
        public const string UpdateV1 = "basisregisters.address.update.v1";
        public const string DeleteV1 = "basisregisters.address.delete.v1";
        public const string TransformV1 = "basisregisters.address.transform.v1";
    }

    public static class AddressAttributeNames
    {
        public const string StreetNameId = "straatnaam.id";
        public const string StatusName = "adresStatus";
        public const string HouseNumber = "huisnummer";
        public const string BoxNumber = "busnummer";
        public const string PostalCode = "postcode";
        public const string Position = "adresPositie";
        public const string PositionGeometryMethod = "positieGeometrieMethode";
        public const string PositionSpecification = "positieSpecificatie";
        public const string OfficiallyAssigned = "officieelToegekend";
    }
}
