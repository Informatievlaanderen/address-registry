namespace AddressRegistry.Importer.Subaddress
{
    using System;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Api;
    using Properties;

    internal class SettingsBasedConfig : IHttpApiProxyConfig, ICommandProcessorConfig
    {
        public Uri BaseUrl => new Uri(Settings.Default.BaseUrl);
        public string ImportEndpoint => Settings.Default.ImportEndpoint;
        public int HttpTimeoutMinutes => Settings.Default.HttpTimeoutInMinutes;
        public string AuthUserName => Settings.Default.ImportAuthUserName;
        public string AuthPassword => Settings.Default.ImportAuthPassword;
        public int NrOfProducers => Settings.Default.NrOfProducers;
        public int BufferSize => Settings.Default.BufferSize;
        public int NrOfConsumers => Settings.Default.NrOfConsumers;
        public int BatchSize => Settings.Default.BatchSize;
        public TimeSpan TimeMargin => Settings.Default.TimeMargin;

        public DateTime? LastRunDate
        {
            get => Settings.Default.LastRunDate == DateTime.MinValue
                ? (DateTime?) null
                : Settings.Default.LastRunDate;
            set
            {
                Settings.Default.LastRunDate = value ?? DateTime.MinValue;
                Settings.Default.Save();
            }
        }

        public DateTime? EndDateRecovery
        {
            get => Settings.Default.EndDateRecovery == DateTime.MinValue
                ? (DateTime?) null
                : Settings.Default.EndDateRecovery;
            set
            {
                Settings.Default.EndDateRecovery = value ?? DateTime.MinValue;
                Settings.Default.Save();
            }
        }

        public override string ToString() => $"{Environment.NewLine}" +
                                             $"BaseUrl: {BaseUrl}{Environment.NewLine}" +
                                             $"ImportEndpoint: {ImportEndpoint}{Environment.NewLine}" +
                                             $"HttpTimeoutMinutes: {HttpTimeoutMinutes}{Environment.NewLine}" +
                                             $"TimeMargin: {TimeMargin}{Environment.NewLine}" +
                                             $"EndDateRecovery: {EndDateRecovery}{Environment.NewLine}" +
                                             $"LastRunDate: {LastRunDate}{Environment.NewLine}" +
                                             $"NrOfProducers: {NrOfProducers}{Environment.NewLine}" +
                                             $"BufferSize: {BufferSize}{Environment.NewLine}" +
                                             $"NrOfConsumers: {NrOfConsumers}{Environment.NewLine}" +
                                             $"BatchSize: {BatchSize}";
    }
}
