namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    /// <summary>
    /// Transforms every position the street name holds to Lambert 2008 (EPSG 3812), see ADR 0004. One command per
    /// street name stream: the transformation has nothing to decide per address, and batching it keeps the number
    /// of commands to the number of streams.
    /// </summary>
    public class TransformToLambert2008 : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("2f0a1a2e-8a95-4c39-9d0a-5f6a3b4c7d18");

        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public Provenance Provenance { get; }

        public TransformToLambert2008(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            Provenance provenance)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"TransformToLambert2008-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNamePersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
