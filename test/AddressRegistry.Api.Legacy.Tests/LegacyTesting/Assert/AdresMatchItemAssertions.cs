namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Assert
{
    using AddressMatch.Responses;
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using FluentAssertions;

    public class AdresMatchItemAssertions : Assertions<AdresMatchItem, AdresMatchItemAssertions>
    {
        public AdresMatchItemAssertions(AdresMatchItem subject) : base(subject)
        {
        }

        public AndWhichConstraint<AdresMatchItemAssertions, AdresMatchItemGemeente> HaveGemeente()
        {
            AssertingThat("'Gemeente' is not null");

            Subject.Gemeente.Should().NotBeNull();

            return AndWhich(Subject.Gemeente);
        }

        internal AndWhichConstraint<AdresMatchItemAssertions, AdresMatchItemStraatnaam> HaveStraatnaam()
        {
            AssertingThat("'Straatnaam' is not null");

            Subject.Straatnaam.Should().NotBeNull();

            return AndWhich(Subject.Straatnaam);
        }

        internal AndWhichConstraint<AdresMatchItemAssertions, VolledigAdres> HaveVolledigAdres()
        {
            AssertingThat("'VolledigAdres' is not null");

            Subject.VolledigAdres.Should().NotBeNull();

            return AndWhich(Subject.VolledigAdres);
        }

        internal AndConstraint<AdresMatchItemAssertions> BeExactMatch()
        {
            AssertingThat("The match is exact (Score=100%)");

            Subject.Score.Should().Be(100);

            return And();
        }

        internal AndConstraint<AdresMatchItemAssertions> NotBeExactMatch()
        {
            AssertingThat("The match is not exact (Score<100%)");

            Subject.Score.Should().BeLessThan(100);

            return And();
        }

        internal AndConstraint<AdresMatchItemAssertions> HaveNoStraatnaam()
        {
            AssertingThat("'Straatnaam' is null");

            Subject.Straatnaam.Should().BeNull();

            return And();
        }

        internal AndConstraint<AdresMatchItemAssertions> NotHaveVolledigAdres()
        {
            AssertingThat("'VolledigAdres' is null");

            Subject.VolledigAdres.Should().BeNull();

            return And();
        }

        internal AndConstraint<AdresMatchItemAssertions> HaveScore(int score)
        {
            AssertingThat($"the score is {score}%");

            Subject.Score.Should().Be(score);

            return And();
        }
    }
}
