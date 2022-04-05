namespace AddressRegistry.Tests
{
    using System;
    using Address.Commands.Crab;
    using Address.Events;
    using Address.Events.Crab;
    using Address.ValueObjects;
    using Address.ValueObjects.Crab;
    using Be.Vlaanderen.Basisregisters.Crab;
    using NodaTime;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;

    public static class EventExtensions
    {
        public static AddressHouseNumberStatusWasImportedFromCrab WithStatus(this AddressHouseNumberStatusWasImportedFromCrab @event, CrabAddressStatus status)
        {
            return new AddressHouseNumberStatusWasImportedFromCrab(
                new CrabHouseNumberStatusId(@event.HouseNumberStatusId),
                new CrabHouseNumberId(@event.HouseNumberId),
                status,
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation
                );
        }

        public static AddressHouseNumberStatusWasImportedFromCrab WithHouseNumberStatusId(this AddressHouseNumberStatusWasImportedFromCrab @event, CrabHouseNumberStatusId statusId)
        {
            return new AddressHouseNumberStatusWasImportedFromCrab(
                statusId,
                new CrabHouseNumberId(@event.HouseNumberId),
                @event.AddressStatus,
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation
            );
        }

        public static AddressHouseNumberStatusWasImportedFromCrab WithBeginDate(this AddressHouseNumberStatusWasImportedFromCrab @event, DateTimeOffset? beginDate)
        {
            return @event.WithBeginDate(beginDate.HasValue ? LocalDateTime.FromDateTime(beginDate.Value.LocalDateTime) : new LocalDateTime?());
        }

        public static AddressHouseNumberStatusWasImportedFromCrab WithBeginDate(this AddressHouseNumberStatusWasImportedFromCrab @event, LocalDateTime? beginDate)
        {
            return new AddressHouseNumberStatusWasImportedFromCrab(
                new CrabHouseNumberStatusId(@event.HouseNumberStatusId),
                new CrabHouseNumberId(@event.HouseNumberId),
                @event.AddressStatus,
                new CrabLifetime(beginDate, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation
            );
        }

        public static AddressHouseNumberStatusWasImportedFromCrab WithCrabModification(this AddressHouseNumberStatusWasImportedFromCrab @event, CrabModification modification)
        {
            return new AddressHouseNumberStatusWasImportedFromCrab(
                new CrabHouseNumberStatusId(@event.HouseNumberStatusId),
                new CrabHouseNumberId(@event.HouseNumberId),
                @event.AddressStatus,
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                modification,
                @event.Organisation
            );


        }

        public static AddressHouseNumberPositionWasImportedFromCrab WithWkbGeometry(this AddressHouseNumberPositionWasImportedFromCrab @event, WkbGeometry wkbGeometry)
        {
            return new AddressHouseNumberPositionWasImportedFromCrab(
                new CrabAddressPositionId(@event.AddressPositionId),
                new CrabHouseNumberId(@event.HouseNumberId),
                wkbGeometry,
                @event.AddressPositionOrigin,
                new CrabAddressNature(@event.AddressNature),
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressHouseNumberPositionWasImportedFromCrab WithCrabAddressPositionOrigin(this AddressHouseNumberPositionWasImportedFromCrab @event, CrabAddressPositionOrigin addressPositionOrigin)
        {
            return new AddressHouseNumberPositionWasImportedFromCrab(
                new CrabAddressPositionId(@event.AddressPositionId),
                new CrabHouseNumberId(@event.HouseNumberId),
                new WkbGeometry(@event.AddressPosition),
                addressPositionOrigin,
                new CrabAddressNature(@event.AddressNature),
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressHouseNumberPositionWasImportedFromCrab WithModification(this AddressHouseNumberPositionWasImportedFromCrab @event, CrabModification modification)
        {
            return new AddressHouseNumberPositionWasImportedFromCrab(
                new CrabAddressPositionId(@event.AddressPositionId),
                new CrabHouseNumberId(@event.HouseNumberId),
                new WkbGeometry(@event.AddressPosition),
                @event.AddressPositionOrigin,
                new CrabAddressNature(@event.AddressNature),
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                modification,
                @event.Organisation);
        }

        public static AddressHouseNumberPositionWasImportedFromCrab WithBeginDate(this AddressHouseNumberPositionWasImportedFromCrab @event, DateTimeOffset? beginDate)
        {
            return @event.WithBeginDate(beginDate.HasValue ? LocalDateTime.FromDateTime(beginDate.Value.LocalDateTime) : new LocalDateTime?());
        }

        public static AddressHouseNumberPositionWasImportedFromCrab WithBeginDate(this AddressHouseNumberPositionWasImportedFromCrab @event, LocalDateTime? beginDate)
        {
            return new AddressHouseNumberPositionWasImportedFromCrab(
                new CrabAddressPositionId(@event.AddressPositionId),
                new CrabHouseNumberId(@event.HouseNumberId),
                new WkbGeometry(@event.AddressPosition),
                @event.AddressPositionOrigin,
                new CrabAddressNature(@event.AddressNature),
                new CrabLifetime(beginDate, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressHouseNumberPositionWasImportedFromCrab WithEndDate(this AddressHouseNumberPositionWasImportedFromCrab @event, DateTimeOffset? endDate)
        {
            return @event.WithEndDate(endDate.HasValue ? LocalDateTime.FromDateTime(endDate.Value.LocalDateTime) : new LocalDateTime?());
        }

        public static AddressHouseNumberPositionWasImportedFromCrab WithEndDate(this AddressHouseNumberPositionWasImportedFromCrab @event, LocalDateTime? endDate)
        {
            return new AddressHouseNumberPositionWasImportedFromCrab(
                new CrabAddressPositionId(@event.AddressPositionId),
                new CrabHouseNumberId(@event.HouseNumberId),
                new WkbGeometry(@event.AddressPosition),
                @event.AddressPositionOrigin,
                new CrabAddressNature(@event.AddressNature),
                new CrabLifetime(@event.BeginDateTime, endDate),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressHouseNumberMailCantonWasImportedFromCrab WithPostalCode(this AddressHouseNumberMailCantonWasImportedFromCrab @event, string postalCode)
        {
            return new AddressHouseNumberMailCantonWasImportedFromCrab(
                new CrabHouseNumberMailCantonId(@event.HouseNumberMailCantonId),
                new CrabHouseNumberId(@event.HouseNumberId),
                new CrabMailCantonId(@event.MailCantonId),
                new CrabMailCantonCode(postalCode),
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressHouseNumberMailCantonWasImportedFromCrab WithBeginDate(this AddressHouseNumberMailCantonWasImportedFromCrab @event, DateTimeOffset? beginDate)
        {
            return @event.WithBeginDate(beginDate.HasValue ? LocalDateTime.FromDateTime(beginDate.Value.LocalDateTime) : new LocalDateTime?());
        }

        public static AddressHouseNumberMailCantonWasImportedFromCrab WithBeginDate(this AddressHouseNumberMailCantonWasImportedFromCrab @event, LocalDateTime? beginDate)
        {
            return new AddressHouseNumberMailCantonWasImportedFromCrab(
                new CrabHouseNumberMailCantonId(@event.HouseNumberMailCantonId),
                new CrabHouseNumberId(@event.HouseNumberId),
                new CrabMailCantonId(@event.MailCantonId),
                new CrabMailCantonCode(@event.MailCantonCode),
                new CrabLifetime(beginDate, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressHouseNumberMailCantonWasImportedFromCrab WithHouseNumberMailCantonId(this AddressHouseNumberMailCantonWasImportedFromCrab @event, CrabHouseNumberMailCantonId houseNumberMailCantonId)
        {
            return new AddressHouseNumberMailCantonWasImportedFromCrab(
                houseNumberMailCantonId,
                new CrabHouseNumberId(@event.HouseNumberId),
                new CrabMailCantonId(@event.MailCantonId),
                new CrabMailCantonCode(@event.MailCantonCode),
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressHouseNumberMailCantonWasImportedFromCrab WithCrabModification(this AddressHouseNumberMailCantonWasImportedFromCrab @event, CrabModification modification)
        {
            return new AddressHouseNumberMailCantonWasImportedFromCrab(
                new CrabHouseNumberMailCantonId(@event.HouseNumberMailCantonId),
                new CrabHouseNumberId(@event.HouseNumberId),
                new CrabMailCantonId(@event.MailCantonId),
                new CrabMailCantonCode(@event.MailCantonCode),
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                modification,
                @event.Organisation);
        }

        public static ImportHouseNumberMailCantonFromCrab ToCommand(this AddressHouseNumberMailCantonWasImportedFromCrab @event)
        {
            return new ImportHouseNumberMailCantonFromCrab(
                new CrabHouseNumberMailCantonId(@event.HouseNumberMailCantonId),
                new CrabHouseNumberId(@event.HouseNumberId),
                new CrabMailCantonId(@event.MailCantonId),
                new CrabMailCantonCode(@event.MailCantonCode),
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressSubaddressWasImportedFromCrab WithBeginDate(this AddressSubaddressWasImportedFromCrab @event, LocalDateTime? beginDate)
        {
            return new AddressSubaddressWasImportedFromCrab(
                new CrabSubaddressId(@event.SubaddressId),
                new CrabHouseNumberId(@event.HouseNumberId),
                new BoxNumber(@event.BoxNumber),
                new CrabBoxNumberType(@event.BoxNumberType),
                new CrabLifetime(beginDate, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressSubaddressWasImportedFromCrab WithBoxNumber(this AddressSubaddressWasImportedFromCrab @event, BoxNumber boxNumber)
        {
            return new AddressSubaddressWasImportedFromCrab(
                new CrabSubaddressId(@event.SubaddressId),
                new CrabHouseNumberId(@event.HouseNumberId),
                boxNumber,
                new CrabBoxNumberType(@event.BoxNumberType),
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressSubaddressStatusWasImportedFromCrab WithStatus(this AddressSubaddressStatusWasImportedFromCrab @event, CrabAddressStatus status)
        {
            return new AddressSubaddressStatusWasImportedFromCrab(
                new CrabSubaddressStatusId(@event.SubaddressStatusId),
                new CrabSubaddressId(@event.SubaddressId),
                status,
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation
                );
        }

        public static AddressSubaddressStatusWasImportedFromCrab WithSubaddressStatusId(this AddressSubaddressStatusWasImportedFromCrab @event, CrabSubaddressStatusId statusId)
        {
            return new AddressSubaddressStatusWasImportedFromCrab(
                statusId,
                new CrabSubaddressId(@event.SubaddressId),
                @event.AddressStatus,
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation
            );
        }

        public static AddressSubaddressStatusWasImportedFromCrab WithBeginDate(this AddressSubaddressStatusWasImportedFromCrab @event, DateTimeOffset? beginDate)
        {
            return @event.WithBeginDate(beginDate.HasValue ? LocalDateTime.FromDateTime(beginDate.Value.LocalDateTime) : new LocalDateTime?());
        }

        public static AddressSubaddressStatusWasImportedFromCrab WithBeginDate(this AddressSubaddressStatusWasImportedFromCrab @event, LocalDateTime? beginDate)
        {
            return new AddressSubaddressStatusWasImportedFromCrab(
                new CrabSubaddressStatusId(@event.SubaddressStatusId),
                new CrabSubaddressId(@event.SubaddressId),
                @event.AddressStatus,
                new CrabLifetime(beginDate, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation
            );
        }

        public static AddressSubaddressStatusWasImportedFromCrab WithCrabModification(this AddressSubaddressStatusWasImportedFromCrab @event, CrabModification modification)
        {
            return new AddressSubaddressStatusWasImportedFromCrab(
                new CrabSubaddressStatusId(@event.SubaddressStatusId),
                new CrabSubaddressId(@event.SubaddressId),
                @event.AddressStatus,
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                modification,
                @event.Organisation
            );


        }

        public static AddressSubaddressPositionWasImportedFromCrab WithWkbGeometry(this AddressSubaddressPositionWasImportedFromCrab @event, WkbGeometry wkbGeometry)
        {
            return new AddressSubaddressPositionWasImportedFromCrab(
                new CrabAddressPositionId(@event.AddressPositionId),
                new CrabSubaddressId(@event.SubaddressId),
                wkbGeometry,
                @event.AddressPositionOrigin,
                new CrabAddressNature(@event.AddressNature),
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressSubaddressPositionWasImportedFromCrab WithCrabAddressPositionOrigin(this AddressSubaddressPositionWasImportedFromCrab @event, CrabAddressPositionOrigin addressPositionOrigin)
        {
            return new AddressSubaddressPositionWasImportedFromCrab(
                new CrabAddressPositionId(@event.AddressPositionId),
                new CrabSubaddressId(@event.SubaddressId),
                new WkbGeometry(@event.AddressPosition),
                addressPositionOrigin,
                new CrabAddressNature(@event.AddressNature),
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressSubaddressPositionWasImportedFromCrab WithBeginDate(this AddressSubaddressPositionWasImportedFromCrab @event, LocalDateTime? beginDate)
        {
            return new AddressSubaddressPositionWasImportedFromCrab(
                new CrabAddressPositionId(@event.AddressPositionId),
                new CrabSubaddressId(@event.SubaddressId),
                new WkbGeometry(@event.AddressPosition),
                @event.AddressPositionOrigin,
                new CrabAddressNature(@event.AddressNature),
                new CrabLifetime(beginDate, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressSubaddressPositionWasImportedFromCrab WithEndDate(this AddressSubaddressPositionWasImportedFromCrab @event, LocalDateTime? endDate)
        {
            return new AddressSubaddressPositionWasImportedFromCrab(
                new CrabAddressPositionId(@event.AddressPositionId),
                new CrabSubaddressId(@event.SubaddressId),
                new WkbGeometry(@event.AddressPosition),
                @event.AddressPositionOrigin,
                new CrabAddressNature(@event.AddressNature),
                new CrabLifetime(@event.BeginDateTime, endDate),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                @event.Modification,
                @event.Organisation);
        }

        public static AddressSubaddressPositionWasImportedFromCrab WithModification(this AddressSubaddressPositionWasImportedFromCrab @event, CrabModification modification)
        {
            return new AddressSubaddressPositionWasImportedFromCrab(
                new CrabAddressPositionId(@event.AddressPositionId),
                new CrabSubaddressId(@event.SubaddressId),
                new WkbGeometry(@event.AddressPosition),
                @event.AddressPositionOrigin,
                new CrabAddressNature(@event.AddressNature),
                new CrabLifetime(@event.BeginDateTime, @event.EndDateTime),
                new CrabTimestamp(@event.Timestamp),
                new CrabOperator(@event.Operator),
                modification,
                @event.Organisation);
        }

        public static AddressWasPositioned WithAddressGeometry(this AddressWasPositioned @event, AddressGeometry addressGeometry)
        {
            var addressWasPositioned = new AddressWasPositioned(new AddressId(@event.AddressId), addressGeometry);
            (addressWasPositioned as ISetProvenance).SetProvenance(@event.Provenance.ToProvenance());
            return addressWasPositioned;
        }

        public static AddressStreetNameWasChanged WithStreetNameId(this AddressStreetNameWasChanged @event, StreetNameId streetNameId)
        {
            var result = new AddressStreetNameWasChanged(new AddressId(@event.AddressId), streetNameId);
            (result as ISetProvenance).SetProvenance(@event.Provenance.ToProvenance());
            return result;

        }

        public static AddressStreetNameWasCorrected WithStreetNameId(this AddressStreetNameWasCorrected @event, StreetNameId streetNameId)
        {
            var result = new AddressStreetNameWasCorrected(new AddressId(@event.AddressId), streetNameId);
            (result as ISetProvenance).SetProvenance(@event.Provenance.ToProvenance());
            return result;

        }

        public static AddressHouseNumberWasCorrected WithHouseNumber(this AddressHouseNumberWasCorrected @event, HouseNumber houseNumber)
        {
            var result = new AddressHouseNumberWasCorrected(new AddressId(@event.AddressId), houseNumber);
            (result as ISetProvenance).SetProvenance(@event.Provenance.ToProvenance());
            return result;

        }
    }
}
