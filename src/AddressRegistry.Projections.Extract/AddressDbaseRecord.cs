namespace AddressRegistry.Projections.Extract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class AddressDbaseRecord : DbaseRecord
    {
        public static readonly AddressDbaseSchema Schema = new AddressDbaseSchema();

        public DbaseString id { get; }
        public DbaseString adresid { get; }
        public DbaseDateTime versie { get; }
        public DbaseString posspec { get; }
        public DbaseString posgeommet { get; }
        public DbaseString straatnmid { get; }
        public DbaseString straatnm { get; }
        public DbaseString huisnr { get; }
        public DbaseString busnr { get; }
        public DbaseString postcode { get; }
        public DbaseString gemeenteid { get; }
        public DbaseString gemeentenm { get; }
        public DbaseString status { get; }
        public DbaseBoolean offtoegknd { get; }

        public AddressDbaseRecord()
        {
            id = new DbaseString(Schema.id);
            adresid = new DbaseString(Schema.adresid);
            versie = new DbaseDateTime(Schema.versie);
            posspec = new DbaseString(Schema.posspec);
            posgeommet = new DbaseString(Schema.posgeommet);
            straatnmid = new DbaseString(Schema.straatnmid); // TODO: waiting for interregistry-communication
            straatnm = new DbaseString(Schema.straatnm); // TODO: waiting for interregistry-communication
            huisnr = new DbaseString(Schema.huisnr);
            busnr = new DbaseString(Schema.busnr);
            postcode = new DbaseString(Schema.postcode);
            gemeenteid = new DbaseString(Schema.gemeenteid); // TODO: waiting for interregistry-communication
            gemeentenm = new DbaseString(Schema.gemeentenm); // TODO: waiting for interregistry-communication
            status = new DbaseString(Schema.status);
            offtoegknd = new DbaseBoolean(Schema.offtoegknd);

            Values = new DbaseFieldValue[]
            {
                id,
                adresid,
                versie,
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
                offtoegknd
            };
        }
    }
}
