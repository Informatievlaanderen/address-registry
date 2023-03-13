namespace AddressRegistry.Tests.ApiTests.AddressMatch.Asserts
{
    using AddressRegistry.Api.Oslo.AddressMatch.Responses;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using FluentAssertions;

    internal class AdresMatchItemAssertions : Assertions<AdresMatchOsloItem, AdresMatchItemAssertions>
    {
        public AdresMatchItemAssertions(AdresMatchOsloItem subject)
            : base(subject) { }

        public AndWhichConstraint<AdresMatchItemAssertions, AdresMatchOsloItemGemeente> HaveGemeente()
        {
            AssertingThat("'Gemeente' is not null");

            Subject.Gemeente.Should().NotBeNull();

            return AndWhich(Subject.Gemeente);
        }

        public AndWhichConstraint<AdresMatchItemAssertions, AdresMatchOsloItemStraatnaam> HaveStraatnaam()
        {
            AssertingThat("'Straatnaam' is not null");

            Subject.Straatnaam.Should().NotBeNull();

            return AndWhich(Subject.Straatnaam);
        }

        public AndWhichConstraint<AdresMatchItemAssertions, VolledigAdres> HaveVolledigAdres()
        {
            AssertingThat("'VolledigAdres' is not null");

            Subject.VolledigAdres.Should().NotBeNull();

            return AndWhich(Subject.VolledigAdres);
        }

        public AndConstraint<AdresMatchItemAssertions> HaveNoStraatnaam()
        {
            AssertingThat("'Straatnaam' is null");

            Subject.Straatnaam.Should().BeNull();

            return And();
        }

        public AndConstraint<AdresMatchItemAssertions> NotHaveVolledigAdres()
        {
            AssertingThat("'VolledigAdres' is null");

            Subject.VolledigAdres.Should().BeNull();

            return And();
        }

        public AndConstraint<AdresMatchItemAssertions> HaveScore(int score)
        {
            AssertingThat($"the score is {score}%");

            Subject.Score.Should().Be(score);

            return And();
        }
    }
}
