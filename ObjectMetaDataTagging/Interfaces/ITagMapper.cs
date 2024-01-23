namespace ObjectMetaDataTagging.Interfaces
{
    public interface ITagMapper<T>
    { 
        Task<T> MapTagsBetweenTypes(object sourceObject);
    }
}
