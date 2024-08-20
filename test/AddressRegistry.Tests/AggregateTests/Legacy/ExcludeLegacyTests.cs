// Apply the custom framework at the assembly level
[assembly: Xunit.TestFramework("AddressRegistry.Tests.AggregateTests.Legacy.ExcludeLegacyTests", "AddressRegistry.Tests")]

namespace AddressRegistry.Tests.AggregateTests.Legacy
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Xunit.Abstractions;
    using Xunit.Sdk;

    // Custom Xunit Test Framework
    public class ExcludeLegacyTests : XunitTestFramework
    {
        public ExcludeLegacyTests(IMessageSink messageSink) : base(messageSink)
        {
        }

        protected override ITestFrameworkExecutor CreateExecutor(AssemblyName assemblyName)
        {
            return new ExcludeLegacyTestsExecutor(assemblyName, SourceInformationProvider, DiagnosticMessageSink);
        }
    }

    // Custom Executor
    public class ExcludeLegacyTestsExecutor : XunitTestFrameworkExecutor
    {
        public ExcludeLegacyTestsExecutor(AssemblyName assemblyName, ISourceInformationProvider sourceInformationProvider, IMessageSink diagnosticMessageSink)
            : base(assemblyName, sourceInformationProvider, diagnosticMessageSink)
        {
        }

        protected override void RunTestCases(IEnumerable<IXunitTestCase> testCases, IMessageSink executionMessageSink, ITestFrameworkExecutionOptions executionOptions)
        {
            var filteredTestCases = testCases.Where(tc => !tc.TestMethod.TestClass.Class.Name.StartsWith("AddressRegistry.Tests.AggregateTests.Legacy."));
            base.RunTestCases(filteredTestCases, executionMessageSink, executionOptions);
        }
    }
}
