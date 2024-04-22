namespace AddressRegistry.Projections.Extract.PostalCodeStreetNameExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class PostalCodeStreetNameExtractDbaseRecord : DbaseRecord
    {
        public static readonly PostalCodeStreetNameExtractDbaseSchema Schema = new PostalCodeStreetNameExtractDbaseSchema();

        public DbaseCharacter postcode { get; }
        public DbaseCharacter straatnmid { get; }

        public PostalCodeStreetNameExtractDbaseRecord()
        {
            postcode = new DbaseCharacter(Schema.postcode);
            straatnmid = new DbaseCharacter(Schema.straatnmid);

            Values = new DbaseFieldValue[]
            {
                postcode,
                straatnmid
            };
        }
    }
}
