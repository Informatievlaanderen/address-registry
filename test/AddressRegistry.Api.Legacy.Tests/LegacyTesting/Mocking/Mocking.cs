namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Mocking
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Generate;
    using Moq;
    using Newtonsoft.Json;

    public class Mocking<TMoq, TMockingSetup, TMockingVerification>
		where TMoq : class
		where TMockingSetup : MockingSetup<TMoq>, new()
		where TMockingVerification : MockingVerification<TMoq>, new()
	{
		private TMockingSetup _setup;
		private TMockingVerification _verification;
		private Mock<TMoq> _moq;
        private Random _random;

        public Mocking(string description = null, Action<string> logAction = null, Formatting? logFormatting = null)
		{
            _random = new Random();
			_moq = new Mock<TMoq>();
            Action<string> notNullLogaction = logAction ?? Mocking.LogAction ?? (message => Trace.WriteLine(message));
			description = description ?? typeof(TMoq).Name;
			Action<string> prefixedLogAction = message => notNullLogaction($"{"ARRANGE".PadRight(10)}{description}: {message}");

            LogFormatting = logFormatting?? Mocking.LogFormatting;

            Dictionary<string, object> mockingContext = new Dictionary<string, object>();

			_setup = new TMockingSetup();
			_setup.LogFormatting = LogFormatting;
			_setup.MockingContext = mockingContext;
			_setup.Moq = _moq;
			_setup.LogAction = prefixedLogAction;

			_verification = new TMockingVerification();
            _verification.LogFormatting = LogFormatting;
			_verification.MockingContext = mockingContext;
			_verification.Moq = _moq;
			_verification.LogAction = prefixedLogAction;
		}

		public TMoq Object
		{
			get { return _moq.Object; }
		}

		public TMockingSetup When()
		{
			return _setup;
		}

		public TMockingVerification VerifyThat()
		{
			return _verification;
		}

        public Formatting LogFormatting { get; set; }

        public T Arrange<T>(Generator<T> generator, Action<TMockingSetup, T> when)
        {
            T stub = generator.Generate(_random);
            when(_setup, stub);

            return stub;
        }
    }

    public static class Mocking
    {
        static Mocking()
        {
            LogFormatting = Formatting.Indented;
            LogAction = message => Trace.WriteLine(message);
        }
        public static Formatting LogFormatting { get; set; }
        public static Action<string> LogAction { get; set; }

    }
}
