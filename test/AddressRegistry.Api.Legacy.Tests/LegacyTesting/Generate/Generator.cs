namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Generate
{
    using System;

    public class Generator<T>
    {
        private readonly Func<Random, T> generate;

        public Generator(Func<Random, T> generate)
        {
            if (generate == null)
                throw new ArgumentNullException(nameof(generate));

            this.generate = generate;
        }

        public Generator<T1> Select<T1>(Func<T, T1> f)
        {
            if (f == null)
                throw new ArgumentNullException(nameof(f));

            Func<Random, T1> newGenerator = r => f(this.generate(r));
            return new Generator<T1>(newGenerator);
        }

        public Generator<T1> Then<T1>(Func<T, Generator<T1>> f)
        {
            if (f == null)
                throw new ArgumentNullException(nameof(f));

            Func<Random, T1> newGenerator = r => f(this.generate(r)).Generate(r);
            return new Generator<T1>(newGenerator);
        }

        public T Generate(Random random)
        {
            if (random == null)
                throw new ArgumentNullException(nameof(random));

            return generate(random);
        }
    }
}
