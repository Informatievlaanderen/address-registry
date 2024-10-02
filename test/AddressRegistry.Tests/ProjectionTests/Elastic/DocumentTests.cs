namespace AddressRegistry.Tests.ProjectionTests.Elastic
{
    using System;
    using System.Linq;
    using AddressRegistry.StreetName;
    using FluentAssertions;
    using global::AutoFixture;
    using Infrastructure.Elastic;
    using NetTopologySuite.Geometries;
    using Projections.Elastic.AddressSearch;
    using Xunit;
    using StreetName = Projections.Elastic.AddressSearch.StreetName;

    public class DocumentTests
    {
        [Fact]
        public void PartialUpdateDocument_PropertiesShouldMatchAddressSearchDocument()
        {
            var partialProperties = typeof(AddressSearchPartialDocument).GetProperties();
            var documentProperties = typeof(AddressSearchDocument).GetProperties();

            partialProperties.Should().NotBeEmpty();

            foreach (var partialProperty in partialProperties)
            {
                var documentProperty = documentProperties.SingleOrDefault(x => x.Name == partialProperty.Name);
                documentProperty.Should().NotBeNull();
                var nonNullablePartialPropertyType = Nullable.GetUnderlyingType(partialProperty.PropertyType) ?? partialProperty.PropertyType;
                nonNullablePartialPropertyType.Should().Be(documentProperty!.PropertyType);
            }
        }

        [Theory]
        [InlineData(AddressStatus.Proposed, true)]
        [InlineData(AddressStatus.Current, true)]
        [InlineData(AddressStatus.Rejected, false)]
        [InlineData(AddressStatus.Retired, false)]
        [InlineData(AddressStatus.Unknown, false)]
        public void AddressSearchDocument_Active(AddressStatus status, bool active)
        {
            var fixture = new Fixture();
            var document = fixture.Create<AddressSearchDocument>();
            document.Status = status;

            document.Active.Should().Be(active);
        }

        [Fact]
        public void AddressSearchDocument_FullAddress()
        {
            var fixture = new Fixture();
            var document = fixture.Create<AddressSearchDocument>();
            document.Municipality = new Municipality("12345", [
                new Name("Gemeente", Language.nl),
                new Name("Ville", Language.fr)
            ]);
            document.PostalInfo = new PostalInfo("1234", [
                new Name("Postcode", Language.nl),
                new Name("Code postal", Language.fr)
            ]);
            document.StreetName = new StreetName(1, [
                new Name("Straatnaam", Language.nl),
                new Name("Rue", Language.fr)
            ], [
                new Name("Homonym", Language.nl),
                new Name("Homonym FR", Language.fr)
            ]);
            document.HouseNumber = "123";
            document.BoxNumber = "A";

            document.FullAddress.Length.Should().Be(2);
            document.FullAddress[0].Language.Should().Be(Language.nl);
            document.FullAddress[0].Spelling.Should().Be("Straatnaam 123 bus A, 1234 Gemeente");
            document.FullAddress[1].Language.Should().Be(Language.fr);
            document.FullAddress[1].Spelling.Should().Be("Rue 123 boîte A, 1234 Ville");
        }

        [Fact]
        public void AddressSearchDocument_FullAddress_WithoutPostalInfo()
        {
            var fixture = new Fixture();
            var document = fixture.Create<AddressSearchDocument>();
            document.Municipality = new Municipality("12345", [
                new Name("Gemeente", Language.nl),
                new Name("Ville", Language.fr)
            ]);
            document.PostalInfo = null;
            document.StreetName = new StreetName(1, [
                new Name("Straatnaam", Language.nl),
                new Name("Rue", Language.fr)
            ], [
                new Name("Homonym", Language.nl),
                new Name("Homonym FR", Language.fr)
            ]);
            document.HouseNumber = "123";
            document.BoxNumber = "A";

            document.FullAddress.Length.Should().Be(2);
            document.FullAddress[0].Language.Should().Be(Language.nl);
            document.FullAddress[0].Spelling.Should().Be("Straatnaam 123 bus A, Gemeente");
            document.FullAddress[1].Language.Should().Be(Language.fr);
            document.FullAddress[1].Spelling.Should().Be("Rue 123 boîte A, Ville");
        }

        [Fact]
        public void AddressSearchDocument_FullAddress_WithoutBoxNumber()
        {
            var fixture = new Fixture();
            var document = fixture.Create<AddressSearchDocument>();
            document.Municipality = new Municipality("12345", [
                new Name("Gemeente", Language.nl),
                new Name("Ville", Language.fr)
            ]);
            document.PostalInfo = new PostalInfo("1234", [
                new Name("Postcode", Language.nl),
                new Name("Code postal", Language.fr)
            ]);
            document.StreetName = new StreetName(1, [
                new Name("Straatnaam", Language.nl),
                new Name("Rue", Language.fr)
            ], [
                new Name("Homonym", Language.nl),
                new Name("Homonym FR", Language.fr)
            ]);
            document.HouseNumber = "123";
            document.BoxNumber = null;

            document.FullAddress.Length.Should().Be(2);
            document.FullAddress[0].Language.Should().Be(Language.nl);
            document.FullAddress[0].Spelling.Should().Be("Straatnaam 123, 1234 Gemeente");
            document.FullAddress[1].Language.Should().Be(Language.fr);
            document.FullAddress[1].Spelling.Should().Be("Rue 123, 1234 Ville");
        }

        [Fact]
        public void AddressSearchDocument_AddressPosition()
        {
            var fixture = new Fixture();
            var document = fixture.Create<AddressSearchDocument>();

            document.AddressPosition = new AddressPosition(
                new Point(73862.07, 211634.58),
                fixture.Create<GeometryMethod>(),
                fixture.Create<GeometrySpecification>()
            );

            document.AddressPosition.GeometryAsWkt.Should().Be("POINT (73862.07 211634.58)");
            document.AddressPosition.GeometryAsWgs84.Should().Be("3.277957970797176, 51.20937520963882");
        }
    }
}
