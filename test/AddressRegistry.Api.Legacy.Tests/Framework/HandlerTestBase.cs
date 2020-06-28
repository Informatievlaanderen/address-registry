namespace AddressRegistry.Api.Legacy.Tests.Framework
{
    using System;
    using Assert;
    using Autofac;
    using Microsoft.EntityFrameworkCore.Storage;
    using Newtonsoft.Json;
    using Xunit.Abstractions;

    public abstract class HandlerTestBase : IocBasedTest
    {
        private readonly Action<string> _noOpLogging = m => { };
        private readonly Action<string> _logAction;

        internal static readonly InMemoryDatabaseRoot InMemoryDatabaseRootRoot = new InMemoryDatabaseRoot();

        protected HandlerTestBase(
            ITestOutputHelper testOutputHelper,
            Action<string> logAction,
            Formatting logFormatting = Formatting.Indented,
            bool disableArrangeLogging = false,
            bool disableAssertionLogging = false)
            : base(testOutputHelper)
        {
            _logAction = logAction;

            Assertions.LogAction.Value = !disableAssertionLogging ? Log : _noOpLogging;
            Mocking.Mocking.LogAction = !disableArrangeLogging ? Log : _noOpLogging;
            Mocking.Mocking.LogFormatting = logFormatting;
        }

        protected override void ConfigureMocks(ContainerBuilder containerBuilder) { }

        protected void Log(string message)
            => _logAction?.Invoke($"{DateTime.Now,-26:yyyy/MM/dd HH:mm:ss.fff}{message}");
    }
}

