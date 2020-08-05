namespace AddressRegistry.Api.Backoffice.TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Api
{
    using System;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;
    using NodaTime;

    public abstract class LastObservedPositionResult : OkObjectResult
    {
        private readonly string _location;
        private readonly LastObservedPosition _lastObservedPosition;

        protected LastObservedPositionResult(
            int statusCode,
            LastObservedPosition lastObservedPosition)
            : this(statusCode, string.Empty, lastObservedPosition) { }

        protected LastObservedPositionResult(
            int statusCode,
            string location,
            LastObservedPosition lastObservedPosition)
            : base(new { position = lastObservedPosition?.ToString() })
        {
            _location = location;
            _lastObservedPosition = lastObservedPosition ?? throw new ArgumentNullException(nameof(lastObservedPosition));

            StatusCode = statusCode;
        }

        public override void ExecuteResult(ActionContext context)
        {
            AddHeaders(context);
            base.ExecuteResult(context);
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            AddHeaders(context);
            return base.ExecuteResultAsync(context);
        }

        private void AddHeaders(ActionContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            var headers = context.HttpContext.Response.Headers;

            headers.Add(LastObservedPosition.HeaderName, _lastObservedPosition.ToString());

            if (!string.IsNullOrWhiteSpace(_location))
                headers.Add(HeaderNames.Location, _location);
        }
    }
}
