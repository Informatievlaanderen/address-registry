namespace AddressRegistry.StreetName.Commands
{
    using System;
    using System.Collections.Generic;
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using Be.Vlaanderen.Basisregisters.Utilities;

    public class CorrectAddressPosition : IHasCommandProvenance
    {
        private static readonly Guid Namespace = new Guid("02ecb2cc-126c-4209-9798-4d1e77c0cad6");

        public StreetNamePersistentLocalId StreetNamePersistentLocalId { get; }
        public AddressPersistentLocalId AddressPersistentLocalId { get; }
        public GeometryMethod GeometryMethod { get; }
        public GeometrySpecification? GeometrySpecification { get; }
        public ExtendedWkbGeometry? Position { get; }
        public Provenance Provenance { get; }

        public CorrectAddressPosition(
            StreetNamePersistentLocalId streetNamePersistentLocalId,
            AddressPersistentLocalId addressPersistentLocalId,
            GeometryMethod geometryMethod,
            GeometrySpecification? geometrySpecification,
            ExtendedWkbGeometry? position,
            Provenance provenance)
        {
            StreetNamePersistentLocalId = streetNamePersistentLocalId;
            AddressPersistentLocalId = addressPersistentLocalId;
            GeometryMethod = geometryMethod;
            GeometrySpecification = geometrySpecification;
            Position = position;
            Provenance = provenance;
        }

        public Guid CreateCommandId()
            => Deterministic.Create(Namespace, $"CorrectAddressPosition-{ToString()}");

        public override string? ToString()
            => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return StreetNamePersistentLocalId;
            yield return AddressPersistentLocalId;
            yield return GeometryMethod;
            yield return GeometrySpecification?.ToString() ?? string.Empty;
            yield return Position ?? string.Empty;

            foreach (var field in Provenance.GetIdentityFields())
            {
                yield return field;
            }
        }
    }
}
