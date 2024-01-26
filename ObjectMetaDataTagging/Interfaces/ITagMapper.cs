namespace ObjectMetaDataTagging.Interfaces
{
    public interface ITagMapper<T>
    { 
        Task<T> MapTagsFromOtherType(object sourceObject);
    }
}
