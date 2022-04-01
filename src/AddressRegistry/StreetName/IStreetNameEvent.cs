namespace AddressRegistry.StreetName
{
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public interface IStreetNameEvent : IHasStreetNamePersistentLocalId, IHasProvenance, ISetProvenance, IHaveHash
    { }
}
