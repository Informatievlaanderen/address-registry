namespace AddressRegistry.Projections.Extract
{
    using GeoAPI.Geometries;

    public class EnvelopePartialRecord
    {
        public double MinimumX { get; set; }
        public double MaximumX { get; set; }
        public double MinimumY { get; set; }
        public double MaximumY { get; set; }

        public static EnvelopePartialRecord From(Envelope envelope) => new EnvelopePartialRecord
        {
            MinimumX = envelope.MinX,
            MinimumY = envelope.MinY,
            MaximumX = envelope.MaxX,
            MaximumY = envelope.MaxY
        };

        public override bool Equals(object obj)
        {
            return obj is EnvelopePartialRecord envelope && Equals(envelope);
        }

        protected bool Equals(EnvelopePartialRecord other)
        {
            return MinimumX.Equals(other.MinimumX)
                   && MinimumY.Equals(other.MinimumY)
                   && MaximumX.Equals(other.MaximumX)
                   && MaximumY.Equals(other.MaximumY);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = MinimumX.GetHashCode();
                hashCode = (hashCode * 397) ^ MinimumY.GetHashCode();
                hashCode = (hashCode * 397) ^ MaximumX.GetHashCode();
                hashCode = (hashCode * 397) ^ MaximumY.GetHashCode();
                return hashCode;
            }
        }
    }
}
