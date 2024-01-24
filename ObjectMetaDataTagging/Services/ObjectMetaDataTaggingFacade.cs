using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Utilities;

namespace ObjectMetaDataTagging
{
    public class ObjectMetaDataTaggingFacade<T> where T : BaseTag
    {
        private readonly IDefaultTaggingService<T> _taggingService;

        public ObjectMetaDataTaggingFacade(IDefaultTaggingService<T> taggingService)
        {
            _taggingService = taggingService ?? throw new ArgumentNullException(nameof(taggingService));
        }

        public virtual async Task SetTagAsync(object o, T tag) => await _taggingService.SetTagAsync(o, tag);

        public virtual async Task<bool> UpdateTagAsync(object o, Guid tagId, T newTag) => await _taggingService.UpdateTagAsync(o, tagId, newTag);

        public virtual async Task<IEnumerable<T>> GetAllTags(object o) => await _taggingService.GetAllTags(o);

        public virtual async Task<T>? GetTag(object o, Guid tagId) => await _taggingService.GetTag(o, tagId);

        public virtual async Task<bool> RemoveAllTagsAsync(object o) => await _taggingService.RemoveAllTagsAsync(o);

        public virtual async Task<bool> RemoveTagAsync(object? o, Guid tagId) => await _taggingService.RemoveTagAsync(o, tagId);

        public bool HasTag(object o, Guid tagId) => _taggingService.HasTag(o, tagId);

        public virtual  object? GetObjectByTag(Guid tagId) => _taggingService.GetObjectByTag(tagId);

        public async Task<List<GraphNode>> GetObjectGraph() => await _taggingService.GetObjectGraph();

        public virtual async Task BulkAddTagsAsync(object o, IEnumerable<T> tags) => await _taggingService.BulkAddTagsAsync(o, tags);

        public event EventHandler<AsyncTagAddedEventArgs> TagAdded
        {
            add
            {
                if (_taggingService != null)
                    _taggingService.TagAdded += value;
            }
            remove
            {
                if (_taggingService != null)
                    _taggingService.TagAdded -= value;
            }
        }

        public event EventHandler<AsyncTagRemovedEventArgs> TagRemoved
        {
            add
            {
                if (_taggingService != null)
                    _taggingService.TagRemoved += value;
            }
            remove
            {
                if (_taggingService != null)
                    _taggingService.TagRemoved -= value;
            }
        }

        public event EventHandler<AsyncTagUpdatedEventArgs> TagUpdated
        {
            add
            {
                if (_taggingService != null)
                    _taggingService.TagUpdated += value;
            }
            remove
            {
                if (_taggingService != null)
                    _taggingService.TagUpdated -= value;
            }
        }
    }
}
