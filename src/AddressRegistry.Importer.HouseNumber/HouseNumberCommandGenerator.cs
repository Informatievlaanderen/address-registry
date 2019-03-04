namespace AddressRegistry.Importer.HouseNumber
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Linq;
    using System.Threading.Tasks;
    using Address.Commands.Crab;
    using Aiv.Vbr.CentraalBeheer.Crab.CrabHist;
    using Aiv.Vbr.CentraalBeheer.Crab.Entity;
    using Be.Vlaanderen.Basisregisters.Crab;
    using Be.Vlaanderen.Basisregisters.GrAr.Import.Processing.Generate;
    using Crab;
    using Crab.HouseNumber;
    using Dapper;
    using NodaTime;

    internal class HouseNumberCommandGenerator : ICommandGenerator<int>
    {
        private readonly Lazy<ILookup<int, AssignOsloIdForCrabHouseNumberId>> _osloIdCommands;

        public string Name => GetType().Name;

        public HouseNumberCommandGenerator(string vbrConnectionString)
        {
            _osloIdCommands =
                new Lazy<ILookup<int, AssignOsloIdForCrabHouseNumberId>>(() => GetOsloCommandsToPost(vbrConnectionString).ToLookup(x => (int) x.HouseNumberId, x => x));
        }

        public IEnumerable<int> GetChangedKeys(DateTime from,
            DateTime until) => CrabQueries.GetChangedHuisnummerIdsBetween(from, until).Distinct();

        public IEnumerable<dynamic> GenerateInitCommandsFor(
            int key,
            DateTime from,
            DateTime until)
        {
            var crabCommands = CreateCommandsInOrder(key, from, until);

            MapLogging.Log("*");
            crabCommands.Add(_osloIdCommands.Value[key].Single());

            return crabCommands;
        }

        public IEnumerable<dynamic> GenerateUpdateCommandsFor(
            int key,
            DateTime from,
            DateTime until)
        {
            var crabCommands = CreateCommandsInOrder(key, from, until);

            crabCommands.Add(new RequestOsloIdForCrabHouseNumberId(new CrabHouseNumberId(key)));

            return crabCommands;
        }

        protected List<dynamic> CreateCommandsInOrder(
            int huisnummerId,
            DateTime from,
            DateTime until)
        {
            var importHouseNumberCommands = new List<ImportHouseNumberFromCrab>();
            List<ImportHouseNumberFromCrab> importHouseNumberHistCommands;
            List<ImportHouseNumberStatusFromCrab> importHouseNumberStatusCommands;
            List<ImportHouseNumberStatusFromCrab> importHouseNumberStatusHistCommands;
            List<ImportHouseNumberPositionFromCrab> importHouseNumberPositionCommands;
            List<ImportHouseNumberPositionFromCrab> importHouseNumberPositionHistCommands;
            List<ImportHouseNumberMailCantonFromCrab> importHouseNumberMailCantonCommands;
            List<ImportHouseNumberMailCantonFromCrab> importHouseNumberMailCantonHistCommands;

            //using (var context = new CRABEntities())
            {
                var tblHuisNummerByHuisnummerIdTask = Task.Run(() => RunOnContext(context => AdresHuisnummerQueries.GetTblHuisNummerByHuisnummerId(huisnummerId, context)));
                var importHouseNumberHistCommandsTask = Task.Run(() =>
                    RunOnContext(context => CrabHouseNumberMapper.Map(AdresHuisnummerQueries.GetTblHuisNummerHistByHuisnummerId(huisnummerId, context)).ToList()));
                var importHouseNumberStatusCommandsTask = Task.Run(() =>
                    RunOnContext(context => CrabHouseNumberStatusMapper.Map(AdresHuisnummerQueries.GetTblHuisnummerstatussenByHuisnummerId(huisnummerId, context)).ToList()));
                var importHouseNumberStatusHistCommandsTask = Task.Run(() => RunOnContext(context =>
                    CrabHouseNumberStatusMapper.Map(AdresHuisnummerQueries.GetTblHuisnummerstatussenHistByHuisnummerId(huisnummerId, context)).ToList()));
                var importHouseNumberPositionCommandsTask = Task.Run(() =>
                    RunOnContext(context => CrabHouseNumberPositionMapper.Map(AdresHuisnummerQueries.GetTblAdresPositiesByHuisnummerId(huisnummerId, context)).ToList()));
                var importHouseNumberPositionHistCommandsTask = Task.Run(() =>
                    RunOnContext(context => CrabHouseNumberPositionMapper.Map(AdresHuisnummerQueries.GetTblAdresPositiesHistByHuisnummerId(huisnummerId, context)).ToList()));
                var importHouseNumberMailCantonCommandsTask = Task.Run(() => RunOnContext(context =>
                    CrabHouseNumberMailCantonMapper.Map(AdresHuisnummerQueries.GetTblHuisnummerPostkantonsByHuisnummerId(huisnummerId, context)).ToList()));
                var importHouseNumberMailCantonHistCommandsTask = Task.Run(() => RunOnContext(context =>
                    CrabHouseNumberMailCantonMapper.Map(AdresHuisnummerQueries.GetTblHuisnummerPostKantonsHistByHuisnummerId(huisnummerId, context)).ToList()));

                Task.WaitAll(tblHuisNummerByHuisnummerIdTask, importHouseNumberHistCommandsTask, importHouseNumberStatusCommandsTask,
                    importHouseNumberStatusHistCommandsTask, importHouseNumberPositionCommandsTask, importHouseNumberPositionHistCommandsTask,
                    importHouseNumberMailCantonCommandsTask, importHouseNumberMailCantonHistCommandsTask);

                var tblHuisNummerByHuisnummerId = tblHuisNummerByHuisnummerIdTask.Result;
                if (tblHuisNummerByHuisnummerId != null)
                    importHouseNumberCommands = new List<ImportHouseNumberFromCrab> {CrabHouseNumberMapper.Map(tblHuisNummerByHuisnummerId)};

                importHouseNumberHistCommands = importHouseNumberHistCommandsTask.Result;
                importHouseNumberStatusCommands = importHouseNumberStatusCommandsTask.Result;
                importHouseNumberStatusHistCommands = importHouseNumberStatusHistCommandsTask.Result;
                importHouseNumberPositionCommands = importHouseNumberPositionCommandsTask.Result;
                importHouseNumberPositionHistCommands = importHouseNumberPositionHistCommandsTask.Result;
                importHouseNumberMailCantonCommands = importHouseNumberMailCantonCommandsTask.Result;
                importHouseNumberMailCantonHistCommands = importHouseNumberMailCantonHistCommandsTask.Result;
            }

            var commandsByHouseNumber = importHouseNumberCommands
                .Concat(importHouseNumberHistCommands)
                .OrderBy(x => (Instant) x.Timestamp);

            var commands = new List<dynamic> {commandsByHouseNumber.First()};

            var allHouseNumberCommands = importHouseNumberHistCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 0, 0, $"Huisnummer {x.HouseNumberId}"))
                .Concat(importHouseNumberCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 0, 1, $"Huisnummer {x.HouseNumberId}")))
                .Concat(importHouseNumberStatusHistCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 1, 0, $"Huisnummer {x.HouseNumberId}")))
                .Concat(importHouseNumberStatusCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 1, 1, $"Huisnummer {x.HouseNumberId}")))
                .Concat(importHouseNumberPositionHistCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 1, 4, $"Huisnummer {x.HouseNumberId}")))
                .Concat(importHouseNumberPositionCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 1, 5, $"Huisnummer {x.HouseNumberId}")))
                .Concat(importHouseNumberMailCantonHistCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 1, 2, $"Huisnummer {x.HouseNumberId}")))
                .Concat(importHouseNumberMailCantonCommands.Select(x =>
                    Tuple.Create<dynamic, int, int, string>(x, 1, 3, $"Huisnummer {x.HouseNumberId}")))
                .ToList();

            var adresCommands = allHouseNumberCommands
                .Where(x => x.Item1.Timestamp > from.ToCrabInstant() && x.Item1.Timestamp <= until.ToCrabInstant())
                .OrderBy(x => x.Item1.Timestamp)
                .ThenBy(x => x.Item2)
                .ThenBy(x => x.Item3);

            foreach (var adresCommand in adresCommands)
                commands.Add(adresCommand.Item1);

            return commands;
        }

        private T RunOnContext<T>(Func<CRABEntities, T> query)
        {
            using (var context = new CRABEntities())
            {
                return query(context);
            }
        }

        private IEnumerable<AssignOsloIdForCrabHouseNumberId> GetOsloCommandsToPost(string connectionString)
        {
            using (var connection = new SqlConnection(connectionString))
            {
                //Verbose("Fetching house number mappings in vbr");
                return connection.Query<Crab2VbrHouseNumberMapping>(
                        "SELECT o.ObjectID, m.AdresIDInternal, m.CrabHuisnummerID, m.MappingCreatedTimestamp " +
                        "FROM crab.AdresMappingToHuisnummer m " +
                        "INNER JOIN crab.AdresObjectID o ON m.AdresIDInternal = o.AdresIDInternal " +
                        "ORDER BY m.CrabHuisnummerID")
                    .Select(mapping => new AssignOsloIdForCrabHouseNumberId(
                        new CrabHouseNumberId(mapping.CrabHuisnummerId),
                        new OsloId(Convert.ToInt32(mapping.ObjectId)),
                        new OsloAssignmentDate(Instant.FromDateTimeOffset(mapping.MappingCreatedTimestamp))));
            }
        }

        private class Crab2VbrHouseNumberMapping
        {
            public string ObjectId { get; set; }
            public int AdresIdInternal { get; set; }
            public int CrabHuisnummerId { get; set; }
            public DateTimeOffset MappingCreatedTimestamp { get; set; }
        }
    }
}
