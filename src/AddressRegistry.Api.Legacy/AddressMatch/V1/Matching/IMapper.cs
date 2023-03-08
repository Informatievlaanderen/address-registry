namespace AddressRegistry.Api.Legacy.AddressMatch.V1.Matching
{
    public interface IMapper<in TSource, out TDestination>
    {
        TDestination Map(TSource source);
    }
}
