namespace AddressRegistry.Api.Oslo.AddressMatch
{
    using System.Collections.Generic;
    using System.Runtime.Serialization;

    internal class ValidationMessageWarningLogger : IWarningLogger
    {
        public List<ValidationOsloMessage> Warnings { get; }

        public ValidationMessageWarningLogger()
        {
            Warnings = new List<ValidationOsloMessage>();
        }

        public void AddWarning(string code, string message) => Warnings.Add(new ValidationOsloMessage { Code = code, Message = message });
    }

    /// <summary>
    /// contains a warning message in dutch and english
    /// </summary>
    [DataContract(Name = "Warning", Namespace = "")]
    public class ValidationOsloMessage
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
