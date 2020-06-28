namespace AddressRegistry.Api.Legacy.Tests.Framework.Mocking
{
    using System;
    using System.Collections.Generic;
    using Moq;
    using Newtonsoft.Json;

    public class MockingVerification<TMoq> where TMoq : class
	{
		protected internal Mock<TMoq> Moq { get; set; }

		protected internal IDictionary<string, object> MockingContext { get; set; }

		internal Action<string> LogAction { get; set; }

        protected internal Formatting LogFormatting { get; set; }

        protected virtual void VerifiedThat(string message) => LogAction($"Verified that {message}.");

        public void AllMockedCallsWhereUsed() => Moq.VerifyAll();
    }
}
