namespace AddressRegistry.Api.Legacy.Tests.Framework.Mocking
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
		private readonly TMockingSetup _setup;
		private readonly TMockingVerification _verification;
		private readonly Mock<TMoq> _moq;
        private readonly Random _random;

        public TMoq Object => _moq.Object;

        public Formatting LogFormatting { get; set; }

        public Mocking(
            string description = null,
            Action<string> logAction = null,
            Formatting? logFormatting = null)
		{
            _random = new Random();
			_moq = new Mock<TMoq>();

			description ??= typeof(TMoq).Name;
            var log = logAction ?? Mocking.LogAction ?? (message => Trace.WriteLine(message));
            LogFormatting = logFormatting ?? Mocking.LogFormatting;

            void PrefixedLogAction(string message) => log($"{"ARRANGE",-10}{description}: {message}");

            Dictionary<string, object> mockingContext = new Dictionary<string, object>();

            _setup = new TMockingSetup
            {
                LogFormatting = LogFormatting,
                MockingContext = mockingContext,
                Moq = _moq,
                LogAction = PrefixedLogAction
            };

            _verification = new TMockingVerification
            {
                LogFormatting = LogFormatting,
                MockingContext = mockingContext,
                Moq = _moq,
                LogAction = PrefixedLogAction
            };
        }

        public TMockingSetup When() => _setup;

        public TMockingVerification VerifyThat() => _verification;

        public T Arrange<T>(Generator<T> generator, Action<TMockingSetup, T> when)
        {
            var stub = generator.Generate(_random);
            when(_setup, stub);

            return stub;
        }
    }

    public static class Mocking
    {
        public static Formatting LogFormatting { get; set; }
        public static Action<string> LogAction { get; set; }

        static Mocking()
        {
            LogFormatting = Formatting.Indented;
            LogAction = message => Trace.WriteLine(message);
        }
    }
}
