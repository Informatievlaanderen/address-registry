namespace AddressRegistry.Api.Oslo.Infrastructure.Options
{
    using Be.Vlaanderen.Basisregisters.GrAr.ChangeFeed;

    public class ResponseOptionsV3
    {
        public string VolgendeUrl { get; set; }
        public string DetailUrl { get; set; }
        public string PostInfoDetailUrl { get; set; }
        public string StraatnaamDetailUrl { get; set; }
        public string GemeenteDetailUrl { get; set; }
        public string ContextUrlList { get; set; }
        public string ContextUrlDetail { get; set; }
        public string ContextUrlAddressMatch { get; set; }
        public double SimilarityThreshold { get; set; }
        public int MaxStreetNamesThreshold { get; set; }

        public string AddressMatchParcelLink { get; set; }
        public string AddressMatchBuildingUnitLink { get; set; }

        public string AddressDetailBuildingUnitsLink { get; set; }
        public string AddressDetailParcelsLink { get; set; }

        public ChangeFeedConfig AddressFeed { get; set; }
    }
}
