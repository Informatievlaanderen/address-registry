namespace AddressRegistry.StreetName
{
    using System;
    using Be.Vlaanderen.Basisregisters.AggregateSource;
    using Be.Vlaanderen.Basisregisters.GrAr.Common;
    using Be.Vlaanderen.Basisregisters.GrAr.Common.NetTopology;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.IO;

    public class ExtendedWkbGeometry : ByteArrayValueObject<ExtendedWkbGeometry>
    {
        private static readonly WKBWriter WkbWriter = new WKBWriter { Strict = false, HandleSRID = true };

        public const int SridLambert72 = SystemReferenceId.SridLambert72;

        public ExtendedWkbGeometry(byte[] ewkbBytes) : base(ewkbBytes) { }

        public ExtendedWkbGeometry(string ewkbBytesHex) : base(ewkbBytesHex.ToByteArray()) { }

        public override string ToString() => Value.ToHexString();

        public static ExtendedWkbGeometry? CreateEWkb(byte[]? wkb, int useSrid = SridLambert72)
        {
            if (wkb == null)
                return null;

            try
            {
                if (!wkb.TryReadSrid(out var srid))
                {
                    if (useSrid == SridLambert72)
                    {
                        var geometry = WKBReaderFactory.CreateForLambert72().Read(wkb);
                        return new ExtendedWkbGeometry(WkbWriter.Write(geometry));
                    }

                    if (useSrid == SystemReferenceId.SridLambert2008)
                    {
                        var geometry = WKBReaderFactory.CreateForLambert2008().Read(wkb);
                        return new ExtendedWkbGeometry(WkbWriter.Write(geometry));
                    }

                    return null;
                }

                if (srid != useSrid)
                    throw new InvalidOperationException("SRID in EWKB does not match the expected SRID.");

                var reader = WKBReaderFactory.CreateForEwkb(wkb);
                var ewkbGeometry = reader.Read(wkb);
                ewkbGeometry.SRID = srid;
                return new ExtendedWkbGeometry(WkbWriter.Write(ewkbGeometry));
            }
            catch (ParseException)
            {
                return null;
            }
            catch (ArgumentException)
            {
                return null;
            }
        }
    }
}
