namespace AddressRegistry.Api.Legacy.AddressMatch
{
    using System.Collections.Generic;
    using Matching;

    internal class ValidationMessageWarningLogger : IWarningLogger
    {
        private const string SEPARATOR = " - ";
        public List<ValidationMessage> Warnings { get; private set; }

        public ValidationMessageWarningLogger()
        {
            Warnings = new List<ValidationMessage>();
        }

        public void AddWarning(string code, string message)
        {
            Warnings.Add(new ValidationMessage { Code = code, Message = message });
        }
    }

    /// <summary>
    /// contains a warning message in dutch and english
    /// </summary>
    public class ValidationMessage
    {
        /// <summary>
        /// A code
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// A descriptive message. Preferably in Dutch and English
        /// </summary>
        public string Message { get; set; }
    }
}
