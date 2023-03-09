namespace AddressRegistry.Tests.ApiTests.Oslo_Legacy.Framework.Generate
{
    using System;

    public class Generator<T>
    {
        private readonly Func<Random, T> _generate;

        public Generator(Func<Random, T> generate)
            => _generate = generate ?? throw new ArgumentNullException(nameof(generate));

        public Generator<T1> Select<T1>(Func<T, T1> f)
        {
            if (f == null)
                throw new ArgumentNullException(nameof(f));

            return new Generator<T1>(r => f(_generate(r)));
        }

        public Generator<T1> Then<T1>(Func<T, Generator<T1>> f)
        {
            if (f == null)
                throw new ArgumentNullException(nameof(f));

            return new Generator<T1>(r => f(_generate(r)).Generate(r));
        }

        public T Generate(Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            return _generate(random);
        }
    }
}
