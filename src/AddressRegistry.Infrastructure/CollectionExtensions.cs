namespace AddressRegistry.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public static class CollectionExtensions
    {
        public static IEnumerable<ICollection<T>> SplitBySize<T>(this ICollection<T> collection, int batchSize)
        {
            var batchCount = (double)collection.Count / batchSize;

            var batchCountRounded = (int)Math.Floor(batchCount);
            if (batchCount != batchCountRounded)
            {
                batchCountRounded++;
            }

            return Enumerable.Range(0, batchCountRounded)
                .Select(batchIndex => collection.Skip(batchSize * batchIndex).Take(batchSize).ToArray());
        }
    }
}
