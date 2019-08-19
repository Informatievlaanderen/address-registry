namespace AddressRegistry.Importer.HouseNumber
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Api;
    using Properties;

    internal class SettingsBasedConfig : IHttpApiProxyConfig, ICommandProcessorConfig, ICommandProcessorBatchConfiguration<int>
    {
        public Uri BaseUrl => new Uri(Settings.Default.BaseUrl);
        public string ImportEndpoint => Settings.Default.ImportEndpoint;
        public string ImportBatchStatusEndpoint => Settings.Default.ImportBatchStatusEndpoint;
        public int HttpTimeoutMinutes => Settings.Default.HttpTimeoutInMinutes;
        public string AuthUserName => Settings.Default.ImportAuthUser;
        public string AuthPassword => Settings.Default.ImportAuthPass;
        public int NrOfProducers => Settings.Default.NrOfProducers;
        public int BufferSize => Settings.Default.BufferSize;
        public int NrOfConsumers => Settings.Default.NrOfConsumers;
        public int BatchSize => Settings.Default.BatchSize;
        public bool WaitForUserInput => Settings.Default.WaitForUserInput;

        public TimeSpan TimeMargin => Settings.Default.TimeMargin;

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
                                             $"WaitForUserInput: {WaitForUserInput}";

        public int Deserialize(string key) => int.Parse(key);
    }
}
