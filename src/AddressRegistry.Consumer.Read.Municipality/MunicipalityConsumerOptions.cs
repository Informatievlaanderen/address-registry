namespace AddressRegistry.Consumer.Read.Municipality
{
    public class MunicipalityConsumerOptions
    {
        public string Topic { get; }
        public string ConsumerGroupSuffix { get; }

        public MunicipalityConsumerOptions(string topic, string consumerGroupSuffix)
        {
            Topic = topic;
            ConsumerGroupSuffix = consumerGroupSuffix;
        }
    }
}
