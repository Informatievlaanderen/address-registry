namespace AddressRegistry.Projections.Syndication.BuildingUnit
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouw;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gebouweenheid;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.SpatialTools;

    [DataContract(Name = "Gebouw", Namespace = "")]
    public class Building
    {
        [DataMember(Name = "Id", Order = 1)]
        public Guid Id { get; set; }

        [DataMember(Name = "Identificator", Order = 2)]
        public Identificator Identificator { get; set; }

        [DataMember(Name = "GebouwStatus", Order = 3)]
        public GebouwStatus? Status { get; set; }

        [DataMember(Name = "GeometrieMethode", Order = 4)]
        public GeometrieMethode? GeometryMethod { get; set; }

        [DataMember(Name = "GeometriePolygoon", Order = 5)]
        public GmlPolygon Geometry { get; set; }

        [DataMember(Name = "Gebouweenheden", Order = 6)]
        public List<BuildingUnitSyndicationContent> BuildingUnits { get; set; }

        [DataMember(Name = "IsCompleet", Order = 8)]
        public bool IsComplete { get; set; }

        public Building()
        {
            BuildingUnits = new List<BuildingUnitSyndicationContent>();
        }
    }

    [DataContract(Name = "Gebouweenheid", Namespace = "")]
    public class BuildingUnitSyndicationContent
    {
        [DataMember(Name = "Id", Order = 1)]
        public Guid BuildingUnitId { get; set; }

        [DataMember(Name = "Identificator", Order = 2)]
        public Identificator Identificator { get; set; }

        [DataMember(Name = "GebouweenheidStatus", Order = 3)]
        public GebouweenheidStatus? Status { get; set; }

        [DataMember(Name = "PositieGeometrieMethode", Order = 4)]
        public PositieGeometrieMethode? GeometryMethod { get; set; }

        [DataMember(Name = "GeometriePunt", Order = 5)]
        public GmlPoint Geometry { get; set; }

        [DataMember(Name = "Functie", Order = 6)]
        public GebouweenheidFunctie? Function { get; set; }

        [DataMember(Name = "Addressen", Order = 7)]
        public List<Guid> Addresses { get; set; }

        [DataMember(Name = "IsCompleet", Order = 8)]
        public bool IsComplete { get; set; }
    }
}
