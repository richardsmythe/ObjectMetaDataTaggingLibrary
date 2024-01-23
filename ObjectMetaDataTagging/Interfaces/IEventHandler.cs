using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IAsyncEventHandler<T>
    {
        Task<BaseTag> HandleAsync(T eventArgs);
    }
}
