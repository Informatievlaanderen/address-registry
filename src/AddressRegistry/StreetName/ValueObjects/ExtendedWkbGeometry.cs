namespace AddressRegistry.StreetName
{
    using System;
    using System.Buffers.Binary;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.IO;

    public class ExtendedWkbGeometry : ByteArrayValueObject<ExtendedWkbGeometry>
    {
        public const int SridLambert72 = 31370;
        public const int SridLambert2008 = 3812;

        private const uint EwkbSridFlag = 0x20000000;

        public ExtendedWkbGeometry(byte[] ewkbBytes) : base(ewkbBytes) { }

        public ExtendedWkbGeometry(string ewkbBytesHex) : base(ewkbBytesHex.ToByteArray()) { }

        public override string ToString() => Value.ToHexString();

        public static ExtendedWkbGeometry CreateEWkb(byte[] wkb)
        {
            if (wkb == null)
                return null;
            try
            {
                var srid = ResolveSridOrThrow(wkb);
                var reader = srid == SridLambert2008
                    ? WKBReaderFactory.CreateForLambert2008()
                    : WKBReaderFactory.Create();
                var geometry = reader.Read(wkb);
                geometry.SRID = srid;
                var wkbWriter = new WKBWriter { Strict = false, HandleSRID = true };
                return new ExtendedWkbGeometry(wkbWriter.Write(geometry));
            }
            catch (ArgumentException)
            {
                return null;
            }
        }

        private static int ResolveSridOrThrow(byte[] wkb)
        {
            if (!TryReadSrid(wkb, out var srid))
                return SridLambert72;

            if (srid == SridLambert72 || srid == SridLambert2008)
                return srid;

            throw new InvalidOperationException($"Unsupported SRID: {srid}.");
        }

        private static bool TryReadSrid(byte[] wkb, out int srid)
        {
            srid = 0;
            if (wkb == null || wkb.Length < 9)
                return false;

            var littleEndian = wkb[0] == 1;
            var type = littleEndian
                ? BinaryPrimitives.ReadUInt32LittleEndian(wkb.AsSpan(1, 4))
                : BinaryPrimitives.ReadUInt32BigEndian(wkb.AsSpan(1, 4));

            if ((type & EwkbSridFlag) == 0)
                return false;

            srid = littleEndian
                ? BinaryPrimitives.ReadInt32LittleEndian(wkb.AsSpan(5, 4))
                : BinaryPrimitives.ReadInt32BigEndian(wkb.AsSpan(5, 4));

            return true;
        }
    }
}
