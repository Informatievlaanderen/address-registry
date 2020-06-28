namespace AddressRegistry.Api.Legacy.Tests.Framework.Assert
{
    using Be.Vlaanderen.Basisregisters.GrAr.Legacy.Adres;
    using FluentAssertions;

    internal class VolledigAdresAssertions : Assertions<VolledigAdres, VolledigAdresAssertions>
    {
        public VolledigAdresAssertions(VolledigAdres subject)
            : base(subject) { }

        public AndConstraint<VolledigAdresAssertions> HaveGeografischeNaam(string geografischeNaam)
        {
            AssertingThat($"'GeografischeNaam' is [{geografischeNaam}]");

            Subject.GeografischeNaam.Spelling.Should().Be(geografischeNaam);

            return And();
        }
    }
}
