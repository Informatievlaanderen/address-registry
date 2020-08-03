namespace AddressRegistry.Address
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using System;

    public class CrabAddressProvenanceFactory : CrabProvenanceFactory, IProvenanceFactory<Address>
    {
        public bool CanCreateFrom<TCommand>() => typeof(IHasCrabProvenance).IsAssignableFrom(typeof(TCommand));

        public Provenance CreateFrom(
            object provenanceHolder,
            Address aggregate)
        {
            if (!(provenanceHolder is IHasCrabProvenance crabProvenance))
                throw new ApplicationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");

            return CreateFrom(
                aggregate.LastModificationBasedOnCrab,
                crabProvenance.Timestamp,
                crabProvenance.Modification,
                crabProvenance.Operator,
                crabProvenance.Organisation);
        }
    }
}
