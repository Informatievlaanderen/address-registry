namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework.Mocking
{
    using System;
    using System.Collections.Generic;
    using Moq;
    using Newtonsoft.Json;

    public class MockingSetup<TMoq> where TMoq : class
    {
        private Mock<TMoq> _moq;

        protected internal Mock<TMoq> Moq
        {
            get => _moq;
            set
            {
                _moq = value;
                SetupMock();
            }
        }

        protected internal IDictionary<string, object> MockingContext { get; set; }

        protected internal Formatting LogFormatting { get; set; }

        internal Action<string> LogAction { get; set; }

        protected virtual void When(string message) => LogAction($"When {message}.");

        protected virtual void SetupMock() { }
    }
}
