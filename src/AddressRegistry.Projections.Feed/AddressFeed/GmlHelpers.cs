namespace AddressRegistry.Projections.Feed.AddressFeed
{
    using System.Globalization;
    using System.Text;
    using System.Xml;
    using Be.Vlaanderen.Basisregisters.Utilities.HexByteConvertor;
    using NetTopologySuite.IO;

    public static class GmlHelpers
    {
        private static readonly WKBReader WkbReader = WKBReaderFactory.Create();

        public static string ConvertToGml(string extendedWkbGeometryHex)
        {
            var geometry = WkbReader.Read(extendedWkbGeometryHex.ToByteArray());

            var builder = new StringBuilder();
            var settings = new XmlWriterSettings { Indent = false, OmitXmlDeclaration = true };
            using (var xmlWriter = XmlWriter.Create(builder, settings))
            {
                xmlWriter.WriteStartElement("gml", "Point", "http://www.opengis.net/gml/3.2");
                xmlWriter.WriteAttributeString("srsName", "https://www.opengis.net/def/crs/EPSG/0/31370");
                xmlWriter.WriteStartElement("gml", "pos", "http://www.opengis.net/gml/3.2");
                xmlWriter.WriteValue(string.Format(
                    CultureInfo.InvariantCulture,
                    "{0} {1}",
                    geometry.Coordinate.X.ToString("F2", CultureInfo.InvariantCulture),
                    geometry.Coordinate.Y.ToString("F2", CultureInfo.InvariantCulture)));
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
            }

            return builder.ToString();
        }
    }
}
