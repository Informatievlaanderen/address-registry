namespace AddressRegistry.Api.Legacy.AddressMatch
{
    using System;
    using System.Collections.Generic;

    /// <summary>
    /// A comparer that can compare two instances of a class by comparing the result of a func applied to both instances.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TProp"></typeparam>
    public class PropertyEqualityComparer<T, TProp> : IEqualityComparer<T>
    {
        private readonly Func<T, TProp> _propertyGetter;

        /// <summary>
        /// Creates a new instance of PropertyEqualityComparer
        /// </summary>
        /// <param name="propertyGetter">A func that calculates the value required for comparison</param>
        public PropertyEqualityComparer(Func<T, TProp> propertyGetter)
            => _propertyGetter = propertyGetter;

        bool IEqualityComparer<T>.Equals(T x, T y) => _propertyGetter(x).Equals(_propertyGetter(y));

        int IEqualityComparer<T>.GetHashCode(T obj) => _propertyGetter(obj).GetHashCode();
    }
}
