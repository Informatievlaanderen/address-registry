namespace AddressRegistry.Projections.Extract.AddressLinkExtract
{
    using Be.Vlaanderen.Basisregisters.Shaperon;

    public class AddressLinkDbaseRecord : DbaseRecord
    {
        public static readonly AddressLinkDbaseSchema Schema = new AddressLinkDbaseSchema();

        public DbaseCharacter objecttype { get; }
        public DbaseCharacter adresobjid { get; }
        public DbaseInt32 adresid { get; }
        public DbaseCharacter voladres { get; }

        public AddressLinkDbaseRecord()
        {
            objecttype = new DbaseCharacter(Schema.objecttype);
            adresobjid = new DbaseCharacter(Schema.adresobjid);
            adresid = new DbaseInt32(Schema.adresid);
            voladres = new DbaseCharacter(Schema.voladres);

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
