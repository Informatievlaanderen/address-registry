namespace AddressRegistry.Projections.Extract.PostalCodeStreetNameExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class PostalCodeStreetNameExtractDbaseSchema : DbaseSchema
    {
        public DbaseField postcode => Fields[0];
        public DbaseField straatnmid => Fields[1];

        public PostalCodeStreetNameExtractDbaseSchema() => Fields = new[]
        {
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(postcode)), new DbaseFieldLength(4)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(straatnmid)), new DbaseFieldLength(10))
        };
    }
}
