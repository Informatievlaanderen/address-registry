namespace AddressRegistry.Importer.Subaddress.Console
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Api;
    using Microsoft.Extensions.Configuration;

    internal class SettingsBasedConfig : IHttpApiProxyConfig, ICommandProcessorConfig, ICommandProcessorBatchConfiguration<int>
    {
        public SettingsBasedConfig(IConfiguration configurationSection)
        {
            BaseUrl = new Uri(configurationSection["BaseUrl"]);
            ImportEndpoint = configurationSection["ImportEndpoint"];
            ImportBatchStatusEndpoint = configurationSection["ImportBatchStatusEndpoint"];
            HttpTimeoutMinutes = Convert.ToInt32(configurationSection["HttpTimeoutInMinutes"]);
            NrOfProducers = Convert.ToInt32(configurationSection["NrOfProducers"]);
            BufferSize = Convert.ToInt32(configurationSection["BufferSize"]);
            NrOfConsumers = Convert.ToInt32(configurationSection["NrOfConsumers"]);
            BatchSize = Convert.ToInt32(configurationSection["BatchSize"]);
            WaitForUserInput = Convert.ToBoolean(configurationSection["WaitForUserInput"]);
            TimeMargin = new TimeSpan(0, 0, Convert.ToInt32(configurationSection["TimeMarginInMinutes"]), 0);
            FeedName = configurationSection["FeedName"];
        }

        public Uri BaseUrl { get; }
        public string ImportEndpoint { get; }
        public string ImportBatchStatusEndpoint { get; }
        public string AuthUserName => string.Empty;
        public string AuthPassword => string.Empty;
        public int HttpTimeoutMinutes { get; }
        public int NrOfProducers { get; }
        public int BufferSize { get; }
        public int NrOfConsumers { get; }
        public int BatchSize { get; }
        public bool WaitForUserInput { get; }
        public TimeSpan TimeMargin { get; }
        public string FeedName { get; }

        public override string ToString() => $"{Environment.NewLine}" +
                                             $"BaseUrl: {BaseUrl}{Environment.NewLine}" +
                                             $"AuthUserName: {AuthUserName}{Environment.NewLine}" +
                                             $"AuthPassword: {AuthPassword}{Environment.NewLine}" +
                                             $"ImportEndpoint: {ImportEndpoint}{Environment.NewLine}" +
                                             $"ImportBatchStatusEndpoint: {ImportBatchStatusEndpoint}{Environment.NewLine}" +
                                             $"HttpTimeoutMinutes: {HttpTimeoutMinutes}{Environment.NewLine}" +
                                             $"TimeMargin: {TimeMargin}{Environment.NewLine}" +
                                             $"NrOfProducers: {NrOfProducers}{Environment.NewLine}" +
                                             $"BufferSize: {BufferSize}{Environment.NewLine}" +
                                             $"NrOfConsumers: {NrOfConsumers}{Environment.NewLine}" +
                                             $"BatchSize: {BatchSize}" +
                                             $"WaitForUserInput: {WaitForUserInput}" +
                                             $"FeedName: {FeedName}";

        public int Deserialize(string key) => int.Parse(key);
    }
}
