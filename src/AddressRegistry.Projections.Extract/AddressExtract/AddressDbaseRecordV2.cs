namespace AddressRegistry.Projections.Extract.AddressExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class AddressDbaseRecordV2 : DbaseRecord
    {
        public static readonly AddressDbaseSchemaV2 Schema = new AddressDbaseSchemaV2();

        public DbaseCharacter id { get; }
        public DbaseInt32 adresid { get; }
        public DbaseCharacter versieid { get; }
        public DbaseCharacter posspec { get; }
        public DbaseCharacter posgeommet { get; }
        public DbaseCharacter straatnmid { get; }
        public DbaseCharacter straatnm { get; }
        public DbaseCharacter huisnr { get; }
        public DbaseCharacter busnr { get; }
        public DbaseCharacter postcode { get; }
        public DbaseCharacter gemeenteid { get; }
        public DbaseCharacter gemeentenm { get; }
        public DbaseCharacter status { get; }
        public DbaseLogical offtoegknd { get; }
        public DbaseCharacter voladres { get; }

        public AddressDbaseRecordV2()
        {
            id = new DbaseCharacter(Schema.id);
            adresid = new DbaseInt32(Schema.adresid);
            versieid = new DbaseCharacter(Schema.versieid);
            posspec = new DbaseCharacter(Schema.posspec);
            posgeommet = new DbaseCharacter(Schema.posgeommet);
            straatnmid = new DbaseCharacter(Schema.straatnmid);
            straatnm = new DbaseCharacter(Schema.straatnm);
            huisnr = new DbaseCharacter(Schema.huisnr);
            busnr = new DbaseCharacter(Schema.busnr);
            postcode = new DbaseCharacter(Schema.postcode);
            gemeenteid = new DbaseCharacter(Schema.gemeenteid);
            gemeentenm = new DbaseCharacter(Schema.gemeentenm);
            status = new DbaseCharacter(Schema.status);
            offtoegknd = new DbaseLogical(Schema.offtoegknd);
            voladres = new DbaseCharacter(Schema.voladres);

            Values = new DbaseFieldValue[]
            {
                id,
                adresid,
                versieid,
                posspec,
                posgeommet,
                straatnmid,
                straatnm,
                huisnr,
                busnr,
                postcode,
                gemeenteid,
                gemeentenm,
                status,
                offtoegknd,
                voladres
            };
        }
    }
}
