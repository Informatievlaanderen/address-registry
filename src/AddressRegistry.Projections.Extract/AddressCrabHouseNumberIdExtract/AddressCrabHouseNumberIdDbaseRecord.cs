namespace AddressRegistry.Projections.Extract.AddressCrabHouseNumberIdExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class AddressCrabHouseNumberIdDbaseRecord : DbaseRecord
    {
        public static readonly AddressCrabHouseNumberIdDbaseSchema Schema = new AddressCrabHouseNumberIdDbaseSchema();

        public DbaseNumber adresid { get; }
        public DbaseNumber crabhnrid { get; }
        public DbaseLogical isvolledig { get; }

        public AddressCrabHouseNumberIdDbaseRecord()
        {
            adresid = new DbaseNumber(Schema.adresid);
            crabhnrid = new DbaseNumber(Schema.crabhnrid);
            isvolledig = new DbaseLogical(Schema.isvolledig);

            Values = new DbaseFieldValue[]
            {
                adresid,
                crabhnrid,
                isvolledig
            };
        }
    }
}
