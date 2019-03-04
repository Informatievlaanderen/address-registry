using System;
using System.Collections.Generic;
using System.Text;

namespace AddressRegistry.Tests.Bugfixes
{
    using Address.Commands.Crab;
    using Address.Events;

    public static class AssignOsloIdForCrabSubaddressIdExtensions
    {
        public static AddressOsloIdWasAssigned ToLegacyEvent(this AssignOsloIdForCrabSubaddressId command)
        {
            return new AddressOsloIdWasAssigned(new AddressId(command.SubaddressId.CreateDeterministicId()), command.OsloId, command.AssignmentDate);
        }
    }
}
