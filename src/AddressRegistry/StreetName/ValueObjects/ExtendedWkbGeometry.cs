namespace AddressRegistry.StreetName
{
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;

    public class ExtendedWkbGeometry : ByteArrayValueObject<ExtendedWkbGeometry>
    {
        public ExtendedWkbGeometry(byte[] ewkbBytes) : base(ewkbBytes) { }

        public ExtendedWkbGeometry(string ewkbBytesHex) : base(ewkbBytesHex.ToByteArray()) { }

        public override string ToString() => Value.ToHexString();
    }
}
