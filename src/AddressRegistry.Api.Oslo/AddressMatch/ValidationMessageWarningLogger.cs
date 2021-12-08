namespace AddressRegistry.Api.Oslo.AddressMatch
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using Matching;

    internal class ValidationMessageWarningLogger : IWarningLogger
    {
        private const string Separator = " - ";
        public List<ValidationMessage> Warnings { get; private set; }

        public ValidationMessageWarningLogger()
        {
            Warnings = new List<ValidationMessage>();
        }

        public void AddWarning(string code, string message) => Warnings.Add(new ValidationMessage { Code = code, Message = message });
    }

    /// <summary>
    /// contains a warning message in dutch and english
    /// </summary>
    [DataContract(Name = "Warning", Namespace = "")]
    public class ValidationMessage
    {
        /// <summary>
        /// Code van de warning.
        /// </summary>
        [DataMember(Name = "Code", Order = 1)]
        public string Code { get; set; }

        /// <summary>
        /// Een beschrijvende boodschap van de warning in het Nederlands.
        /// </summary>
        [DataMember(Name = "Message", Order = 2)]
        public string Message { get; set; }
    }
}
