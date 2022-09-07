namespace AddressRegistry.Consumer.Read.StreetName
{
    public class StreetNameConsumerOptions
    {
        public string Topic { get; }
        public string ConsumerGroupSuffix { get; }

        public StreetNameConsumerOptions(string topic, string consumerGroupSuffix)
        {
            Topic = topic;
            ConsumerGroupSuffix = consumerGroupSuffix;
        }
    }
}
