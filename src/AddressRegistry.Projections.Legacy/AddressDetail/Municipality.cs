namespace AddressRegistry.Projections.Legacy.AddressDetail
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Gemeente;

    [DataContract(Name = "Gemeente", Namespace = "")]
    public class Municipality
    {
        [DataMember(Name = "Id", Order = 1)]
        public Guid Id { get; set; }

        [DataMember(Name = "Identificator", Order = 2)]
        public Identificator Identificator { get; set; }

        [DataMember(Name = "OfficieleTalen", Order = 3)]
        public List<Taal> OfficialLanguages { get; set; }

        [DataMember(Name = "FaciliteitenTalen", Order = 4)]
        public List<Taal> FacilitiesLanguages { get; set; }

        [DataMember(Name = "Gemeentenamen", Order = 5)]
        public List<GeografischeNaam> MunicipalityNames { get; set; }

        [DataMember(Name = "GemeenteStatus", Order = 6)]
        public GemeenteStatus? Status { get; set; }

        public Municipality()
        {
            MunicipalityNames = new List<GeografischeNaam>();
        }
    }
}
