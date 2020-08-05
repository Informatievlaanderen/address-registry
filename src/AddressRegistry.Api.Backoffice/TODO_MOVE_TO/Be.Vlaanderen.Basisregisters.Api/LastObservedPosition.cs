namespace AddressRegistry.Api.Backoffice.TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Api
{
    using System;
    using System.Linq;
    using Microsoft.AspNetCore.Http;

    public class LastObservedPosition
    {
        public const string HeaderName = "X-LastObservedPosition";

        public readonly long Position;

        public LastObservedPosition(long position)
            => Position = position;

        public LastObservedPosition(HttpRequest request)
            : this(GetHeaderValueFrom(request)) {}

        private static long GetHeaderValueFrom(HttpRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            request.Headers.TryGetValue(HeaderName, out var headerValues);
     
            return headerValues.Count == 1 && long.TryParse(headerValues.First(), out var position)
                ? position
                : 0;
        }
    }
}
