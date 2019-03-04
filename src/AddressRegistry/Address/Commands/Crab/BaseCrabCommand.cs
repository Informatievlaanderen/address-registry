namespace AddressRegistry.Address.Commands.Crab
{
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using System;

    public abstract class BaseCrabCommand : IHasCrabProvenance
    {
        public CrabLifetime Lifetime { get; protected set; }
        public CrabTimestamp Timestamp { get; protected set; }
        public CrabOperator Operator { get; protected set; }
        public CrabModification? Modification { get; protected set; }
        public CrabOrganisation? Organisation { get; protected set; }

        public abstract Guid CreateCommandId();
    }
}
