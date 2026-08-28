namespace AddressRegistry.Migrator.Lambert2008.Infrastructure
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading;

    /// <summary>
    /// Collects per-stream durations so the run can report percentiles rather than an average, which on a
    /// long tail of very large street names says very little.
    /// </summary>
    internal sealed class DurationStatistics
    {
        private readonly Lock _lock = new();
        private readonly List<double> _milliseconds = [];

        public void Add(TimeSpan duration)
        {
            lock (_lock)
            {
                _milliseconds.Add(duration.TotalMilliseconds);
            }
        }

        public int Count
        {
            get
            {
                lock (_lock)
                {
                    return _milliseconds.Count;
                }
            }
        }

        /// <summary>Renders p50/p90/p99/max in milliseconds, or "no samples" when nothing was measured.</summary>
        public string Describe()
        {
            double[] sorted;
            lock (_lock)
            {
                sorted = [.. _milliseconds.Order()];
            }

            if (sorted.Length == 0)
            {
                return "no samples";
            }

            return string.Create(
                CultureInfo.InvariantCulture,
                $"p50={Percentile(sorted, 0.50):F0}ms p90={Percentile(sorted, 0.90):F0}ms p99={Percentile(sorted, 0.99):F0}ms max={sorted[^1]:F0}ms");
        }

        private static double Percentile(double[] sorted, double percentile)
        {
            var index = (int)Math.Ceiling(percentile * sorted.Length) - 1;
            return sorted[Math.Clamp(index, 0, sorted.Length - 1)];
        }
    }
}
