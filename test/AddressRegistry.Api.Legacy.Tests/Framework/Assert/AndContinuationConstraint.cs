﻿namespace AddressRegistry.Api.Legacy.Tests.Framework.Assert
{
    using System.Threading.Tasks;
    using FluentAssertions;

    internal class AndContinuationConstraint<TResult, TAssertion> : AndConstraint<TAssertion>
        where TAssertion:TaskContinuationAssertion<TResult, TAssertion>
    {
        public Task<TResult> Continuation => And.Subject;

        public AndContinuationConstraint(TAssertion parentConstraint)
            : base(parentConstraint) { }
    }
}
