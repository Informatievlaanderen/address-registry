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
        public void IsMappedToDerivedFromObject()
        {
            var result = PositieGeometrieMethode.AfgeleidVanObject.Map();

            result.Should().Be(GeometryMethod.DerivedFromObject);
        }

        [Fact]
        public void IsMappedToAppointedByAdministrator()
        {
            var result = PositieGeometrieMethode.AangeduidDoorBeheerder.Map();

            result.Should().Be(GeometryMethod.AppointedByAdministrator);
        }
    }
}
