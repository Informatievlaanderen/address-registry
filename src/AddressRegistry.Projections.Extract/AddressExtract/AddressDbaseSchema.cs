namespace AddressRegistry.Projections.Extract.AddressExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class AddressDbaseSchema : DbaseSchema
    {
        public DbaseField id => Fields[0];
        public DbaseField adresid => Fields[1];
        public DbaseField versieid => Fields[2];
        public DbaseField posspec => Fields[3];
        public DbaseField posgeommet => Fields[4];
        public DbaseField straatnmid => Fields[5];
        public DbaseField straatnm => Fields[6];
        public DbaseField huisnr => Fields[7];
        public DbaseField busnr => Fields[8];
        public DbaseField postcode => Fields[9];
        public DbaseField gemeentenm => Fields[10];
        public DbaseField status => Fields[11];
        public DbaseField offtoegknd => Fields[12];

        public AddressDbaseSchema() => Fields = new[]
        {
            DbaseField.CreateStringField(new DbaseFieldName(nameof(id)), new DbaseFieldLength(50)),
            DbaseField.CreateInt32Field(new DbaseFieldName(nameof(adresid)), new DbaseFieldLength(10)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(versieid)), new DbaseFieldLength(25)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(posspec)), new DbaseFieldLength(20)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(posgeommet)), new DbaseFieldLength(30)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(straatnmid)), new DbaseFieldLength(10)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(straatnm)), new DbaseFieldLength(80)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(huisnr)), new DbaseFieldLength(11)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(busnr)), new DbaseFieldLength(35)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(postcode)), new DbaseFieldLength(4)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(gemeentenm)), new DbaseFieldLength(40)),
            DbaseField.CreateStringField(new DbaseFieldName(nameof(status)), new DbaseFieldLength(20)),
            DbaseField.CreateLogicalField(new DbaseFieldName(nameof(offtoegknd)))
        };
    }
}
