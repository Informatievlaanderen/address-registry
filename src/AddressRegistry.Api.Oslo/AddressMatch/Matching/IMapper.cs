namespace AddressRegistry.Api.Oslo.AddressMatch.Matching
{
    public interface IMapper<in TSource, out TDestination>
    {
        TDestination Map(TSource source);
    }
}
