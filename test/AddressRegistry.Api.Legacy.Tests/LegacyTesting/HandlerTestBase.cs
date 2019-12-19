namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting
{
    using System;
    using System.Diagnostics;
    using Assert;
    using Autofac;
    using Castle.DynamicProxy;
    using Microsoft.EntityFrameworkCore.Storage;
    using Newtonsoft.Json;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    public abstract class HandlerTestBase : IocBasedTest
    {
        internal static InMemoryDatabaseRoot InMemoryDatabaseRootRoot = new InMemoryDatabaseRoot();
        private readonly Action<string> noOpLogging = m => { };
        private readonly Action<string> _logAction;
        private readonly Formatting _logFormatting;
        private bool _disableActLogging;

        public HandlerTestBase(
            ITestOutputHelper testOutputHelper,
            Action<string> logAction,
            Formatting logFormatting = Formatting.Indented,
            bool disableArrangeLogging = false,
            bool disableActLogging = false,
            bool disableAssertionLogging = false)
            : base(testOutputHelper)
        {
            _logAction = logAction;
            _logFormatting = logFormatting;

            _disableActLogging = disableActLogging;

            Assertions.LogAction.Value = !disableAssertionLogging ? Log : noOpLogging;
            Mocking.Mocking.LogAction = !disableArrangeLogging ? Log : noOpLogging;
            Mocking.Mocking.LogFormatting = _logFormatting;

        }

        protected override void ConfigureMocks(ContainerBuilder containerBuilder)
        {

        }

        protected void Log(string message)
        {
            _logAction($"{DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff").PadRight(26)}{message}");
        }
    }

    public class CallDurationAspect : IInterceptor
    {
        private Action<string> _logAction;

        public CallDurationAspect(Action<string> logAction)
        {
            _logAction = m => logAction($"ANALYZE: {m}");
        }
        public void Intercept(IInvocation invocation)
        {
            var stopwatch = Stopwatch.StartNew();
            invocation.Proceed();
            stopwatch.Stop();
            _logAction($"{invocation.Method.Name} took {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}

