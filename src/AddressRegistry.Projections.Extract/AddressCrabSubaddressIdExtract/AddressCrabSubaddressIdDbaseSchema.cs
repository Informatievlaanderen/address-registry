namespace AddressRegistry.Projections.Extract.AddressCrabSubaddressIdExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class AddressCrabSubaddressIdDbaseSchema : DbaseSchema
    {
        public DbaseField adresid => Fields[0];
        public DbaseField crabsubid => Fields[1];
        public DbaseField isvolledig => Fields[2];

        public AddressCrabSubaddressIdDbaseSchema() => Fields = new[]
        {
            DbaseField.CreateNumberField(new DbaseFieldName(nameof(adresid)), new DbaseFieldLength(10), new DbaseDecimalCount(0)),
            DbaseField.CreateNumberField(new DbaseFieldName(nameof(crabsubid)), new DbaseFieldLength(12), new DbaseDecimalCount(0)),
            DbaseField.CreateLogicalField(new DbaseFieldName(nameof(isvolledig)))
        };
    }
}
