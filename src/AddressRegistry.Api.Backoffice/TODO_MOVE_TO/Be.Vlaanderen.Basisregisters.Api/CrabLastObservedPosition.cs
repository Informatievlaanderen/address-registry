namespace AddressRegistry.Api.Backoffice.TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Api
{
    using System.Linq;
    using Microsoft.AspNetCore.Http;
    using NodaTime;

    public class CrabLastObservedPosition : LastObservedPosition
    {
        private readonly Instant _executionTime;

        public CrabLastObservedPosition(long lastObservedPosition, Instant executionTime)
            : base(lastObservedPosition)
            => _executionTime = executionTime;

        public CrabLastObservedPosition(HttpRequest request)
            : this(GetHeaderValuesFrom(request)) { }

        private CrabLastObservedPosition(LastObservedPositionHeaderValues headerValues)
            : this (headerValues.Position, headerValues.ExecutionTime) { }

        private static LastObservedPositionHeaderValues GetHeaderValuesFrom(HttpRequest request)
        {
            var values = GetHeaderValue(request)
                .Split('|')
                .Select(s => long.TryParse((string) s, out var result) ? result : 0)
                .ToArray();

            return new LastObservedPositionHeaderValues
            {
                Position = values.Length > 0 ? values[0] : 0,
                ExecutionTime = values.Length > 1 ? Instant.FromUnixTimeMilliseconds(values[1]) : Instant.MinValue
            };
        }

        public override string ToString()
            => $"{Position}|{_executionTime.ToUnixTimeMilliseconds()}";

        private class LastObservedPositionHeaderValues
        {
            public long Position { get; set; }
            public Instant ExecutionTime { get; set; }
        }
    }
}
