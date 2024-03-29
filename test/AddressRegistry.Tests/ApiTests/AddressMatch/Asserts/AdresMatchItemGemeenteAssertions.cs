namespace AddressRegistry.Tests.ApiTests.AddressMatch.Asserts
{
    using AddressRegistry.Api.Oslo.AddressMatch.Responses;
    using FluentAssertions;

    internal class AdresMatchItemGemeenteAssertions : Assertions<AdresMatchOsloItemGemeente, AdresMatchItemGemeenteAssertions>
    {
        public AdresMatchItemGemeenteAssertions(AdresMatchOsloItemGemeente subject)
            : base(subject) { }

        public AndConstraint<AdresMatchItemGemeenteAssertions> HaveGemeentenaam(string expectedGemeentenaam)
        {
            AssertingThat($"'Gemeentenaam' is [{expectedGemeentenaam}]");

            Subject.Gemeentenaam.GeografischeNaam.Spelling.Should().Be(expectedGemeentenaam);

            return And();
        }

        public AndConstraint<AdresMatchItemGemeenteAssertions> HaveObjectId(string expectedObjectId)
        {
            AssertingThat($"'ObjectId' is [{expectedObjectId}]");

            Subject.ObjectId.Should().Be(expectedObjectId);

            return And();
        }
    }
}
