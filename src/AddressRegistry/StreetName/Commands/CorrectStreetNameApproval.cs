namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class CorrectStreetNameApproval : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("27ed1dc4-280d-4495-9f6e-894b722624bc");

        public StreetNamePersistentLocalId PersistentLocalId { get; }
        public Provenance Provenance { get; }

        public CorrectStreetNameApproval(
            StreetNamePersistentLocalId persistentLocalId,
            Provenance provenance)
        {
            PersistentLocalId = persistentLocalId;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CorrectStreetNameApproval-{ToString()}");

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
