namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class ApproveStreetName : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("ce9ad37a-7548-48ed-81cd-85bffe41523c");

        public StreetNamePersistentLocalId PersistentLocalId { get; }
        public Provenance Provenance { get; }

        public ApproveStreetName(
            StreetNamePersistentLocalId persistentLocalId,
            Provenance provenance)
        {
            PersistentLocalId = persistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"ApproveStreetName-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return PersistentLocalId;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
