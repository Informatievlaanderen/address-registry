namespace AddressRegistry.Projections.Extract.AddressExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class AddressDbaseSchemaV2 : DbaseSchema
    {
        public DbaseField id => Fields[0];
        public DbaseField adresid => Fields[1];
        public DbaseField creatieid => Fields[2];
        public DbaseField versieid => Fields[3];
        public DbaseField posspec => Fields[4];
        public DbaseField posgeommet => Fields[5];
        public DbaseField straatnmid => Fields[6];
        public DbaseField straatnm => Fields[7];
        public DbaseField huisnr => Fields[8];
        public DbaseField busnr => Fields[9];
        public DbaseField postcode => Fields[10];
        public DbaseField gemeenteid => Fields[11];
        public DbaseField gemeentenm => Fields[12];
        public DbaseField status => Fields[13];
        public DbaseField offtoegknd => Fields[14];
        public DbaseField voladres => Fields[15];

        public AddressDbaseSchemaV2() => Fields = new[]
        {
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(id)), new DbaseFieldLength(50)),
            DbaseField.CreateNumberField(new DbaseFieldName(nameof(adresid)), new DbaseFieldLength(10), new DbaseDecimalCount(0)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(creatieid)), new DbaseFieldLength(25)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(versieid)), new DbaseFieldLength(25)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(posspec)), new DbaseFieldLength(20)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(posgeommet)), new DbaseFieldLength(30)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(straatnmid)), new DbaseFieldLength(10)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(straatnm)), new DbaseFieldLength(80)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(huisnr)), new DbaseFieldLength(11)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(busnr)), new DbaseFieldLength(35)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(postcode)), new DbaseFieldLength(4)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(gemeenteid)), new DbaseFieldLength(5)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(gemeentenm)), new DbaseFieldLength(40)),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(status)), new DbaseFieldLength(20)),
            DbaseField.CreateLogicalField(new DbaseFieldName(nameof(offtoegknd))),
            DbaseField.CreateCharacterField(new DbaseFieldName(nameof(voladres)), new DbaseFieldLength(254))
        };
    }
}
