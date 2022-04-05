namespace AddressRegistry.Address.Commands
{
    using Be.Vlaanderen.Basisregisters.Generators.Guid;
    using Be.Vlaanderen.Basisregisters.Utilities;
    using System;
    using System.Collections.Generic;
    using ValueObjects;

    public class RegisterAddress
    {
        private static readonly Guid Namespace = new Guid("67f3e429-515e-47e1-bf44-d6c6970b88e7");

        public AddressId AddressId { get; }

        public StreetNameId StreetNameId { get; }

        public PostalCode PostalCode { get; }

        public HouseNumber HouseNumber { get; }

        public BoxNumber BoxNumber { get; }

        public RegisterAddress(
            AddressId addressId,
            StreetNameId streetNameId,
            PostalCode postalCode,
            HouseNumber houseNumber,
            BoxNumber boxNumber)
        {
            AddressId = addressId;
            StreetNameId = streetNameId;
            PostalCode = postalCode;
            HouseNumber = houseNumber;
            BoxNumber = boxNumber;
        }

        public Guid CreateCommandId() => Deterministic.Create(Namespace, $"RegisterAddress-{ToString()}");

        public override string ToString() => ToStringBuilder.ToString(IdentityFields());

        private IEnumerable<object> IdentityFields()
        {
            yield return AddressId;
            yield return StreetNameId;
            yield return PostalCode;
            yield return HouseNumber;
            yield return BoxNumber;
        }
    }
}
