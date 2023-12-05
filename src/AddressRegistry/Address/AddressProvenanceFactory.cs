namespace AddressRegistry.Address
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using NodaTime;

    [Obsolete("This is a legacy class and should not be used anymore.")]
    public class AddressProvenanceFactory : IProvenanceFactory<Address>
    {
        // TODO: Do we introduce an IHasProvenance?
        public bool CanCreateFrom<TCommand>() => true;

        public Provenance CreateFrom(
            object provenanceHolder,
            Address aggregate)
        {
#if DEBUG
            // Todo: this should be a debug-config flag, so you can run development without GeoSecure
            return new Provenance(
                SystemClock.Instance.GetCurrentInstant(),
                Application.Unknown,
                new Reason("DUMMY PROVENANCE"),
                new Operator($"DUMMY OPERATOR: {Environment.MachineName}"),
                Modification.Unknown,
                Organisation.Unknown);
#endif

            // TODO: Get from Geosecure
            throw new NotImplementedException($"{nameof(AddressProvenanceFactory)}.{nameof(CreateFrom)}: Creating provenance from GeoSecure is not implemented yet");

        }
    }
}
