namespace AddressRegistry.Structurizr
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Be.Vlaanderen.Basisregisters.EventHandling;
    using Microsoft.Extensions.Configuration;
    using global::Structurizr;
    using global::Structurizr.Api;

    public static class Ids
    {
        public static int PersonUser = 10000;
        public static int SoftwareSystemAddressRegistry = 10001;
        public static int ContainerApi = 10002;
        public static int ContainerApiRunner = 10003;
        public static int ContainerApiStore = 10004;
        public static int ContainerEventStore = 10005;
        public static int ContainerAggregateRoot = 10006;

        public static int SoftwareSystemProjectionProducer = 10007;
        public static int SoftwareSystemApi = 10008;
    }

    public static class CustomTags
    {
        public static string Store = "Store";
        public static string Event = "Event";
        public static string Command = "Command";
        public static string Https = "HTTPS";
        public static string EntityFramework = "Entity Framework";
        public static string SqlStreamStore = "SqlStreamStore";
        public static string Direct = "Direct";
    }

    public class Program
    {
        private const string WorkspaceUrlFormat = "https://structurizr.com/workspace/{0}";

        private const string PersonUserName = "Gebruiker";

        // This crap is because structurizr.com expects integers for ids, while structurizr.net wants strings
        private static readonly string PersonUserId = Ids.PersonUser.ToString();
        private static readonly string SoftwareSystemAddressRegistryId = Ids.SoftwareSystemAddressRegistry.ToString();
        private static readonly string ContainerApiId = Ids.ContainerApi.ToString();
        private static readonly string ContainerApiRunnerId = Ids.ContainerApiRunner.ToString();
        private static readonly string ContainerApiStoreId = Ids.ContainerApiStore.ToString();
        private static readonly string ContainerEventStoreId = Ids.ContainerEventStore.ToString();
        private static readonly string ContainerAggregateRootId = Ids.ContainerAggregateRoot.ToString();

        private static readonly string SoftwareSystemProjectionProducerId = Ids.SoftwareSystemProjectionProducer.ToString();
        private static readonly string SoftwareSystemApiId = Ids.SoftwareSystemApi.ToString();

        private static string _workspaceUrlViewFormat;
        private static long _workspaceId;
        private static string _apiKey;
        private static string _apiSecret;

        private static void Main()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: true, reloadOnChange: false)
                .AddJsonFile($"appsettings.{Environment.MachineName.ToLowerInvariant()}.json", optional: true, reloadOnChange: false)
                .Build();

            _workspaceId = long.Parse(configuration["Structurizr:WorkspaceId"]);
            _apiKey = configuration["Structurizr:ApiKey"];
            _apiSecret = configuration["Structurizr:ApiSecret"];
            _workspaceUrlViewFormat = $"{string.Format(WorkspaceUrlFormat, _workspaceId)}#{{0}}";

            var workspace = new Workspace("AddressRegistry", "Adres register.")
            {
                Version = DateTime.Today.ToString("yyyy-MM-dd"),
            };

            var model = workspace.Model;

            var user = CreatePersonUser(model);

            var addressRegistry = CreateSystemAddressRegistry(model);

            var api = CreateContainerApi(addressRegistry);
            var apiRunner = CreateContainerApiRunner(addressRegistry);
            var apiStore = CreateContainerApiStore(addressRegistry);
            var eventStore = CreateContainerEventStore(addressRegistry);
            var aggregateRoot = CreateContainerAggregateRoot(addressRegistry);

            user.Uses(addressRegistry, "raadpleegt", "HTTPS").AddTags(CustomTags.Https);
            user.Uses(api, "raadpleegt", "HTTPS").AddTags(CustomTags.Https);

            api.Uses(apiStore, "leest gegevens", "Entity Framework").AddTags(CustomTags.EntityFramework);
            api.Uses(eventStore, "produceert events", "SqlStreamStore").AddTags(CustomTags.SqlStreamStore);

            apiRunner.Uses(eventStore, "leest events", "SqlStreamStore").AddTags(CustomTags.SqlStreamStore);
            apiRunner.Uses(apiStore, "projecteert gegevens", "Entity Framework").AddTags(CustomTags.EntityFramework);

            foreach (var e in FindAllEvents())
            {
                var @event = eventStore.AddComponent(e.Name, e.Type, FormatDescription(e.Description, e.Properties), "Event");
                @event.AddTags(CustomTags.Event);
                api.Uses(@event, "produceert").AddTags(CustomTags.Event);
            }

            foreach (var c in FindAllCommands())
            {
                var command = aggregateRoot.AddComponent(c.Name, c.Type, FormatDescription(c.Description, c.Properties), "Command");
                command.AddTags(CustomTags.Command);
                api.Uses(command, "aanvaardt").AddTags(CustomTags.Command);
            }

            CreateApiRunnerFake(model);
            CreateApiFake(model);

            var views = workspace.Views;

            CreateContextView(views, model);
            CreateApiContainerView(views, model);
            CreateApiRunnerContainerView(views, model);
            CreateEventsComponentView(views, model);
            CreateCommandsComponentView(views, model);

            ConfigureStyles(views);

            UploadWorkspaceToStructurizr(workspace);
        }

        private static string FormatDescription(string description, IEnumerable<string> properties)
            //=> $"{description}{Environment.NewLine}{string.Join(", ", properties)}";
            => $"{description}";

        private static void ConfigureStyles(ViewSet views)
        {
            var styles = views.Configuration.Styles;
            styles.Add(new ElementStyle(Tags.Person) { Background = "#08427b", Color = "#ffffff", Shape = Shape.Person });
            styles.Add(new ElementStyle(Tags.SoftwareSystem) { Background = "#1168bd", Color = "#ffffff" });
            styles.Add(new ElementStyle(Tags.Container) { Background = "#438dd5", Color = "#ffffff" });
            styles.Add(new ElementStyle(Tags.Component) { Background = "#85BBF0", Color = "#444444" });
            styles.Add(new ElementStyle(CustomTags.Store) { Background = "#438DD5", Color = "#ffffff", Shape = Shape.Cylinder });

            styles.Add(new ElementStyle(CustomTags.Event)
            {
                Background = "#85BBF0",
                Color = "#444444",
                Shape = Shape.RoundedBox,
                Width = 690
            });

            styles.Add(new ElementStyle(CustomTags.Command)
            {
                Background = "#85BBF0",
                Color = "#444444",
                Shape = Shape.RoundedBox,
                Width = 690
            });

            styles.Add(new RelationshipStyle(Tags.Asynchronous) { Dashed = true });
            styles.Add(new RelationshipStyle(Tags.Synchronous) { Dashed = false });

            styles.Add(new RelationshipStyle(Tags.Relationship) { Routing = Routing.Orthogonal });
            styles.Add(new RelationshipStyle(CustomTags.Direct) { Routing = Routing.Direct });

            styles.Add(new RelationshipStyle(CustomTags.Https) { Color = "#5a9b44" });
            styles.Add(new RelationshipStyle(CustomTags.EntityFramework) { Color = "#9b4473" });
            styles.Add(new RelationshipStyle(CustomTags.SqlStreamStore) { Color = "#448d9b" });
        }

        private static Container CreateContainerEventStore(SoftwareSystem addressRegistry)
        {
            var eventStore = addressRegistry
                .AddContainer(
                    "Eventstore",
                    "Authentieke bron van gegevens, opgeslagen als een stroom van events.",
                    "SQL Server");

            eventStore.Id = ContainerEventStoreId;
            eventStore.AddTags(CustomTags.Store);

            return eventStore;
        }

        private static Container CreateContainerAggregateRoot(SoftwareSystem addressRegistry)
        {
            var aggregateRoot = addressRegistry
                .AddContainer(
                    "AggregateRoot",
                    "Authentieke objecten.",
                    ".NET Core/C#");

            aggregateRoot.Id = ContainerAggregateRootId;
            aggregateRoot.AddTags(CustomTags.Store);

            return aggregateRoot;
        }

        private static Container CreateContainerApiStore(SoftwareSystem addressRegistry)
        {
            var apiStore = addressRegistry
                .AddContainer(
                    "Loket API gegevens",
                    "Gegevens geoptimaliseerd voor de loket API.",
                    "SQL Server");

            apiStore.Id = ContainerApiStoreId;
            apiStore.AddTags(CustomTags.Store);

            return apiStore;
        }

        private static Container CreateContainerApiRunner(SoftwareSystem addressRegistry)
        {
            var apiRunner = addressRegistry
                .AddContainer(
                    "Loket API projecties",
                    "Asynchrone runner die events verwerkt ten behoeve van de loket API.",
                    "Event Sourcing");

            apiRunner.Id = ContainerApiRunnerId;

            return apiRunner;
        }

        private static Container CreateContainerApi(SoftwareSystem addressRegistry)
        {
            var api = addressRegistry
                .AddContainer(
                    "Loket API",
                    "Publiek beschikbare API, bedoeld ter integratie in het loket.",
                    "REST/HTTPS");

            api.Id = ContainerApiId;

            return api;
        }

        private static SoftwareSystem CreateSystemAddressRegistry(Model model)
        {
            var addressRegistry = model
                .AddSoftwareSystem(
                    Location.Internal,
                    "AddressRegistry",
                    "Het adres register laat gebruikers toe alle authentieke gegevens van een adres te raadplegen.");

            addressRegistry.Id = SoftwareSystemAddressRegistryId;

            return addressRegistry;
        }

        private static Person CreatePersonUser(Model model)
        {
            var user = model
                .AddPerson(
                    PersonUserName,
                    "Een gebruiker van het adres register.");

            user.Id = PersonUserId;

            return user;
        }

        private static void CreateApiRunnerFake(Model model)
        {
            var addressRegistry = model.GetSoftwareSystemWithId(SoftwareSystemAddressRegistryId);

            var apiStore = addressRegistry.GetContainerWithId(ContainerApiStoreId);
            var eventStore = addressRegistry.GetContainerWithId(ContainerEventStoreId);

            var projectionProducer = model
                .AddSoftwareSystem(
                    Location.Internal,
                    "Loket API projecties runner",
                    "Asynchrone runner die events verwerkt ten behoeve van de loket API.");

            projectionProducer.Id = SoftwareSystemProjectionProducerId;
            //projectionProducer.Url = string.Format(_workspaceUrlViewFormat, "Loket%20API%20overzicht");

            apiStore
                .Uses(projectionProducer, "runner projecteert gegevens", "Entity Framework", InteractionStyle.Asynchronous)
                .AddTags(CustomTags.EntityFramework, CustomTags.Direct);

            eventStore
                .Uses(projectionProducer, "runner leest events", "SqlStreamStore", InteractionStyle.Asynchronous)
                .AddTags(CustomTags.SqlStreamStore, CustomTags.Direct);
        }

        private static void CreateApiFake(Model model)
        {
            var addressRegistry = model.GetSoftwareSystemWithId(SoftwareSystemAddressRegistryId);

            var apiStore = addressRegistry.GetContainerWithId(ContainerApiStoreId);
            var eventStore = addressRegistry.GetContainerWithId(ContainerEventStoreId);

            var api = model
                .AddSoftwareSystem(
                    Location.Internal,
                    "Loket API",
                    "Publiek beschikbare API, bedoeld ter integratie in het loket.");

            api.Id = SoftwareSystemApiId;
            //projectionProducer.Url = string.Format(_workspaceUrlViewFormat, "Loket%20API%20overzicht");

            api
                .Uses(apiStore, "loket api leest gegevens", "Entity Framework")
                .AddTags(CustomTags.EntityFramework, CustomTags.Direct);

            api
                .Uses(eventStore, "loket api produceert events", "SqlStreamStore")
                .AddTags(CustomTags.SqlStreamStore, CustomTags.Direct);
        }

        private static IEnumerable<EventInfo> FindAllEvents()
        {
            var events = typeof(DomainAssemblyMarker)
                .GetTypeInfo()
                .Assembly
                .GetExportedTypes()
                .Where(x => x.AssemblyQualifiedName.Contains("AddressRegistry.Address.Events"))
                .ToList();

            return events.Select(x => new EventInfo
            {
                Name = x.GetCustomAttribute<EventNameAttribute>().Value,
                Description = x.GetCustomAttribute<EventDescriptionAttribute>().Value,
                //Description = x.FullName.Replace("AddressRegistry.Address.Events.", string.Empty),
                Type = x,
                Properties = x.GetProperties().Select(y => y.Name).ToList()
            });
        }

        private static IEnumerable<EventInfo> FindAllCommands()
        {
            var events = typeof(DomainAssemblyMarker)
                .GetTypeInfo()
                .Assembly
                .GetExportedTypes()
                .Where(x => x.AssemblyQualifiedName.Contains("AddressRegistry.Address.Commands"))
                .ToList();

            return events.Select(x => new EventInfo
            {
                Name = x.Name,
                //Description = x.GetCustomAttribute<EventDescriptionAttribute>().Value,
                Description = x.FullName.Replace("AddressRegistry.Address.Commands.", string.Empty),
                Type = x,
                Properties = x.GetProperties().Select(y => y.Name).ToList()
            });
        }

        private static void CreateContextView(ViewSet views, Model model)
        {
            var addressRegistry = model.GetSoftwareSystemWithId(SoftwareSystemAddressRegistryId);
            var user = model.GetPersonWithName(PersonUserName);

            var contextView = views
                .CreateSystemContextView(
                    addressRegistry,
                    "Globaal overzicht",
                    "Globaal overzicht van het adres register.");

            contextView.Add(addressRegistry);
            contextView.Add(user);

            contextView.PaperSize = PaperSize.A6_Portrait;
        }

        private static void CreateApiContainerView(ViewSet views, Model model)
        {
            var addressRegistry = model.GetSoftwareSystemWithId(SoftwareSystemAddressRegistryId);
            var projectionProducer = model.GetSoftwareSystemWithId(SoftwareSystemProjectionProducerId);

            var user = model.GetPersonWithName(PersonUserName);
            var api = addressRegistry.GetContainerWithId(ContainerApiId);
            var apiStore = addressRegistry.GetContainerWithId(ContainerApiStoreId);
            var eventStore = addressRegistry.GetContainerWithId(ContainerEventStoreId);

            var containerView = views.CreateContainerView(
                addressRegistry,
                "Loket API overzicht",
                "Detail overzicht hoe de loket API aan gegevens komt.");

            containerView.Add(user);
            containerView.Add(api);
            containerView.Add(apiStore);
            containerView.Add(eventStore);

            containerView.Add(projectionProducer);

            containerView.PaperSize = PaperSize.A5_Portrait;
        }

        private static void CreateApiRunnerContainerView(ViewSet views, Model model)
        {
            var addressRegistry = model.GetSoftwareSystemWithId(SoftwareSystemAddressRegistryId);
            var api = model.GetSoftwareSystemWithId(SoftwareSystemApiId);

            var apiRunner = addressRegistry.GetContainerWithId(ContainerApiRunnerId);
            var apiStore = addressRegistry.GetContainerWithId(ContainerApiStoreId);
            var eventStore = addressRegistry.GetContainerWithId(ContainerEventStoreId);

            var containerView = views.CreateContainerView(
                addressRegistry,
                "Loket API projecties runner",
                "Detail overzicht hoe gegevens voor de loket API worden gemaakt.");

            containerView.Add(apiRunner);
            containerView.Add(apiStore);
            containerView.Add(eventStore);

            containerView.Add(api);

            containerView.PaperSize = PaperSize.A6_Portrait;

            var start = 100;
            var blockWidth = 450;
            var blockHeight = 300;
            var xPortion = (containerView.PaperSize.height - (start * 3) - (3 * blockHeight)) / (3 - 1);

            var middle = containerView.PaperSize.width / 2 - (blockWidth / 2);
            var left = containerView.PaperSize.width / 4 - (blockWidth / 2);
            var right = (containerView.PaperSize.width / 4 * 3) - (blockWidth / 2);

            SetPosition(containerView, ContainerApiRunnerId, middle, start);
            SetPosition(containerView, ContainerApiStoreId, left, start + blockHeight + xPortion);
            SetPosition(containerView, ContainerEventStoreId, right, start + blockHeight + xPortion);
            SetPosition(containerView, SoftwareSystemApiId, middle, start + blockHeight + (blockHeight + xPortion * 2));
        }

        private static void CreateEventsComponentView(ViewSet views, Model model)
        {
            var addressRegistry = model.GetSoftwareSystemWithId(SoftwareSystemAddressRegistryId);
            var eventStore = addressRegistry.GetContainerWithId(ContainerEventStoreId);

            var events = eventStore
                .Components
                .Where(x => x.Type.Contains("AddressRegistry.Address.Events"))
                .ToList();

            var eventsView = views.CreateComponentView(
                eventStore,
                "Events",
                "Overzicht van alle events.");

            eventsView.Add(eventStore);

            foreach (var @event in events)
                eventsView.Add(@event);

            // eventsView.Relationships.RemoveWhere(x => x.Relationship.Tags.Contains(CustomTags.Event));

            eventsView.PaperSize = PaperSize.A4_Landscape;

            var eventsPerRow = 4;
            var startLeft = 150;
            var left = startLeft;
            var top = 300;
            var blockWidth = 690;
            var blockHeight = 300;
            var yPortion = (eventsView.PaperSize.width - (left * 2) - (eventsPerRow * blockWidth)) / (eventsPerRow - 1);

            for (var eventNumber = 0; eventNumber < events.Count; eventNumber++)
            {
                var @event = events[eventNumber];
                SetPosition(eventsView, @event.Id, left, top);
                left += blockWidth + yPortion;

                if ((eventNumber + 1) % eventsPerRow != 0)
                    continue;

                left = startLeft;
                top += blockHeight + 100;
            }
        }

        private static void CreateCommandsComponentView(ViewSet views, Model model)
        {
            var addressRegistry = model.GetSoftwareSystemWithId(SoftwareSystemAddressRegistryId);
            var aggregateRoot = addressRegistry.GetContainerWithId(ContainerAggregateRootId);

            var commands = aggregateRoot
                .Components
                .Where(x => x.Type.Contains("AddressRegistry.Address.Commands"))
                .ToList();

            var commandsView = views.CreateComponentView(
                aggregateRoot,
                "Commands",
                "Overzicht van alle commands.");

            commandsView.Add(aggregateRoot);

            foreach (var command in commands)
                commandsView.Add(command);

            // commandsView.Relationships.RemoveWhere(x => x.Relationship.Tags.Contains(CustomTags.Event));

            commandsView.PaperSize = PaperSize.A4_Landscape;

            var commandsPerRow = 4;
            var startLeft = 150;
            var left = startLeft;
            var top = 300;
            var blockWidth = 690;
            var blockHeight = 300;
            var yPortion = (commandsView.PaperSize.width - (left * 2) - (commandsPerRow * blockWidth)) / (commandsPerRow - 1);

            for (var commandNumber = 0; commandNumber < commands.Count; commandNumber++)
            {
                var @event = commands[commandNumber];
                SetPosition(commandsView, @event.Id, left, top);
                left += blockWidth + yPortion;

                if ((commandNumber + 1) % commandsPerRow != 0)
                    continue;

                left = startLeft;
                top += blockHeight + 100;
            }
        }

        private static void SetPosition(View view, string id, int x, int y)
        {
            var element = view.Elements.Single(e => e.Id == id);
            element.X = x;
            element.Y = y;
        }

        private static void UploadWorkspaceToStructurizr(Workspace workspace)
        {
            var structurizrClient = new StructurizrClient(_apiKey, _apiSecret) { MergeFromRemote = false };
            structurizrClient.PutWorkspace(_workspaceId, workspace);
            Console.WriteLine($"Workspace can be viewed at {string.Format(WorkspaceUrlFormat, _workspaceId)}");
        }
    }
}
