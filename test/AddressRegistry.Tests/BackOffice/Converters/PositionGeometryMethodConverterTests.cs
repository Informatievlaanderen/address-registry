namespace AddressRegistry.Tests.BackOffice.Converters
{
    using AddressRegistry.Api.BackOffice.Abstractions.Converters;
    using Be.Vlaanderen.Basisregisters.GrAr.Edit.Contracts;
    using FluentAssertions;
    using StreetName;
    using Xunit;

    public class PositionGeometryMethodConverterTests
    {
        [Fact]
        public void WhenNoPositionGeometryMethodProvided_ThenReturnNull()
        {
            PositieGeometrieMethode? methode = null;

            var result = methode.Map();

            result.Should().BeNull();
        }

        [Fact]
        public void IsMappedToDerivedFromObject()
        {
            PositieGeometrieMethode? methode = PositieGeometrieMethode.AfgeleidVanObject;

            var result = methode.Map();

            result.Should().Be(GeometryMethod.DerivedFromObject);
        }

        [Fact]
        public void IsMappedToAppointedByAdministrator()
        {
            PositieGeometrieMethode? methode = PositieGeometrieMethode.AangeduidDoorBeheerder;

            var result = methode.Map();

            result.Should().Be(GeometryMethod.AppointedByAdministrator);
        }
    }
}
