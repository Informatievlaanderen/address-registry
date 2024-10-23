namespace AddressRegistry.Tests
{
    using System.Collections.Generic;
    using System.Linq;
    using Infrastructure;
    using Xunit;

    public class CollectionExtensionsTests
    {
        [Theory]
        [MemberData(nameof(SplitBySizeCases))]
        public void SplitBySize(int collectionCount, int batchSize, int[] expectedBatchSizes)
        {
            var collection = Enumerable.Range(1, collectionCount).ToArray();
            var batches = collection.SplitBySize(batchSize).ToArray();
            Assert.Equal(expectedBatchSizes.Length, batches.Length);
            for (var i = 0; i < batches.Length; i++)
            {
                Assert.Equal(expectedBatchSizes[i], batches[i].Count);
            }
        }

        public static IEnumerable<object[]> SplitBySizeCases()
        {
            yield return [1, 1, new[] { 1 }];
            yield return [2, 1, new[] { 1, 1 }];
            yield return [1, 2, new[] { 1 }];
            yield return [2, 2, new[] { 2 }];
            yield return [3, 2, new[] { 2, 1 }];
        }
    }
}
