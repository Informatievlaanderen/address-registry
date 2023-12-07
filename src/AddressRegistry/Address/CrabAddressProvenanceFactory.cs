namespace AddressRegistry.Address
{
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using System;

    [Obsolete("This is a legacy class and should not be used anymore.")]
    public class CrabAddressProvenanceFactory : CrabProvenanceFactory, IProvenanceFactory<Address>
    {
        public bool CanCreateFrom<TCommand>() => typeof(IHasCrabProvenance).IsAssignableFrom(typeof(TCommand));

        public Provenance CreateFrom(
            object provenanceHolder,
            Address aggregate)
        {
            if (!(provenanceHolder is IHasCrabProvenance crabProvenance))
            {
                throw new InvalidOperationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");
            }

            return CreateFrom(
                aggregate.LastModificationBasedOnCrab,
                crabProvenance.Timestamp,
                crabProvenance.Modification,
                crabProvenance.Operator,
                crabProvenance.Organisation);
        }
    }

    public class AddressLegacyProvenanceFactory : IProvenanceFactory<Address>
    {
        public bool CanCreateFrom<TCommand>() => typeof(IHasCommandProvenance).IsAssignableFrom(typeof(TCommand));

        public Provenance CreateFrom(
            object provenanceHolder,
            Address aggregate)
        {
            if (provenanceHolder is not IHasCommandProvenance provenance)
            {
                throw new InvalidOperationException($"Cannot create provenance from {provenanceHolder.GetType().Name}");
            }

            return provenance.Provenance;
        }
    }
}
