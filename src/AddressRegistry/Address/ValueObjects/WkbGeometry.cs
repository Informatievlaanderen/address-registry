namespace AddressRegistry.Address.ValueObjects
{
    using System.Collections.Generic;
    using System.Linq;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using Newtonsoft.Json;

    public class WkbGeometry : ByteArrayValueObject<WkbGeometry>
    {
        [JsonConstructor]
        public WkbGeometry([JsonProperty("value")] byte[] wkbBytes) : base(wkbBytes) { }

        public WkbGeometry(string wkbBytesHex) : base(wkbBytesHex.ToByteArray()) { }

        public override string ToString() => Value.ToHexString();

        protected override IEnumerable<object> Reflect() => Value.Cast<object>();
    }

    public class ExtendedWkbGeometry : ByteArrayValueObject<ExtendedWkbGeometry>
    {
        [JsonConstructor]
        public ExtendedWkbGeometry([JsonProperty("value")] byte[] ewkbBytes) : base(ewkbBytes) { }

        public ExtendedWkbGeometry(string ewkbBytesHex) : base(ewkbBytesHex.ToByteArray()) { }

        public override string ToString() => Value.ToHexString();
    }
}
