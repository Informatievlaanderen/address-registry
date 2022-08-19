namespace AddressRegistry.StreetName
{
    using DataStructures;

    public interface IMunicipalities
    {
        MunicipalityData Get(MunicipalityId municipalityId);
    }
}
