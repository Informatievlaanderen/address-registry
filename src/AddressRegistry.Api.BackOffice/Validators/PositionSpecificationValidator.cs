namespace AddressRegistry.Api.BackOffice.Validators
{
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;

    public static class PositionSpecificationValidator
    {
        public static bool IsValidWhenAppointedByAdministrator(PositieSpecificatie specificatie)
        {
            return specificatie == PositieSpecificatie.Ingang ||
                   specificatie == PositieSpecificatie.Perceel ||
                   specificatie == PositieSpecificatie.Lot ||
                   specificatie == PositieSpecificatie.Standplaats ||
                   specificatie == PositieSpecificatie.Ligplaats;
        }

        public static bool IsValidWhenDerivedFromObject(PositieSpecificatie specificatie)
        {
            return specificatie == PositieSpecificatie.Perceel ||
                   specificatie == PositieSpecificatie.Gebouweenheid;
        }
    }
}
