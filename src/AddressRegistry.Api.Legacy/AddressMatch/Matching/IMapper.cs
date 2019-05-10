namespace AddressRegistry.Api.Legacy.AddressMatch.Matching
{
    public interface IMapper<in TSource, out TDestination>
    {
        TDestination Map(TSource source);
    }
}
