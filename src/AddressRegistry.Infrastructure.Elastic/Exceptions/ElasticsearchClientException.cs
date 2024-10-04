namespace AddressRegistry.Infrastructure.Elastic.Exceptions
{
    using System;
    using System.Runtime.Serialization;
    using global::Elastic.Transport.Products.Elasticsearch;

    [Serializable]
    public class ElasticsearchClientException : Exception
    {
        public ElasticsearchClientException()
        { }

        public ElasticsearchClientException(string message)
            : base(message)
        { }

        public ElasticsearchClientException(Exception? exception)
            : base("Failed to project to Elasticsearch", exception)
        { }

        public ElasticsearchClientException(string message, ElasticsearchServerError? serverError, string debugInformation)
            : base($"{message} [ServerError.Status={serverError?.Status}, ServerError.Error={serverError?.Error}, DebugInformation={debugInformation}]")
        { }

        public ElasticsearchClientException(string message, Exception? inner)
            : base(message, inner)
        { }

        private ElasticsearchClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
