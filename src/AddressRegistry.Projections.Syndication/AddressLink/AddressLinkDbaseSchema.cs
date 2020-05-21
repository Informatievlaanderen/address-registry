namespace AddressRegistry.Projections.Syndication.AddressLink
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class AddressLinkDbaseSchema : DbaseSchema
    {
        public DbaseField objecttype => Fields[0];
        public DbaseField adresobjid => Fields[1];
        public DbaseField adresid => Fields[2];
        public DbaseField voladres => Fields[3];

        public AddressLinkDbaseSchema() => Fields = new[]
        {
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(objecttype)), new DbaseFieldLength(20)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(adresobjid)), new DbaseFieldLength(30)),
            DbaseField.CreateNumberField(new DbaseFieldName(nameof(adresid)), new DbaseFieldLength(DbaseInt32.MaximumIntegerDigits.ToInt32()), new DbaseDecimalCount(0)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(voladres)), new DbaseFieldLength(254))
        };
    }
}
