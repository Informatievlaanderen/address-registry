namespace AddressRegistry.Api.CrabImport.CrabImport
{
    using System;
    using System.Collections.Generic;

    /// <inheritdoc />
    /// <summary>
    /// Allow IEqualityComparer to be configured within a lambda expression.
    /// From https://stackoverflow.com/questions/98033/wrap-a-delegate-in-an-iequalitycomparer
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class LambdaEqualityComparer<T> : IEqualityComparer<T>
    {
        private readonly Func<T, T, bool> _comparer;
        private readonly Func<T, int> _hash;

        /// <inheritdoc />
        /// <summary>
        /// Simplest constructor, provide a conversion to string for type T to use as a comparison key (GetHashCode() and Equals().
        /// https://stackoverflow.com/questions/98033/wrap-a-delegate-in-an-iequalitycomparer, user "orip"
        /// </summary>
        /// <param name="toString"></param>
        public LambdaEqualityComparer(Func<T, string> toString)
            : this((t1, t2) => toString(t1) == toString(t2), t => toString(t).GetHashCode())
        {
        }

        /// <inheritdoc />
        /// <summary>
        /// Constructor.  Assumes T.GetHashCode() is accurate.
        /// </summary>
        /// <param name="comparer"></param>
        public LambdaEqualityComparer(Func<T, T, bool> comparer)
            : this(comparer, t => t.GetHashCode())
        {
        }

        /// <summary>
        /// Constructor, provide a equality comparer and a hash.
        /// </summary>
        /// <param name="comparer"></param>
        /// <param name="hash"></param>
        public LambdaEqualityComparer(Func<T, T, bool> comparer, Func<T, int> hash)
        {
            _comparer = comparer;
            _hash = hash;
        }

        public bool Equals(T x, T y) => _comparer(x, y);

        public int GetHashCode(T obj) => _hash(obj);
    }
}
