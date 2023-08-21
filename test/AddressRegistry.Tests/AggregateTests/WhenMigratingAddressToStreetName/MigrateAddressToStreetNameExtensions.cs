namespace AddressRegistry.Tests.AggregateTests.WhenMigratingAddressToStreetName
{
    using Address;
    using StreetName.Commands;
    using AddressGeometry = Address.AddressGeometry;
    using AddressId = Address.AddressId;
    using AddressStatus = Address.AddressStatus;
    using BoxNumber = Address.BoxNumber;
    using ExtendedWkbGeometry = Address.ExtendedWkbGeometry;
    using GeometryMethod = Address.GeometryMethod;
    using GeometrySpecification = Address.GeometrySpecification;
    using HouseNumber = Address.HouseNumber;
    using PostalCode = Address.PostalCode;
    using StreetNameId = Address.StreetNameId;

    public static class MigrateAddressToStreetNameExtensions
    {
        public static MigrateAddressToStreetName WithoutParentAddressId(this MigrateAddressToStreetName command)
        {
            return new MigrateAddressToStreetName(
                new AddressId(command.AddressId),
                command.StreetNamePersistentLocalId,
                new StreetNameId(command.StreetNameId),
                new PersistentLocalId(command.AddressPersistentLocalId),
                (AddressStatus)command.Status,
                new HouseNumber(command.HouseNumber),
                new BoxNumber(command.BoxNumber),
                new AddressGeometry(
                    (GeometryMethod)command.Geometry.GeometryMethod,
                    (GeometrySpecification)command.Geometry.GeometrySpecification,
                    new ExtendedWkbGeometry(command.Geometry.Geometry.ToString())),
                command.OfficiallyAssigned,
                string.IsNullOrEmpty(command.PostalCode) ? null : new PostalCode(command.PostalCode),
                command.IsCompleted,
                command.IsRemoved,
                null,
                command.Provenance);
        }

        public static MigrateAddressToStreetName WithPostalCode(this MigrateAddressToStreetName command, string? postalCode)
        {
            return new MigrateAddressToStreetName(
                new AddressId(command.AddressId),
                command.StreetNamePersistentLocalId,
                new StreetNameId(command.StreetNameId),
                new PersistentLocalId(command.AddressPersistentLocalId),
                (AddressStatus)command.Status,
                new HouseNumber(command.HouseNumber),
                new BoxNumber(command.BoxNumber),
                new AddressGeometry(
                    (GeometryMethod)command.Geometry.GeometryMethod,
                    (GeometrySpecification)command.Geometry.GeometrySpecification,
                    new ExtendedWkbGeometry(command.Geometry.Geometry.ToString())),
                command.OfficiallyAssigned,
                string.IsNullOrEmpty(postalCode) ? null : new PostalCode(postalCode),
                command.IsCompleted,
                command.IsRemoved,
                command.ParentAddressId is not null ? new AddressId(command.ParentAddressId) : null,
                command.Provenance);
        }

        public static MigrateAddressToStreetName WithParentAddressId(this MigrateAddressToStreetName command, AddressId parentId)
        {
            return new MigrateAddressToStreetName(
                new AddressId(command.AddressId),
                command.StreetNamePersistentLocalId,
                new StreetNameId(command.StreetNameId),
                new PersistentLocalId(command.AddressPersistentLocalId),
                (AddressStatus)command.Status,
                new HouseNumber(command.HouseNumber),
                new BoxNumber(command.BoxNumber),
                new AddressGeometry(
                    (GeometryMethod)command.Geometry.GeometryMethod,
                    (GeometrySpecification)command.Geometry.GeometrySpecification,
                    new ExtendedWkbGeometry(command.Geometry.Geometry.ToString())),
                command.OfficiallyAssigned,
                string.IsNullOrEmpty(command.PostalCode) ? null : new PostalCode(command.PostalCode),
                command.IsCompleted,
                command.IsRemoved,
                parentId,
                command.Provenance);
        }

        public static MigrateAddressToStreetName WithStatus(this MigrateAddressToStreetName command, AddressStatus addressStatus)
        {
            return new MigrateAddressToStreetName(
                new AddressId(command.AddressId),
                command.StreetNamePersistentLocalId,
                new StreetNameId(command.StreetNameId),
                new PersistentLocalId(command.AddressPersistentLocalId),
                addressStatus,
                new HouseNumber(command.HouseNumber),
                new BoxNumber(command.BoxNumber),
                new AddressGeometry(
                    (GeometryMethod)command.Geometry.GeometryMethod,
                    (GeometrySpecification)command.Geometry.GeometrySpecification,
                    new ExtendedWkbGeometry(command.Geometry.Geometry.ToString())),
                command.OfficiallyAssigned,
                string.IsNullOrEmpty(command.PostalCode) ? null : new PostalCode(command.PostalCode),
                command.IsCompleted,
                command.IsRemoved,
                command.ParentAddressId is not null ? new AddressId(command.ParentAddressId) : null,
                command.Provenance);
        }
    }
}
