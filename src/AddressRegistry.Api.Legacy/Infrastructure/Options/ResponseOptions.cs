namespace AddressRegistry.Api.Legacy.Infrastructure.Options
{
    public class ResponseOptions
    {
        public string Naamruimte { get; set; }
        public string GemeenteNaamruimte { get; set; }
        public string StraatNaamNaamruimte { get; set; }
        public string PostInfoNaamruimte { get; set; }
        public string VolgendeUrl { get; set; }
        public string DetailUrl { get; set; }
        public string PostInfoDetailUrl { get; set; }
        public string StraatnaamDetailUrl { get; set; }
        public string GemeenteDetailUrl { get; set; }
        public string GebouweenheidDetailUrl { get; set; }
        public string PerceelDetailUrl { get; set; }
        public double SimilarityThreshold { get; set; }
        public int MaxStreetNamesThreshold { get; set; }
    }
}
