namespace AddressRegistry.Api.Legacy.Tests.Framework.Assert
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using FluentAssertions;
    using FluentAssertions.Primitives;

    internal abstract class Assertions<T, TAssertion> : ReferenceTypeAssertions<T, TAssertion>
		where TAssertion : ReferenceTypeAssertions<T, TAssertion>
	{
		private readonly Action<string> _logAction;

        protected override string Identifier => typeof(T).Name;

        protected Assertions(T subject) : base(subject)
		{
			Action<string> notNullLogaction = Assertions.LogAction.Value ?? (message => Trace.WriteLine(message));
			_logAction = message => notNullLogaction($"{"ASSERT",-10}{Identifier}: {message}");
        }

        protected void AssertingThat(string message)
		{
			_logAction($"Asserting that {message}.");
		}

		protected AndConstraint<TAssertion> And()
            => new AndConstraint<TAssertion>(this as TAssertion);

        protected AndWhichConstraint<TAssertion, TWhich> AndWhich<TWhich>(TWhich newSubject)
            => new AndWhichConstraint<TAssertion, TWhich>(this as TAssertion, newSubject);
    }

    /// <summary>
    /// Allows for changing the default (Trace.WriteLine) log behaviour of Assertions
    /// </summary>
    internal static class Assertions
	{
        public static AsyncLocal<Action<string>> LogAction { get; set; }

        static Assertions()
		{
			LogAction = new AsyncLocal<Action<string>>(message => Trace.WriteLine(message));
		}
    }

}
