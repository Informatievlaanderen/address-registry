namespace AddressRegistry.Api.BackOffice
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Be.Vlaanderen.Basisregisters.Api.ETag;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Net.Http.Headers;

    //TODO: move to lib after queue system is approved
    public class AcceptedWithETagResult : AcceptedResult
    {
        private readonly List<string> _tags;
        public string ETag { get; }

        public AcceptedWithETagResult(Uri location, string eTag, params string[] tags) : base(location, null)
        {
            _tags = new List<string>(tags);
            ETag = eTag;
        }

        public override async Task ExecuteResultAsync(ActionContext context)
        {
            AddLastObservedPositionAsEtag(context);
            await base.ExecuteResultAsync(context);
        }

        public override void ExecuteResult(ActionContext context)
        {
            AddLastObservedPositionAsEtag(context);
            base.ExecuteResult(context);
        }

        private void AddLastObservedPositionAsEtag(ActionContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            _tags.Add(ETag);
            var etag = new ETag(ETagType.Strong, string.Join("-", _tags));

            if (context.HttpContext.Response.Headers.Keys.Contains(HeaderNames.ETag))
            {
                context.HttpContext.Response.Headers[HeaderNames.ETag] = etag.ToString();
            }
            else
            {
                context.HttpContext.Response.Headers.Add(HeaderNames.ETag, etag.ToString());
            }
        }
    }
}
