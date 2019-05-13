namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Mocking
{
    using System;
    using System.Collections.Generic;
    using Moq;
    using Newtonsoft.Json;

    public class MockingSetup<TMoq> where TMoq : class
	{
		private Mock<TMoq> moq;
		protected internal Mock<TMoq> Moq
		{
			get { return moq; }
			set {
				moq = value;
				SetupMock();
			}
		}

		protected internal IDictionary<string, object> MockingContext { get; set; }

        protected internal Formatting LogFormatting { get; set; }

        internal Action<string> LogAction { get; set; }

		protected virtual void When(string message)
		{
			LogAction(string.Format("When {0}.", message));
		}

		protected virtual void SetupMock() { }
	}
}
