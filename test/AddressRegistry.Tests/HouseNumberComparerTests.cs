namespace AddressRegistry.Tests
{
    using System.Linq;
    using FluentAssertions;
    using StreetName;
    using Xunit;

    public class HouseNumberComparerTests
    {
        private readonly HouseNumberComparer _sut = new();

        [Fact]
        public void OrderHouseNumbersWithDigitsOnly()
        {
            var unordered = new[]
            {
                new HouseNumber("5"),
                new HouseNumber("9"),
                new HouseNumber("10"),
                new HouseNumber("1")
            };

            var ordered = unordered.OrderBy(i => i, _sut).ToArray();

            ordered[0].Should().Be(new HouseNumber("1"));
            ordered[1].Should().Be(new HouseNumber("5"));
            ordered[2].Should().Be(new HouseNumber("9"));
            ordered[3].Should().Be(new HouseNumber("10"));
        }

        [Fact]
        public void OrderHouseNumbersWithDigitsAndLetters()
        {
            var unordered = new[]
            {
                new HouseNumber("5"),
                new HouseNumber("5A"),
                new HouseNumber("10"),
                new HouseNumber("10K"),
                new HouseNumber("1"),
                new HouseNumber("1C")
            };

            var ordered = unordered.OrderBy(i => i, _sut).ToArray();

            ordered[0].Should().Be(new HouseNumber("1"));
            ordered[1].Should().Be(new HouseNumber("1C"));
            ordered[2].Should().Be(new HouseNumber("5"));
            ordered[3].Should().Be(new HouseNumber("5A"));
            ordered[4].Should().Be(new HouseNumber("10"));
            ordered[5].Should().Be(new HouseNumber("10K"));
        }

        [Fact]
        public void OrderHouseNumberWithLettersOnly()
        {
            var unordered = new[]
            {
                new HouseNumber("D"),
                new HouseNumber("A"),
                new HouseNumber("Z"),
                new HouseNumber("K")
            };

            var ordered = unordered.OrderBy(i => i, _sut).ToArray();

            ordered[0].Should().Be(new HouseNumber("A"));
            ordered[1].Should().Be(new HouseNumber("D"));
            ordered[2].Should().Be(new HouseNumber("K"));
            ordered[3].Should().Be(new HouseNumber("Z"));
        }

        [Fact]
        public void OrderHouseNumberWithLettersOnlyAndDigitsOnly()
        {
            var unordered = new[]
            {
                new HouseNumber("5"),
                new HouseNumber("5A"),
                new HouseNumber("A"),
                new HouseNumber("10"),
                new HouseNumber("10K"),
                new HouseNumber("I"),
                new HouseNumber("1"),
                new HouseNumber("1C"),
                new HouseNumber("100"),
                new HouseNumber("Z")
            };

            var ordered = unordered.OrderBy(i => i, _sut).ToArray();

            ordered[0].Should().Be(new HouseNumber("1"));
            ordered[1].Should().Be(new HouseNumber("1C"));
            ordered[2].Should().Be(new HouseNumber("5"));
            ordered[3].Should().Be(new HouseNumber("5A"));
            ordered[4].Should().Be(new HouseNumber("10"));
            ordered[5].Should().Be(new HouseNumber("10K"));
            ordered[6].Should().Be(new HouseNumber("100"));
            ordered[7].Should().Be(new HouseNumber("A"));
            ordered[8].Should().Be(new HouseNumber("I"));
            ordered[9].Should().Be(new HouseNumber("Z"));
        }
    }
}
