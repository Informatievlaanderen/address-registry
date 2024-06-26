namespace AddressRegistry.Api.Oslo.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Be.Vlaanderen.Basisregisters.Api.Syndication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.SyndicationFeed;
    using Microsoft.SyndicationFeed.Atom;

    public static class AtomFeedConfigurationBuilder
    {
        public static AtomFeedConfiguration CreateFrom(IConfigurationSection configuration, DateTimeOffset lastUpdated)
        {
            return new AtomFeedConfiguration(
                configuration["Id"],
                configuration["Title"],
                configuration["Subtitle"],
                configuration["GeneratorTitle"],
                configuration["GeneratorUri"],
                Assembly.GetEntryAssembly().GetName().Version.ToString(),
                configuration["Rights"],
                lastUpdated,
                new SyndicationPerson(configuration["AuthorName"], configuration["AuthorEmail"], AtomContributorTypes.Author),
                new SyndicationLink(new Uri(configuration["Self"]), AtomLinkTypes.Self),
                new List<SyndicationLink>(),
                configuration
                    .GetSection("Related")
                    .GetChildren()
                    .Select(c =>
                        new SyndicationLink(new Uri(c.Value), AtomLinkTypes.Related))
                    .ToList()
            );
        }
    }
}
