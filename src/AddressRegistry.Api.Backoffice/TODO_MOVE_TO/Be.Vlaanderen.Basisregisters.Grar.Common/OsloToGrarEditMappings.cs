namespace AddressRegistry.Api.Backoffice.TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Grar.Common
{
    using System;
    using System.Collections.Generic;
    using global::Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using global::Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;

    public static class OsloToGrarEditMappings
    {
        public static AdresStatus AsAddressStatus(this IdentifierUri value)
            => Map(StatusMapping, value);

        public static PositieGeometrieMethode AsAddressPositionMethod(this IdentifierUri value)
            => Map(GeometryMethodMapping, value);

        public static PositieSpecificatie AsAddressPositionSpecification(this IdentifierUri value)
            => Map(PositionSpecificationMapping, value);

        private static TMapped Map<TValue, TMapped>(
            IDictionary<TValue, TMapped> mapping,
            TValue value)
            => mapping.ContainsKey(value)
                ? mapping[value]
                : throw new ArgumentException($"No mapping defined for value {value}");

        private static IDictionary<IdentifierUri, AdresStatus> StatusMapping
            => new Dictionary<IdentifierUri, AdresStatus>
            {
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/adresstatus/voorgesteld"), AdresStatus.Voorgesteld },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/adresstatus/inGebruik"), AdresStatus.InGebruik },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/adresstatus/gehistoreerd"), AdresStatus.Gehistoreerd }
            };

        private static IDictionary<IdentifierUri, PositieGeometrieMethode> GeometryMethodMapping
            => new Dictionary<IdentifierUri, PositieGeometrieMethode>
            {
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriemethode/aangeduidDoorBeheerder"), PositieGeometrieMethode.AangeduidDoorBeheerder },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriemethode/afgeleidVanObject"), PositieGeometrieMethode.AfgeleidVanObject },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriemethode/geinterpoleerd"), PositieGeometrieMethode.Geinterpoleerd }
            };

        private static IDictionary<IdentifierUri, PositieSpecificatie> PositionSpecificationMapping
            => new Dictionary<IdentifierUri, PositieSpecificatie>
            {
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/ligplaats"), PositieSpecificatie.Ligplaats },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/gemeente"), PositieSpecificatie.Gemeente },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/gebouw"), PositieSpecificatie.Gebouw },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/gebouweenheid"), PositieSpecificatie.Gebouweenheid },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/ingang"), PositieSpecificatie.Ingang },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/lot"), PositieSpecificatie.Lot},
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/perceel"), PositieSpecificatie.Perceel },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/straat"), PositieSpecificatie.Straat },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/standplaats"), PositieSpecificatie.Standplaats },
                { new IdentifierUri("https://data.vlaanderen.be/id/concept/geometriespecificatie/wegsegment"), PositieSpecificatie.Wegsegment }
            };
    }
}
