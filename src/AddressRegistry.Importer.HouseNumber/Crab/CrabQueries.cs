namespace AddressRegistry.Importer.HouseNumber.Crab
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Aiv.Vbr.CrabModel;

    public static class CrabQueries
    {
        public static List<int> GetChangedHuisnummerIdsBetween(
            DateTime since,
            DateTime until,
            int? lastProcessedId = -1)
        {
            if (since == DateTime.MinValue)
                using (var crabEntities = CreateCrabEntities())
                {
                    return crabEntities
                        .tblHuisNummer
                        .GroupBy(s => s.huisNummerId)
                        .Select(s => new { s.Key, beginTijd = s.Min(nummer => nummer.beginTijd) })
                        .Concat(crabEntities
                            .tblHuisNummer_hist
                            .GroupBy(s => s.huisNummerId)
                            .Select(s => new { Key = s.Key.Value, beginTijd = s.Min(nummer => nummer.beginTijd.Value) }))
                        .GroupBy(h => h.Key)
                        .Select(s => new { s.Key, beginTijd = s.Min(nummer => nummer.beginTijd) })
                        .Where(s => s.beginTijd <= until && s.Key > lastProcessedId)
                        .OrderBy(s => s.beginTijd)
                        .Select(s => s.Key)
                        .ToList();
                }

            var tasks = new List<Task<List<int>>>
            {
                Task.Run(() =>
                {
                    var huisnummerIds = new List<int>();
                    using (var crabEntities = CreateCrabEntities())
                    {
                        huisnummerIds.AddRange(crabEntities
                            .tblHuisNummer
                            .Where(x => x.beginTijd > since && x.beginTijd <= until)
                            .Select(x => x.huisNummerId));

                        huisnummerIds.AddRange(crabEntities
                            .tblHuisNummer_hist
                            .Where(x => x.beginTijd > since && x.beginTijd <= until)
                            .Select(x => x.huisNummerId.Value));

                        huisnummerIds.AddRange(crabEntities
                            .tblHuisNummer_hist
                            .Where(x => x.eindTijd > since && x.eindTijd <= until &&
                                        x.eindBewerking == CrabBewerking.Verwijdering.Code)
                            .Select(x => x.huisNummerId.Value));
                    }

                    return huisnummerIds;
                }),

                Task.Run(() =>
                {
                    var huisnummerIds = new List<int>();
                    using (var crabEntities = CreateCrabEntities())
                    {
                        huisnummerIds.AddRange(crabEntities
                            .tblHuisnummerstatus
                            .Where(x => x.begintijd > since && x.begintijd <= until)
                            .Select(x => x.huisnummerid));

                        huisnummerIds.AddRange(crabEntities
                            .tblHuisnummerstatus_hist
                            .Where(x => x.begintijd > since && x.begintijd <= until)
                            .Select(x => x.huisnummerid.Value));

                        huisnummerIds.AddRange(crabEntities
                            .tblHuisnummerstatus_hist
                            .Where(x => x.eindTijd > since && x.eindTijd <= until &&
                                        x.eindBewerking == CrabBewerking.Verwijdering.Code)
                            .Select(x => x.huisnummerid.Value));
                    }

                    return huisnummerIds;
                }),
                Task.Run(() =>
                {
                    var huisnummerIds = new List<int>();
                    using (var crabEntities = CreateCrabEntities())
                    {
                        huisnummerIds.AddRange(crabEntities
                            .tblHuisNummer_postKanton
                            .Where(x => x.beginTijd > since && x.beginTijd <= until)
                            .Select(x => x.huisNummerId));

                        huisnummerIds.AddRange(crabEntities
                            .tblHuisNummer_postKanton_hist
                            .Where(x => x.beginTijd > since && x.beginTijd <= until)
                            .Select(x => x.huisNummerId.Value));

                        huisnummerIds.AddRange(crabEntities
                            .tblHuisNummer_postKanton_hist
                            .Where(x => x.eindTijd > since && x.eindTijd <= until &&
                                        x.eindBewerking == CrabBewerking.Verwijdering.Code)
                            .Select(x => x.huisNummerId.Value));
                    }

                    return huisnummerIds;
                }),
                Task.Run(() =>
                {
                    var huisnummerIds = new List<int>();
                    using (var crabEntities = CreateCrabEntities())
                    {
                        const string aardHuisnummer = "2";
                        huisnummerIds.AddRange(crabEntities.tblAdrespositie
                            .Where(x => x.aardAdres == aardHuisnummer && x.begintijd > since && x.begintijd <= until)
                            .Select(x => x.adresid));

                        huisnummerIds.AddRange(crabEntities
                            .tblAdrespositie_hist
                            .Where(x => x.aardAdres == aardHuisnummer && x.begintijd > since && x.begintijd <= until)
                            .Select(x => x.adresid.Value));

                        huisnummerIds.AddRange(crabEntities
                            .tblAdrespositie_hist.Where(x =>
                            x.aardAdres == aardHuisnummer && x.eindTijd > since && x.eindTijd <= until &&
                            x.eindBewerking == CrabBewerking.Verwijdering.Code)
                            .Select(x => x.adresid.Value));
                    }

                    return huisnummerIds;
                })
            };

            Task.WaitAll(tasks.ToArray());

            return tasks.SelectMany(s => s.Result).Distinct().ToList();
        }

        public static List<int> GetChangedSubadresIdsBetween(
            DateTime since,
            DateTime until,
            int? lastProcessedId = -1)
        {
            if (since == DateTime.MinValue)
                using (var crabEntities = CreateCrabEntities())
                {
                    return crabEntities
                        .tblSubAdres
                        .GroupBy(s => s.subAdresId)
                        .Select(s => new { s.Key, beginTijd = s.Min(adres => adres.beginTijd) })
                        .Concat(crabEntities
                            .tblSubAdres_hist
                            .GroupBy(s => s.subAdresId)
                            .Select(s => new { Key = s.Key.Value, beginTijd = s.Min(adres => adres.beginTijd.Value) }))
                        .GroupBy(h => h.Key)
                        .Select(s => new { s.Key, beginTijd = s.Min(nummer => nummer.beginTijd) })
                        .Where(s => s.beginTijd <= until && s.Key > lastProcessedId)
                        .OrderBy(s => s.beginTijd)
                        .Select(s => s.Key)
                        .ToList();
                }

            var tasks = new List<Task<List<int>>>
            {
                Task.Run(() =>
                {
                    var subadresIds = new List<int>();
                    using (var crabEntities = CreateCrabEntities())
                    {
                        subadresIds.AddRange(crabEntities
                            .tblSubAdres
                            .Where(x => x.beginTijd > since && x.beginTijd <= until)
                            .Select(x => x.subAdresId));

                        subadresIds.AddRange(crabEntities
                            .tblSubAdres_hist
                            .Where(x => x.beginTijd > since && x.beginTijd <= until)
                            .Select(x => x.subAdresId.Value));

                        subadresIds.AddRange(crabEntities
                            .tblSubAdres_hist
                            .Where(x => x.eindTijd > since && x.eindTijd <= until &&
                                        x.eindBewerking == CrabBewerking.Verwijdering.Code)
                            .Select(x => x.subAdresId.Value));

                        //Huisnummer influences subadres (3B => 3 = huisnummer, B = subadres)
                        var huisnummerIds = new List<int>();
                        huisnummerIds.AddRange(crabEntities
                            .tblHuisNummer
                            .Where(x => x.beginTijd > since && x.beginTijd <= until)
                            .Select(x => x.huisNummerId));

                        huisnummerIds.AddRange(crabEntities
                            .tblHuisNummer_hist
                            .Where(x => x.beginTijd > since && x.beginTijd <= until)
                            .Select(x => x.huisNummerId.Value));

                        huisnummerIds.AddRange(crabEntities
                            .tblHuisNummer_hist
                            .Where(x => x.eindTijd > since && x.eindTijd <= until &&
                                        x.eindBewerking == CrabBewerking.Verwijdering.Code)
                            .Select(x => x.huisNummerId.Value));

                        huisnummerIds = huisnummerIds.Distinct().ToList();

                        subadresIds.AddRange(IterateSqlContains(huisnummerIds, (idsInRange,
                            filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblSubAdres.Where(sa => idsInRange.Contains(sa.huisNummerId))
                                    .Select(sa => sa.subAdresId)
                                    .Concat(crabEntities.tblSubAdres_hist
                                        .Where(sa => idsInRange.Contains(sa.huisNummerId.Value))
                                        .Select(sa => sa.subAdresId.Value))
                                    .ToList());
                        }));

                        subadresIds.AddRange(IterateSqlContains(huisnummerIds, (idsInRange,
                            filteredIds) =>
                        {
                            filteredIds.AddRange(
                                crabEntities.tblSubAdres.Where(sa => idsInRange.Contains(sa.huisNummerId))
                                    .Select(sa => sa.subAdresId)
                                    .Concat(crabEntities.tblSubAdres_hist
                                        .Where(sa => idsInRange.Contains(sa.huisNummerId.Value))
                                        .Select(sa => sa.subAdresId.Value))
                                    .ToList());
                        }));
                    }

                    return subadresIds;
                }),
                Task.Run(() =>
                {
                    var subadresIds = new List<int>();
                    using (var crabEntities = CreateCrabEntities())
                    {
                        subadresIds.AddRange(crabEntities
                            .tblSubadresstatus
                            .Where(x => x.begintijd > since && x.begintijd <= until)
                            .Select(x => x.subadresid));

                        subadresIds.AddRange(crabEntities
                            .tblSubadresstatus_hist
                            .Where(x => x.begintijd > since && x.begintijd <= until)
                            .Select(x => x.subadresid.Value));

                        subadresIds.AddRange(crabEntities
                            .tblSubadresstatus_hist
                            .Where(x => x.eindTijd > since && x.eindTijd <= until &&
                                        x.eindBewerking == CrabBewerking.Verwijdering.Code)
                            .Select(x => x.subadresid.Value));
                    }

                    return subadresIds;
                }),
                Task.Run(() =>
                {
                    var subadresIds = new List<int>();
                    using (var crabEntities = CreateCrabEntities())
                    {
                        const string aardSubadres = "1";
                        subadresIds.AddRange(crabEntities
                            .tblAdrespositie
                            .Where(x => x.aardAdres == aardSubadres && x.begintijd > since && x.begintijd <= until)
                            .Select(x => x.adresid));

                        subadresIds.AddRange(crabEntities
                            .tblAdrespositie_hist
                            .Where(x => x.aardAdres == aardSubadres && x.begintijd > since && x.begintijd <= until)
                            .Select(x => x.adresid.Value));

                        subadresIds.AddRange(crabEntities
                            .tblAdrespositie_hist.Where(x =>
                            x.aardAdres == aardSubadres && x.eindTijd > since && x.eindTijd <= until &&
                            x.eindBewerking == CrabBewerking.Verwijdering.Code)
                            .Select(x => x.adresid.Value));
                    }

                    return subadresIds;
                }),
                Task.Run(() =>
                {
                    var subadresIds = new List<int>();
                    using (var crabEntities = CreateCrabEntities())
                    {
                        //Postkanton
                        subadresIds.AddRange(crabEntities
                            .tblHuisNummer_postKanton
                            .Where(x => x.beginTijd > since && x.beginTijd <= until)
                            .SelectMany(x => x.tblHuisNummer.tblSubAdres.Select(s => s.subAdresId)));

                        var huisnummerIdsPostkanton = new List<int>();
                        huisnummerIdsPostkanton.AddRange(crabEntities
                            .tblHuisNummer_postKanton_hist
                            .Where(x => x.beginTijd > since && x.beginTijd <= until)
                            .Select(x => x.huisNummerId.Value));

                        huisnummerIdsPostkanton.AddRange(crabEntities
                            .tblHuisNummer_postKanton_hist
                            .Where(x => x.eindTijd > since && x.eindTijd <= until &&
                                        x.eindBewerking == CrabBewerking.Verwijdering.Code)
                            .Select(x => x.huisNummerId.Value));

                        subadresIds.AddRange(crabEntities
                            .tblSubAdres
                            .Where(sa => huisnummerIdsPostkanton.Contains(sa.huisNummerId))
                            .Select(sa => sa.subAdresId));

                        subadresIds.AddRange(crabEntities
                            .tblSubAdres_hist
                            .Where(sa => huisnummerIdsPostkanton.Contains(sa.huisNummerId.Value))
                            .Select(sa => sa.subAdresId.Value));
                    }

                    return subadresIds;
                })
            };

            Task.WaitAll(tasks.ToArray());

            return tasks
                .SelectMany(s => s.Result)
                .Distinct()
                .ToList();
        }

        private static CRABEntities CreateCrabEntities()
        {
            var entities = new CRABEntities();
            entities.Database.CommandTimeout = 60 * 60;
            return entities;
        }

        private static List<int> IterateSqlContains(
            IReadOnlyCollection<int> allIds,
            Action<List<int>, List<int>> addRangeAction)
        {
            var filteredIds = new List<int>();
            const int sqlContainsSize = 1000;
            for (var i = 0; i < Math.Ceiling(allIds.Count / (double)sqlContainsSize); i++)
            {
                var idsInThisRange = allIds
                    .Skip(i * sqlContainsSize)
                    .Take(Math.Min(sqlContainsSize, allIds.Count - i * sqlContainsSize))
                    .ToList();

                addRangeAction(idsInThisRange, filteredIds);
            }

            return filteredIds;
        }
    }
}
