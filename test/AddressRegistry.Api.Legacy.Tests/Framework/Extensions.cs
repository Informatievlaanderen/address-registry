namespace AddressRegistry.Api.Legacy.Tests.Framework
{
    using System.Collections.Generic;
    using System.Linq;
    using Newtonsoft.Json;

    public static class Extensions
    {
        public static string ToLoggableString<T>(this T @object, Formatting formatting = Formatting.None)
            => JsonConvert.SerializeObject(@object, formatting);

        public static string ToLoggableString<T>(this IEnumerable<T> objects, Formatting formatting = Formatting.None)
            => objects.Count() < 5
                ? JsonConvert.SerializeObject(objects, formatting)
                : "...";
    }
}
