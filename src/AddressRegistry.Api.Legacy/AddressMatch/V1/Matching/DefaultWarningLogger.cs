namespace AddressRegistry.Api.Legacy.AddressMatch.V1.Matching
{
    using System.Collections.Generic;

    internal class DefaultWarningLogger : IWarningLogger
    {
        public List<string> Warnings { get; } = new List<string>();

        public void AddWarning(string code, string message) => Warnings.Add($"{code}: {message}");
    }

    public interface IWarningLogger
    {
        void AddWarning(string code, string message);
    }
}
