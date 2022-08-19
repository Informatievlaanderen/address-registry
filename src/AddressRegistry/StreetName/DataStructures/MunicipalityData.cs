namespace AddressRegistry.StreetName.DataStructures
{
    public class MunicipalityData
    {
        public MunicipalityId MunicipalityId { get; }
        public byte[]? ExtendedWkbGeometry { get; }

        public MunicipalityData(MunicipalityId municipalityId, byte[]? extendedWkbGeometry)
        {
            MunicipalityId = municipalityId;
            ExtendedWkbGeometry = extendedWkbGeometry;
        }

        public ExtendedWkbGeometry Centroid()
        {
            var municipalityGeometry =
                WKBReaderFactory.Create().Read(ExtendedWkbGeometry);

            return AddressRegistry.StreetName.ExtendedWkbGeometry.CreateEWkb(municipalityGeometry.Centroid.AsBinary());
        }
    }
}
