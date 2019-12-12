namespace AddressRegistry.Projections.Extract.AddressExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class AddressDbaseRecord : DbaseRecord
    {
        public static readonly AddressDbaseSchema Schema = new AddressDbaseSchema();

        public DbaseString id { get; }
        public DbaseInt32 adresid { get; }
        public DbaseString versieid { get; }
        public DbaseString posspec { get; }
        public DbaseString posgeommet { get; }
        public DbaseString straatnmid { get; }
        public DbaseString straatnm { get; }
        public DbaseString huisnr { get; }
        public DbaseString busnr { get; }
        public DbaseString postcode { get; }
        public DbaseString gemeentenm { get; }
        public DbaseString status { get; }
        public DbaseBoolean offtoegknd { get; }

        public AddressDbaseRecord()
        {
            id = new DbaseString(Schema.id);
            adresid = new DbaseInt32(Schema.adresid);
            versieid = new DbaseString(Schema.versieid);
            posspec = new DbaseString(Schema.posspec);
            posgeommet = new DbaseString(Schema.posgeommet);
            straatnmid = new DbaseString(Schema.straatnmid);
            straatnm = new DbaseString(Schema.straatnm);
            huisnr = new DbaseString(Schema.huisnr);
            busnr = new DbaseString(Schema.busnr);
            postcode = new DbaseString(Schema.postcode);
            gemeentenm = new DbaseString(Schema.gemeentenm);
            status = new DbaseString(Schema.status);
            offtoegknd = new DbaseBoolean(Schema.offtoegknd);

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
                gemeentenm,
                status,
                offtoegknd
            };
        }
    }
}
