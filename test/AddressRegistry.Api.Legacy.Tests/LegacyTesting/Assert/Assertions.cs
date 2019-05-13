namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Assert
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using FluentAssertions;
    using FluentAssertions.Primitives;

    public abstract class Assertions<T, TAssertion> : ReferenceTypeAssertions<T, TAssertion>
		where TAssertion : ReferenceTypeAssertions<T, TAssertion>
	{
		private readonly Action<string> _logAction;

		protected Assertions(T subject)
		{
			Action<string> notNullLogaction = Assertions.LogAction.Value ?? (message => Trace.WriteLine(message));
			_logAction = message => notNullLogaction($"{"ASSERT".PadRight(10)}{Identifier}: {message}");
			Subject = subject;
		}

		protected override string Identifier => typeof(T).Name;

        protected void AssertingThat(string message)
		{
			_logAction(string.Format("Asserting that {0}.", message));
		}

		protected AndConstraint<TAssertion> And()
		{
			return new AndConstraint<TAssertion>(this as TAssertion);
		}

		protected AndWhichConstraint<TAssertion, TWhich> AndWhich<TWhich>(TWhich newSubject)
		{
			return new AndWhichConstraint<TAssertion, TWhich>(this as TAssertion, newSubject);
		}
	}

	/// <summary>
	/// Allows for changing the default (Trace.WriteLine) log behaviour of Assertions
	/// </summary>
	public static class Assertions
	{
		static Assertions()
		{
			LogAction = new AsyncLocal<Action<string>>(message => Trace.WriteLine(message));
		}

		public static AsyncLocal<Action<string>> LogAction { get; set; }
	}

}
