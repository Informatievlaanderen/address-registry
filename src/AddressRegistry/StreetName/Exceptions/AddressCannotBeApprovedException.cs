namespace AddressRegistry.StreetName.Exceptions
{
    public class AddressCannotBeApprovedException : AddressRegistryException
    {
        public AddressCannotBeApprovedException(AddressStatus status)
            : base($"Address status '{status}' is invalid for approval.")
        { }
    }
}
