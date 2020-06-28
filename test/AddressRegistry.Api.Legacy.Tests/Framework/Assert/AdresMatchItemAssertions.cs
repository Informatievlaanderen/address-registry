namespace AddressRegistry.Api.Legacy.Tests.Framework.Assert
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using FluentAssertions;
    using Legacy.AddressMatch.Responses;

    internal class AdresMatchItemAssertions : Assertions<AdresMatchItem, AdresMatchItemAssertions>
    {
        public AdresMatchItemAssertions(AdresMatchItem subject)
            : base(subject) { }

        public AndWhichConstraint<AdresMatchItemAssertions, AdresMatchItemGemeente> HaveGemeente()
        {
            AssertingThat("'Gemeente' is not null");

            Subject.Gemeente.Should().NotBeNull();

            return AndWhich(Subject.Gemeente);
        }

        public AndWhichConstraint<AdresMatchItemAssertions, AdresMatchItemStraatnaam> HaveStraatnaam()
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

        public AndConstraint<AdresMatchItemAssertions> BeExactMatch()
        {
            AssertingThat("The match is exact (Score=100%)");

            Subject.Score.Should().Be(100);

            return And();
        }

        public AndConstraint<AdresMatchItemAssertions> NotBeExactMatch()
        {
            AssertingThat("The match is not exact (Score<100%)");

            Subject.Score.Should().BeLessThan(100);

            return And();
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
