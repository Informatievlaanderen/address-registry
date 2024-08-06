namespace AddressRegistry.Projections.Elastic.Exceptions
{
    using System;
    using System.Runtime.Serialization;

    [Serializable]
    public class ElasticsearchClientException : Exception
    {
        public ElasticsearchClientException()
        { }

        public ElasticsearchClientException(string message)
            : base(message)
        { }

        public ElasticsearchClientException(string message, Exception? inner)
            : base(message, inner)
        { }

        private ElasticsearchClientException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }
    }
}
