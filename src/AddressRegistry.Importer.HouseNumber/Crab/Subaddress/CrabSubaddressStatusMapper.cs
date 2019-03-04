namespace AddressRegistry.Importer.HouseNumber.Crab.Subaddress
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Address.Commands.Crab;
    using AddressRegistry.Crab;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Aiv.Vbr.CrabModel;
    using Be.Vlaanderen.Basisregisters.Crab;
    using CrabLifetime = Be.Vlaanderen.Basisregisters.Crab.CrabLifetime;

    public static class CrabSubaddressStatusMapper
    {
        public static IEnumerable<ImportSubaddressStatusFromCrab> Map(IEnumerable<tblSubadresstatus_hist> subadresStatussesHist)
        {
            return subadresStatussesHist
                .Select(
                    subadresstatusHist =>
                    {
                        MapLogging.Log(".");

                        return new ImportSubaddressStatusFromCrab(
                            new CrabSubaddressStatusId(subadresstatusHist.subadresstatusid.Value),
                            new CrabSubaddressId(subadresstatusHist.subadresid.Value),
                            ParseSubaddressStatus(subadresstatusHist.Status),
                            new CrabLifetime(subadresstatusHist.begindatum?.ToCrabLocalDateTime(), subadresstatusHist.einddatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(subadresstatusHist.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(subadresstatusHist.Operator),
                            CrabEnumMapper.Map(subadresstatusHist.Bewerking),
                            CrabEnumMapper.Map(subadresstatusHist.Organisatie));
                    });
        }

        public static IEnumerable<ImportSubaddressStatusFromCrab> Map(IEnumerable<tblSubadresstatus> subadresStatuses)
        {
            return subadresStatuses
                .Select(
                    subadresstatus =>
                    {
                        MapLogging.Log(".");

                        return new ImportSubaddressStatusFromCrab(
                            new CrabSubaddressStatusId(subadresstatus.subadresstatusid),
                            new CrabSubaddressId(subadresstatus.subadresid),
                            ParseSubaddressStatus(subadresstatus.Status),
                            new CrabLifetime(subadresstatus.begindatum.ToCrabLocalDateTime(), subadresstatus.einddatum?.ToCrabLocalDateTime()),
                            new CrabTimestamp(subadresstatus.CrabTimestamp.ToCrabInstant()),
                            new CrabOperator(subadresstatus.Operator),
                            CrabEnumMapper.Map(subadresstatus.Bewerking),
                            CrabEnumMapper.Map(subadresstatus.Organisatie));
                    });
        }

        private static CrabAddressStatus ParseSubaddressStatus(CrabSubadresStatusEnum subadresStatus)
        {
            if (subadresStatus.Code == CrabSubadresStatusEnum.Gereserveerd.Code)
                return CrabAddressStatus.Reserved;

            if (subadresStatus.Code == CrabSubadresStatusEnum.Voorgesteld.Code)
                return CrabAddressStatus.Proposed;

            if (subadresStatus.Code == CrabSubadresStatusEnum.InGebruik.Code)
                return CrabAddressStatus.InUse;

            if (subadresStatus.Code == CrabSubadresStatusEnum.BuitenGebruik.Code)
                return CrabAddressStatus.OutOfUse;

            if (subadresStatus.Code == CrabSubadresStatusEnum.NietOfficieel.Code)
                return CrabAddressStatus.Unofficial;

            throw new ApplicationException($"Onbekende subadres status {subadresStatus}");
        }
    }
}
