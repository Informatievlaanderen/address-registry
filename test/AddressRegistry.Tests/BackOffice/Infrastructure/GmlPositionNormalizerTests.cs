namespace AddressRegistry.Tests.BackOffice.Infrastructure
{
    using System;
    using AddressRegistry.Api.BackOffice.Abstractions;
    using AddressRegistry.Api.BackOffice.Infrastructure;
    using FluentAssertions;
    using Xunit;

    public class GmlPositionNormalizerTests
    {
        private const string HttpSrsNameGmlPointGeometry =
            "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/31370\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:pos>103671.37 192046.71</gml:pos></gml:Point>";

        private const string HttpSrsNameGmlPointGeometryLambert2008 =
            "<gml:Point srsName=\"http://www.opengis.net/def/crs/EPSG/0/3812\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
            "<gml:pos>603668.87 692041.51</gml:pos></gml:Point>";

        private static GmlPositionNormalizer WithLambert2008EventStore(bool enabled)
            => new GmlPositionNormalizer(new UseLambert2008EventStoreToggle(enabled));

        /// <summary>
        /// A position already in the event store's reference system is passed through verbatim,
        /// srsName scheme and coordinate precision included.
        /// </summary>
        [Theory]
        [InlineData(GeometryHelpers.GmlPointGeometry)]
        [InlineData(HttpSrsNameGmlPointGeometry)]
        public void GivenLambert72EventStore_WhenLambert72Position_ThenPositionIsUnchanged(string position)
        {
            WithLambert2008EventStore(false)
                .ToEventStoreSrs(position)
                .Should().Be(position);
        }

        [Theory]
        [InlineData(GeometryHelpers.GmlPointGeometryLambert2008)]
        [InlineData(HttpSrsNameGmlPointGeometryLambert2008)]
        public void GivenLambert2008EventStore_WhenLambert2008Position_ThenPositionIsUnchanged(string position)
        {
            WithLambert2008EventStore(true)
                .ToEventStoreSrs(position)
                .Should().Be(position);
        }

        /// <summary>
        /// A converted position is re-serialized to a single canonical form with an http srsName,
        /// regardless of the scheme it was sent with.
        /// </summary>
        [Theory]
        [InlineData(GeometryHelpers.GmlPointGeometryLambert2008)]
        [InlineData(HttpSrsNameGmlPointGeometryLambert2008)]
        public void GivenLambert72EventStore_WhenLambert2008Position_ThenPositionIsConvertedToLambert72(string position)
        {
            WithLambert2008EventStore(false)
                .ToEventStoreSrs(position)
                .Should().Be(GeometryHelpers.NormalizedGmlPointGeometry);
        }

        [Theory]
        [InlineData(GeometryHelpers.GmlPointGeometry)]
        [InlineData(HttpSrsNameGmlPointGeometry)]
        public void GivenLambert2008EventStore_WhenLambert72Position_ThenPositionIsConvertedToLambert2008(string position)
        {
            WithLambert2008EventStore(true)
                .ToEventStoreSrs(position)
                .Should().Be(GeometryHelpers.NormalizedGmlPointGeometryLambert2008);
        }

        [Theory]
        [InlineData("<gml:Point srsName=\"https://www.opengis.net/def/crs/EPSG/0/4326\" xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                    "<gml:pos>4.35 50.85</gml:pos></gml:Point>")]
        [InlineData("<gml:Point xmlns:gml=\"http://www.opengis.net/gml/3.2\">" +
                    "<gml:pos>103671.37 192046.71</gml:pos></gml:Point>")]
        public void WhenSrsNameIsUnsupported_ThenThrows(string gml)
        {
            var act = () => WithLambert2008EventStore(false).ToEventStoreSrs(gml);

            act.Should().Throw<InvalidOperationException>();
        }
    }
}
