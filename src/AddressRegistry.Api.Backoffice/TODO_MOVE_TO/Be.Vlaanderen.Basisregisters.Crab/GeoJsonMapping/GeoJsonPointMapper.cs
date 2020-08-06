namespace AddressRegistry.Api.Backoffice.TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Crab.GeoJsonMapping
{
    using System;
    using GeoJSON.Net.Geometry;

    public class GeoJsonObjectPointMapper : GeoJsonObjectMapper<Point>
    {
        public override string ToWkt(Point point)
        {
            if (point == null)
                throw new ArgumentNullException(nameof(point));

            return $"POINT ({point.Coordinates.Longitude} {point.Coordinates.Latitude})";
        }
    }
}
