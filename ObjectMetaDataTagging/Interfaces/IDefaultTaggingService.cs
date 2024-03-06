using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;
using ObjectMetaDataTagging.Utilities;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IDefaultTaggingService<T>
        where T : BaseTag
    {
        event EventHandler<AsyncTagAddedEventArgs<T>> TagAdded;
        event EventHandler<AsyncTagRemovedEventArgs<T>> TagRemoved;
        event EventHandler<AsyncTagUpdatedEventArgs<T>> TagUpdated;

        Task SetTagAsync(object o, T tag);
        Task<bool> UpdateTagAsync(object o, Guid tagId, T newTag);
        Task<IEnumerable<T>> GetAllTags(object o);
        Task<T> GetTag(object o, Guid tagId);
        Task<bool> RemoveAllTagsAsync(object o);
        Task<bool> RemoveTagAsync(object? o, Guid tagId);
        bool HasTag(object o, Guid tagId);
        object? GetObjectByTag(Guid tagId);

        Task<List<GraphNode>> GetObjectGraph();
        Task BulkAddTagsAsync(object o, IEnumerable<T> tags);

    }
}
