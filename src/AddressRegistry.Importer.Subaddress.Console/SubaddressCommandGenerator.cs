namespace AddressRegistry.Importer.Subaddress.Console
{
    using System;
    using System.Collections.Generic;
    using Microsoft.Data.SqlClient;
    using System.Linq;
    using Address.Commands.Crab;
    using Aiv.Vbr.CentraalBeheer.Crab.CrabHist;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Generate;
    using Dapper;
    using HouseNumber.Console.Crab;
    using HouseNumber.Console.Crab.Subaddress;
    using NodaTime;

    public class SubaddressCommandGenerator : ICommandGenerator<int>
    {
        private readonly Func<CRABEntities> _crabEntitiesFactory;
        private readonly Lazy<ILookup<int, AssignPersistentLocalIdForCrabSubaddressId>> _persistentLocalIdCommands;

        public string Name => GetType().Name;

        public SubaddressCommandGenerator(string vbrConnectionString, Func<CRABEntities> crabEntitiesFactory)
        {
            _crabEntitiesFactory = crabEntitiesFactory;
            _persistentLocalIdCommands = new Lazy<ILookup<int, AssignPersistentLocalIdForCrabSubaddressId>>(() =>
                GetCommandsToPost(vbrConnectionString).ToLookup(x => (int)x.SubaddressId, x => x));
        }

        public IEnumerable<int> GetChangedKeys(
            DateTime from,
            DateTime until) => CrabQueries.GetChangedSubadresIdsBetween(from, until, _crabEntitiesFactory).Distinct();

        public IEnumerable<dynamic> GenerateInitCommandsFor(
            int key,
            DateTime from,
            DateTime until)
        {
            var crabCommands = CreateCommandsInOrder(ImportMode.Init, key, from, until);

            MapLogging.Log("*");
            crabCommands.Add(_persistentLocalIdCommands.Value[key].Single());

            return crabCommands;
        }

        public IEnumerable<dynamic> GenerateUpdateCommandsFor(
            int key,
            DateTime from,
            DateTime until)
        {
            var crabCommands = CreateCommandsInOrder(ImportMode.Update, key, from, until);

            crabCommands.Add(new RequestPersistentLocalIdForCrabSubaddressId(new CrabSubaddressId(key)));

            return crabCommands;
        }

        protected List<dynamic> CreateCommandsInOrder(
            ImportMode importMode,
            int id,
            DateTime from,
            DateTime until)
        {
            var importSubaddressCommands = new List<ImportSubaddressFromCrab>();
            List<ImportSubaddressFromCrab> importSubaddressHistCommands;
            List<ImportSubaddressStatusFromCrab> importSubaddressStatusCommands;
            List<ImportSubaddressStatusFromCrab> importSubaddressStatusHistCommands;
            List<ImportSubaddressPositionFromCrab> importSubaddressPositionCommands;
            List<ImportSubaddressPositionFromCrab> importSubaddressPositionHistCommands;
            var importHouseNumberSubaddressCommands = new List<ImportHouseNumberSubaddressFromCrab>();
            List<ImportHouseNumberSubaddressFromCrab> importHouseNumberSubaddressCommandsHist;
            var importSubaddressMailCantonCommands = new List<ImportSubaddressMailCantonFromCrab>();
            List<ImportSubaddressMailCantonFromCrab> importSubaddressMailCantonCommandsHist;

            using (var context = _crabEntitiesFactory())
            {
                var importSubaddressFromCrab = AdresSubadresQueries.GetTblSubadresBySubadresId(id, context);
                if (importSubaddressFromCrab != null)
                    importSubaddressCommands = new List<ImportSubaddressFromCrab> { CrabSubaddressMapper.Map(importSubaddressFromCrab) };

                importSubaddressHistCommands = CrabSubaddressMapper.Map(AdresSubadresQueries.GetTblSubadresHistBySubadresId(id, context)).ToList();

                importSubaddressStatusCommands = CrabSubaddressStatusMapper.Map(AdresSubadresQueries.GetTblSubadresstatussesBySubadresId(id, context)).ToList();
                importSubaddressStatusHistCommands = CrabSubaddressStatusMapper.Map(AdresSubadresQueries.GetTblSubadresstatussesHistBySubadresId(id, context)).ToList();

                importSubaddressPositionCommands = CrabSubaddressPositionMapper.Map(AdresSubadresQueries.GetTblAdrespositiesBySubadresId(id, context)).ToList();
                importSubaddressPositionHistCommands = CrabSubaddressPositionMapper.Map(AdresSubadresQueries.GetTblAdrespositiesHistBySubadresId(id, context)).ToList();

                var allHouseNumbers = importSubaddressHistCommands.Select(x => (int)x.HouseNumberId).ToList();
                if (importSubaddressFromCrab != null)
                    allHouseNumbers.Add((int)importSubaddressCommands.First().HouseNumberId);

                allHouseNumbers = allHouseNumbers.Distinct().ToList();

                foreach (var houseNumber in allHouseNumbers)
                {
                    var tblHuisNummerByHuisnummerId = AdresHuisnummerQueries.GetTblHuisNummerByHuisnummerId(houseNumber, context);
                    if (tblHuisNummerByHuisnummerId != null)
                    {
                        var importHouseNumberSubadresFromCrab = CrabSubaddressHouseNumberMapper.Map(tblHuisNummerByHuisnummerId, id);
                        if (importHouseNumberSubadresFromCrab != null)
                            importHouseNumberSubaddressCommands.Add(importHouseNumberSubadresFromCrab);
                    }

                    importSubaddressMailCantonCommands.AddRange(
                        CrabSubaddressMailCantonMapper.GetCommandsFor(AdresHuisnummerQueries.GetTblHuisnummerPostkantonsByHuisnummerId(houseNumber, context), id));
                }

                importHouseNumberSubaddressCommandsHist =
                    CrabSubaddressHouseNumberMapper.Map(AdresHuisnummerQueries.GetTblHuisNummerHistByHuisnummerIds(allHouseNumbers, context), id).ToList();

                importSubaddressMailCantonCommandsHist =
                    CrabSubaddressMailCantonMapper.GetCommandsFor(AdresHuisnummerQueries.GetTblHuisnummerPostKantonHistsByHuisnummerIds(allHouseNumbers, context),
                        id).ToList();
            }

            var allSubaddressCommands = importSubaddressHistCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 2, 0, $"Subadres {x.SubaddressId}"))
                .Concat(importSubaddressCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 2, 1, $"Subadres {x.SubaddressId}")))
                .Concat(importSubaddressStatusHistCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 3, 0, $"Subadres {x.SubaddressId}")))
                .Concat(importSubaddressStatusCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 3, 1, $"Subadres {x.SubaddressId}")))
                .Concat(importSubaddressPositionHistCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 3, 2, $"Subadres {x.SubaddressId}")))
                .Concat(importSubaddressPositionCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 3, 3, $"Subadres {x.SubaddressId}")))
                .Concat(importHouseNumberSubaddressCommandsHist.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 0, 0, $"Subadres {x.SubaddressId}")))
                .Concat(importHouseNumberSubaddressCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 0, 1, $"Subadres {x.SubaddressId}")))
                .Concat(importSubaddressMailCantonCommandsHist.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 1, 0, $"Subadres {x.SubaddressId}")))
                .Concat(importSubaddressMailCantonCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 1, 1, $"Subadres {x.SubaddressId}")))
                .ToList();

            var addressCommands = allSubaddressCommands
                .Where(x => x.Item1.Timestamp > from.ToCrabInstant() && x.Item1.Timestamp <= until.ToCrabInstant());

            if (importMode == ImportMode.Update) //if an update changes the subaddress' house number, make sure the commands for that house number are also sent
            {
                var houseNumbersForUpdate = importSubaddressCommands
                    .Concat(importSubaddressHistCommands)
                    .Where(x => x.Timestamp > from.ToCrabInstant() && x.Timestamp <= until.ToCrabInstant())
                    .Select(x => x.HouseNumberId).ToList();

                if (houseNumbersForUpdate.Any())
                {
                    var houseNumbersBeforeUpdate = importSubaddressCommands
                        .Concat(importSubaddressHistCommands)
                        .Where(x => x.Timestamp <= from.ToCrabInstant())
                        .Select(x => x.HouseNumberId).ToList();

                    var newHouseNumbers = houseNumbersForUpdate.Except(houseNumbersBeforeUpdate);

                    foreach (var newHouseNumber in newHouseNumbers)
                    {
                        addressCommands = addressCommands.Concat(allSubaddressCommands.Except(addressCommands).Where(x =>
                            //(x.Item1 is ImportSubaddressFromCrab importSubaddressFromCrab && importSubaddressFromCrab.HouseNumberId == newHouseNumber) ||
                            (x.Item1 is ImportSubaddressMailCantonFromCrab importSubaddressMailCantonFromCrab && importSubaddressMailCantonFromCrab.HouseNumberId == newHouseNumber) ||
                            (x.Item1 is ImportHouseNumberSubaddressFromCrab importHouseNumberSubaddressFromCrab && importHouseNumberSubaddressFromCrab.HouseNumberId == newHouseNumber)));
                    }
                }
            }

            addressCommands = addressCommands.OrderBy(x => x.Item1.Timestamp)
                .ThenBy(x => x.Item2)
                .ThenBy(x => x.Item3);

            var commands = new List<dynamic>();

            ImportHouseNumberSubaddressFromCrab initialImportHouseNumberSubaddressFromCrab = null;
            if (importMode == ImportMode.Init)
            {
                initialImportHouseNumberSubaddressFromCrab = importHouseNumberSubaddressCommands
                    .Concat(importHouseNumberSubaddressCommandsHist)
                    .OrderBy(x => (Instant)x.Timestamp)
                    .First(x => x.HouseNumberId == addressCommands.First<ImportSubaddressFromCrab>().HouseNumberId);

                commands.Add(initialImportHouseNumberSubaddressFromCrab);
            }

            foreach (var adresCommand in addressCommands)
                if (importMode == ImportMode.Update || !adresCommand.Item1.Equals(initialImportHouseNumberSubaddressFromCrab))
                    commands.Add(adresCommand.Item1);

            return commands;
        }

        protected IEnumerable<AssignPersistentLocalIdForCrabSubaddressId> GetCommandsToPost(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                //Verbose("Fetching subaddress mappings in vbr");
                return connection.Query<Crab2VbrSubaddressMapping>(
                        "SELECT o.ObjectID, m.AdresIDInternal, m.CrabSubadresID, m.MappingCreatedTimestamp " +
                        "FROM crab.AdresMappingToSubadres m " +
                        "INNER JOIN crab.AdresObjectID o ON m.AdresIDInternal = o.AdresIDInternal " +
                        "ORDER BY m.CrabSubadresID")
                    .Select(crab2VbrMapping => new AssignPersistentLocalIdForCrabSubaddressId(
                        new CrabSubaddressId(crab2VbrMapping.CrabSubadresId),
                        new PersistentLocalId(Convert.ToInt32(crab2VbrMapping.ObjectId)),
                        new PersistentLocalIdAssignmentDate(Instant.FromDateTimeOffset(crab2VbrMapping.MappingCreatedTimestamp))));
            }
        }

        private class Crab2VbrSubaddressMapping
        {
            public string ObjectId { get; set; }
            public int AdresIdInternal { get; set; }
            public int CrabSubadresId { get; set; }
            public DateTimeOffset MappingCreatedTimestamp { get; set; }
        }
    }

    internal static class CommandGeneratorHelperExtensions
    {
        public static TCommand First<TCommand>(this IEnumerable<Tuple<dynamic, int, int, string>> commands)
            where TCommand : BaseCrabCommand
            => (TCommand)commands.First(item => item.Item1 is TCommand).Item1;
    }
}
