namespace AddressRegistry.Api.Legacy.Tests.Framework.Assert
{
    using FluentAssertions;
    using Legacy.AddressMatch.Responses;

    internal class AdresMatchItemGemeenteAssertions : Assertions<AdresMatchItemGemeente, AdresMatchItemGemeenteAssertions>
    {
        public AdresMatchItemGemeenteAssertions(AdresMatchItemGemeente subject)
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
