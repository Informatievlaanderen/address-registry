namespace AddressRegistry.Projections.Extract.AddressLinkExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class AddressLinkDbaseRecord : DbaseRecord
    {
        public static readonly AddressLinkDbaseSchema Schema = new AddressLinkDbaseSchema();

        public DbaseString objecttype { get; }
        public DbaseString adresobjid { get; }
        public DbaseInt32 adresid { get; }
        public DbaseString voladres { get; }

        public AddressLinkDbaseRecord()
        {
            objecttype = new DbaseString(Schema.objecttype);
            adresobjid = new DbaseString(Schema.adresobjid);
            adresid = new DbaseInt32(Schema.adresid);
            voladres = new DbaseString(Schema.voladres);

            Values = new DbaseFieldValue[]
            {
                objecttype,
                adresobjid,
                adresid,
                voladres
            };
        }
    }
}
