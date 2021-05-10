namespace AddressRegistry.Address
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using NodaTime;

    public class AddressPersistentLocalIdentifierProvenanceFactory : IProvenanceFactory<Address>
    {
        private static readonly List<Type> AllowedTypes = new List<Type>
        {
            typeof(Commands.Crab.RequestPersistentLocalIdForCrabHouseNumberId),
            typeof(Commands.Crab.RequestPersistentLocalIdForCrabSubaddressId),
            typeof(Commands.Crab.AssignPersistentLocalIdForCrabHouseNumberId),
            typeof(Commands.Crab.AssignPersistentLocalIdForCrabSubaddressId),
        };

        private bool CanCreateFrom(Type? type) =>  type != null && AllowedTypes.Any(t => t == type);

        public bool CanCreateFrom<TCommand>() =>CanCreateFrom(typeof(TCommand));

        public Provenance CreateFrom(
            object command,
            Address aggregate)
        {
            if (CanCreateFrom(command?.GetType()))
                return new Provenance(
                    SystemClock.Instance.GetCurrentInstant(),
                    Application.AddressRegistry,
                    new Reason("Stabiele en unieke objectidentificatie"),
                    new Operator("AddressRegistry"),
                    Modification.Update,
                    Organisation.DigitaalVlaanderen);

            throw new ApplicationException($"Cannot create provenance for {command.GetType().Name}");
        }
    }
}
