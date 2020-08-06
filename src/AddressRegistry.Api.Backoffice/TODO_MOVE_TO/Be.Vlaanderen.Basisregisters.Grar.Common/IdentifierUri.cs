namespace AddressRegistry.Api.Backoffice.TODO_MOVE_TO.Be.Vlaanderen.Basisregisters.Grar.Common
{
    using System;
    using System.IO;

    public class IdentifierUri
    {
        private readonly Uri _uri;

        public string Value
            => Path.GetFileName(_uri.AbsolutePath);

        public IdentifierUri(string uri)
            : this(new Uri(uri)) { }

        public IdentifierUri(Uri uri)
            => _uri = uri ?? throw new ArgumentNullException(nameof(uri));

        public override bool Equals(object obj)
            => Equals(obj as IdentifierUri);

        protected bool Equals(IdentifierUri other)
            => Equals(_uri, other?._uri);

        public override int GetHashCode()
            => _uri != null ? _uri.GetHashCode() : 0;
    }

    public static class AsIdentifierExtension
    {
        public static IdentifierUri AsIdentifier(this string uri)
            => new IdentifierUri(uri);

        public static IdentifierUri AsIdentifier(this Uri uri)
            => new IdentifierUri(uri);
    }
}
