namespace AddressRegistry.Projections.Syndication.Parcel
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Perceel;

    [DataContract(Name = "Perceel", Namespace = "")]
    public class Parcel
    {
        [DataMember(Name = "Id", Order = 1)]
        public Guid Id { get; set; }

        [DataMember(Name = "Identificator", Order = 2)]
        public PerceelIdentificator Identificator { get; set; }

        [DataMember(Name = "PerceelStatus", Order = 3)]
        public PerceelStatus? Status { get; set; }

        [DataMember(Name = "AdressenIds", Order = 4)]
        public List<Guid> AddressIds { get; set; }

        public Parcel()
        {
            AddressIds = new List<Guid>();
        }
    }
}
