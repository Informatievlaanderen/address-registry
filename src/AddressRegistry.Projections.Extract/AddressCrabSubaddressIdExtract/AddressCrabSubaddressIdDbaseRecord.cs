namespace AddressRegistry.Projections.Extract.AddressCrabSubaddressIdExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class AddressCrabSubaddressIdDbaseRecord : DbaseRecord
    {
        public static readonly AddressCrabSubaddressIdDbaseSchema Schema = new AddressCrabSubaddressIdDbaseSchema();

        public DbaseNumber adresid { get; }
        public DbaseNumber crabsubid { get; }
        public DbaseLogical isvolledig { get; }

        public AddressCrabSubaddressIdDbaseRecord()
        {
            adresid = new DbaseNumber(Schema.adresid);
            crabsubid = new DbaseNumber(Schema.crabsubid);
            isvolledig = new DbaseLogical(Schema.isvolledig);

            Values = new DbaseFieldValue[]
            {
                adresid,
                crabsubid,
                isvolledig
            };
        }
    }
}
