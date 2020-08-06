namespace AddressRegistry.Api.Backoffice.TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Crab.GeoJsonMapping
{
    using System;
    using System.Collections.Generic;
    using GeoJSON.Net;

    public class GeoJsonMapper
    {
        private readonly IDictionary<Type, IGeoJsonObjectMapper> _mappers = new Dictionary<Type, IGeoJsonObjectMapper>();

        public GeoJsonMapper(IEnumerable<IGeoJsonObjectMapper> mappers)
        {
            foreach (var geoJsonMapper in mappers)
                Register(geoJsonMapper);
        }

        public string ToWkt<T>(T geoJson)
            where T : GeoJSONObject
        {
            if (geoJson == null)
                throw new ArgumentNullException(nameof(geoJson));

            var geoJsonType = typeof(T);
            if (!_mappers.ContainsKey(geoJsonType))
                throw new NotImplementedException($"No WKT conversion for type {typeof(T).FullName}");

            if (!(_mappers[geoJsonType] is GeoJsonObjectMapper<T> mapper))
                throw new Exception($"Error casting mapper {_mappers[geoJsonType].GetType().FullName} as {typeof(GeoJsonObjectMapper<T>).FullName}");

            return mapper.ToWkt(geoJson);
        }

        private void Register(IGeoJsonObjectMapper objectMapper)
        {
            var geoJsonType = objectMapper.MapType;
            if (!_mappers.ContainsKey(geoJsonType))
                _mappers.Add(geoJsonType, objectMapper);
        }
    }
}
