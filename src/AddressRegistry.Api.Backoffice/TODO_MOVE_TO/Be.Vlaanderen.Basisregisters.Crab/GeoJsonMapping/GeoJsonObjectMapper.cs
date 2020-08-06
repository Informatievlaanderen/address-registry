namespace AddressRegistry.Api.Backoffice.TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Crab.GeoJsonMapping
{
    using System;
    using GeoJSON.Net;

    public interface IGeoJsonObjectMapper
    {
        Type MapType { get; }
    }

    public abstract class GeoJsonObjectMapper<T> : IGeoJsonObjectMapper
        where T : GeoJSONObject
    {
        public Type MapType => typeof(T);
        public abstract string ToWkt(T geoJson);
    }
}
