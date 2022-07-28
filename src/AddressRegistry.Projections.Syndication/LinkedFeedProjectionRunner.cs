namespace AddressRegistry.Projections.Syndication
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Runtime.Serialization;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Xml;
    using Autofac.Features.OwnedInstances;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Runner;
    using Be.Vlaanderen.Basisregisters.ProjectionHandling.Syndication;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Logging;
    using Microsoft.SyndicationFeed;
    using Polly;

    public interface ILinkedFeedProjectionRunner<TContext> where TContext : RunnerDbContext<TContext>
    {
        Task CatchUpAsync(Func<Owned<TContext>> contextFactory, CancellationToken cancellationToken = default);
    }

    public class LinkedFeedProjectionRunner<TMessage, TContent, TContext> : ILinkedFeedProjectionRunner<TContext>
        where TMessage : struct
        where TContext : RunnerDbContext<TContext>
    {
        private readonly ILogger _logger;
        private readonly IRegistryAtomFeedReader _atomFeedReader;
        private readonly DataContractSerializer _dataContractSerializer;
        private readonly AtomEntryProjectionHandlerResolver<TMessage, TContext> _atomEntryProjectionHandlerResolver;
        private static Random _jitterer = new Random();
        private readonly int _numberOfRetryAttempts;
        private readonly int _retryJittererMinSeconds;
        private readonly int _retryJittererMaxSeconds;

        // ReSharper disable StaticMemberInGenericType
        public static string RunnerName { get; private set; }
        public static Uri FeedUri { get; private set; }
        public string FeedUserName { get; }
        public string FeedPassword { get; }
        public bool EmbedEvent { get; }
        public bool EmbedObject { get; }

        public LinkedFeedProjectionRunner(
            string runnerName,
            Uri feedUri,
            string feedUserName,
            string feedPassword,
            bool embedEvent,
            bool embedObject,
            int numberOfRetryAttempts,
            int retryJittererMinSeconds,
            int retryJittererMaxSeconds,
            ILogger logger,
            IRegistryAtomFeedReader atomFeedReader,
            params AtomEntryProjectionHandlerModule<TMessage, TContent, TContext>[] projectionHandlerModules)
        {
            RunnerName = runnerName;
            FeedUri = feedUri;
            FeedUserName = feedUserName;
            FeedPassword = feedPassword;
            EmbedEvent = embedEvent;
            EmbedObject = embedObject;

            _numberOfRetryAttempts = numberOfRetryAttempts;
            _retryJittererMinSeconds = retryJittererMinSeconds;
            _retryJittererMaxSeconds = retryJittererMaxSeconds;

            _logger = logger;
            _atomFeedReader = atomFeedReader;

            _dataContractSerializer = new DataContractSerializer(typeof(TContent));
            _atomEntryProjectionHandlerResolver = Resolve.WhenEqualToEvent(projectionHandlerModules.SelectMany(t => t.ProjectionHandlers).ToArray());
        }

        public async Task CatchUpAsync(
            Func<Owned<TContext>> contextFactory,
            CancellationToken cancellationToken = default)
        {
            // Discover last projected position
            long? position;

            await using (var context = contextFactory().Value)
            {
                var dbPosition = await context
                    .ProjectionStates
                    .AsNoTracking()
                    .SingleOrDefaultAsync(p => p.Name == RunnerName, cancellationToken);

                position = dbPosition?.Position + 1;
            }

            var entries = await Policy
                .Handle<XmlException>()
                .WaitAndRetryAsync(_numberOfRetryAttempts,
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)) // exponential back-off: 2, 4, 8 etc
                                    + TimeSpan.FromMilliseconds(_jitterer.Next(_retryJittererMinSeconds, _retryJittererMaxSeconds))) // plus some jitter (cause sync request can be heavy)
                .ExecuteAsync(async () => (await _atomFeedReader.ReadEntriesAsync(FeedUri, position, FeedUserName, FeedPassword, EmbedEvent, EmbedObject)).ToList()); // Read new events

            while (entries.Any())
            {
                if (!long.TryParse(entries.Last().Id, out var lastEntryId))
                {
                    break;
                }

                await using (var context = contextFactory().Value)
                {
                    await ProjectAtomEntriesAsync(entries, context, cancellationToken);

                    await context.UpdateProjectionState(RunnerName, lastEntryId, cancellationToken);
                    await context.SaveChangesAsync(cancellationToken);
                }

                position = lastEntryId + 1;
                entries = (await _atomFeedReader.ReadEntriesAsync(FeedUri, position, FeedUserName, FeedPassword, EmbedEvent, EmbedObject)).ToList();
            }
        }

        private async Task ProjectAtomEntriesAsync(
            IEnumerable<IAtomEntry> entries,
            TContext context,
            CancellationToken cancellationToken = default)
        {
            foreach (var entry in entries)
            {
                _logger.LogInformation($"[{DateTime.Now}] [{entry.Id}] [{entry.Title}]");

                try
                {
                    using var contentXmlReader = XmlReader.Create(new StringReader(entry.Description), new XmlReaderSettings { Async = true });
                    var atomEntry = new AtomEntry(entry, _dataContractSerializer.ReadObject(contentXmlReader));

                    foreach (var resolvedProjectionHandler in _atomEntryProjectionHandlerResolver(atomEntry))
                    {
                        await resolvedProjectionHandler
                            .Handler
                            .Invoke(atomEntry, context, cancellationToken);
                    }
                }
                catch (AtomResolveHandlerException)
                {
                    // do nothing (not all events should be resolved)
                }
                catch (Exception e)
                {
                    _logger.LogError(e.Message, e);
                    throw;
                }
            }
        }
    }
}
