namespace AddressRegistry.Consumer
{
    public class ConsumerOptions
    {
        public string Topic { get; }
        public string ConsumerGroupSuffix { get; }

        public ConsumerOptions(string topic, string consumerGroupSuffix)
        {
            Topic = topic;
            ConsumerGroupSuffix = consumerGroupSuffix;
        }
    }
}
