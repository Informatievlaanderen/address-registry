namespace AddressRegistry.Api.BackOffice.Abstractions.Validation
{
    using TicketingService.Abstractions;

    public static partial class ValidationErrors
    {
        public static class CorrectRemoval
        {
            public static class ParentInvalidStatus
            {
                public const string Code = "AdresHuisnummerTODO";
                public const string Message = "TODO";

                public static TicketError ToTicketError() => new(Message, Code);
            }
        }
    }
}
