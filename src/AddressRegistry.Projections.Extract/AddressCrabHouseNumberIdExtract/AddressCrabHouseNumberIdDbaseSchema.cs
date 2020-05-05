namespace AddressRegistry.Projections.Extract.AddressCrabHouseNumberIdExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class AddressCrabHouseNumberIdDbaseSchema : DbaseSchema
    {
        public DbaseField adresid => Fields[0];
        public DbaseField crabhnrid => Fields[1];
        public DbaseField isvolledig => Fields[2];

        public AddressCrabHouseNumberIdDbaseSchema() => Fields = new[]
        {
            DbaseField.CreateNumberField(new DbaseFieldName(nameof(adresid)), new DbaseFieldLength(10), new DbaseDecimalCount(0)),
            DbaseField.CreateNumberField(new DbaseFieldName(nameof(crabhnrid)), new DbaseFieldLength(12), new DbaseDecimalCount(0)),
            DbaseField.CreateLogicalField(new DbaseFieldName(nameof(isvolledig)))
        };
    }
}
