namespace ObjectMetaDataTagging.Interfaces
{
    public interface ITagMapper<TSource, TTarget>
    {
        Task<TTarget> MapTagsBetweenTypes<TSource, TTarget>(TSource sourceObject, TTarget targetObject);
    }
}
