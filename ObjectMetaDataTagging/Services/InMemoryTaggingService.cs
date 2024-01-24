using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Exceptions;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Utilities;
using System.Collections.Concurrent;

namespace ObjectMetaDataTagging.Services
{

    public class InMemoryTaggingService<T> : IDefaultTaggingService<T>
        where T : BaseTag
    {
        /// <summary>
        /// Constructs a DefaultTaggingService with the specified TaggingEventManager for event handling.
        /// </summary>
        /// <param name="eventManager">The TaggingEventManager used for handling tagging-related events.</param>
        public InMemoryTaggingService(
            TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs> eventManager)
        {
            _eventManager = eventManager;
        }

        public InMemoryTaggingService()
        {
            _eventManager = null;
        }

        // Hashtable vs dictionary?
        public readonly ConcurrentDictionary<object, Dictionary<Guid, BaseTag>> data = new ConcurrentDictionary<object, Dictionary<Guid, BaseTag>>();
        public readonly TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs> _eventManager;
        private SemaphoreSlim semaphore = new SemaphoreSlim(1, 1);

        public event EventHandler<AsyncTagAddedEventArgs> TagAdded;
        public event EventHandler<AsyncTagRemovedEventArgs> TagRemoved;
        public event EventHandler<AsyncTagUpdatedEventArgs> TagUpdated;

        public Action<object, T>? OnSetTagAsyncCallback { get; set; }

     

        /// <summary>
        /// Retrieve an object graph for the current objects and their tags.
        /// </summary>
        /// <returns>Returns a list of graph nodes.</returns>
        public async Task<List<GraphNode>> GetObjectGraph()
        {
            return await ObjectGraphBuilder.BuildObjectGraph(data);
        }

        #region Default Tag Operations

        /// <summary>
        /// Gets all tags associated with the specified object.
        /// </summary>
        /// <param name="o">The object for which to retrieve tags.</param>
        /// <returns>A task that represents the asynchronous operation. The task result contains the collection of tags associated with the object.</returns>
        public virtual Task<IEnumerable<T>> GetAllTags(object o)
        {
            if (o == null)
            {
                throw new ObjectNotFoundException("No object supplied.");
            }

            if (data.TryGetValue(o, out var tags))
            {
                var allTags = tags.Values.ToList();

                return Task.FromResult((IEnumerable<T>)allTags);
            }

            throw new ObjectNotFoundException("Object not found in the data collection.", nameof(o));
        }

        /// <summary>
        /// Gets the tag with the specified ID associated with the specified object.
        /// </summary>
        /// <param name="o">The object for which to retrieve the tag.</param>
        /// <param name="tagId">The ID of the tag to retrieve.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result contains the tag associated with the object and ID, or null if the tag is not found.
        /// </returns>
        public virtual Task<T>? GetTag(object o, Guid tagId)
        {
            if (o == null)
            {
                throw new ObjectNotFoundException("No object supplied.");
            }

            if (data.TryGetValue(o, out var tagDictionary))
            {
                if (tagDictionary.TryGetValue(tagId, out var tag))
                {
                    return tag as Task<T>;
                }
            }

            throw new ObjectNotFoundException("Object or tag not found in the data collection.", nameof(o));
        }

        /// <summary>
        /// Removes all tags associated with the specified object asynchronously.
        /// </summary>
        /// <param name="o">The object for which to remove all tags.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result indicates whether the removal of tags was successful.
        /// </returns>
        public virtual async Task<bool> RemoveAllTagsAsync(object o)
        {
            await semaphore.WaitAsync();
            try
            {
                if (o == null)
                {
                    throw new ObjectNotFoundException("No object supplied.");
                }

                if (data.Remove(o, out _))
                {
                    return true;
                }

                return false;

            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Removes a tag with the specified ID associated with the specified object asynchronously.
        /// </summary>
        /// <param name="o">The object for which to remove the tag.</param>
        /// <param name="tagId">The ID of the tag to remove.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result indicates whether the removal of the tag was successful.
        /// </returns>
        public virtual async Task<bool> RemoveTagAsync(object? o, Guid tagId)
        {
            await semaphore.WaitAsync();

            try
            {
                if (o == null)
                {
                    throw new ObjectNotFoundException("No object supplied.");
                }

                if (data.TryGetValue(o, out var tags))
                {
                    if (tags.Remove(tagId))
                    {
                        // if there are no tags left for this object, remove entry
                        if (tags.Count == 0)
                        {
                            data.TryRemove(o, out _);
                        }

                        return true;
                    }

                    await _eventManager.RaiseTagRemoved(new AsyncTagRemovedEventArgs(o, tagId));
                }
                else
                {
                    throw new ObjectNotFoundException("Object not found in the data collection.", nameof(o));
                }
            }
            finally
            {
                semaphore.Release();
            }
            return false;
        }

        /// <summary>
        /// Sets a tag associated with the specified object asynchronously.
        /// </summary>
        /// <param name="o">The object for which to set the tag.</param>
        /// <param name="tag">The tag to set.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        public virtual async Task SetTagAsync(object o, T tag)
        {
            if (tag == null || o == null) throw new ObjectNotFoundException("No object or tag supplied.");

            var objectName = o.GetType().Name;
            var objectId = GetObjectId(o);

            var tagDictionary = data.GetOrAdd(o, new Dictionary<Guid, BaseTag>());

            await semaphore.WaitAsync();
            try
            {
                tagDictionary[tag.Id] = tag;
                tag.Parents.Add(objectId);
            }
            finally
            {
                semaphore.Release();
            }

            // Call the callback if set by something (like a test).
            OnSetTagAsyncCallback?.Invoke(o, tag);

        }

        /// <summary>
        /// Updates a tag associated with the specified object asynchronously.
        /// </summary>
        /// <param name="o">The object for which to update the tag.</param>
        /// <param name="tagId">The ID of the tag to update.</param>
        /// <param name="modifiedTag">The modified tag.</param>
        /// <returns>
        /// A task that represents the asynchronous operation. The task result indicates whether the update of the tag was successful.
        /// </returns>
        public virtual async Task<bool> UpdateTagAsync(object o, Guid tagId, T modifiedTag)
        {
            if (modifiedTag == null || o == null)
            {
                throw new ObjectNotFoundException("No object or tag supplied.");
            }

            modifiedTag.DateLastUpdated = DateTime.UtcNow;

            await semaphore.WaitAsync();

            try
            {
                if (data.TryGetValue(o, out var tags))
                {
                    if (tags.ContainsKey(tagId))
                    {
                        var originalTag = tags[tagId];
                        modifiedTag.Id = originalTag.Id;
                        modifiedTag.DateLastUpdated = DateTime.UtcNow;
                        tags[tagId] = modifiedTag;

                        return true;
                    }
                }

                await _eventManager.RaiseTagUpdated(new AsyncTagUpdatedEventArgs(o, null, modifiedTag));
                return false;
            }
            finally
            {
                semaphore.Release();
            }
        }

        /// <summary>
        /// Checks if the specified object has a tag with the given ID.
        /// </summary>
        /// <param name="o">The object to check for the tag.</param>
        /// <param name="tagId">The ID of the tag to check for.</param>
        /// <returns>True if the object has the specified tag; otherwise, false.</returns>
        public bool HasTag(object o, Guid tagId)
        {
            if (o != null && data.TryGetValue(o, out var tags))
            {
                lock (tags)
                {
                    if (tags.ContainsKey(tagId))
                    {
                        return true;
                    }
                    else
                    {
                        throw new ObjectNotFoundException($"Tag with Id {tagId} not found for the specified object.", nameof(tagId));
                    }
                }
            }
            else
            {
                throw new ObjectNotFoundException("Object not found in the data dictionary.", nameof(o));
            }
        }

        /// <summary>
        /// Retrieves the object associated with the specified tag ID.
        /// </summary>
        /// <param name="tagId">The ID of the tag to find the associated object for.</param>
        /// <returns>The object associated with the specified tag ID.</returns>
        public virtual object GetObjectByTag(Guid tagId)
        {
            foreach (var kvp in data)
            {
                var tags = kvp.Value;

                lock (tags)
                {
                    if (tags.TryGetValue(tagId, out var tag))
                    {
                        return kvp.Key;
                    }
                }
            }

            throw new ObjectNotFoundException($"No object found with tagId: {tagId}", nameof(tagId));
        }

        /// <summary>
        /// Gets the ID of the specified object.
        /// </summary>
        /// <param name="o">The object for which to retrieve the ID.</param>
        /// <returns>The ID of the specified object.</returns>
        private Guid GetObjectId(object o)
        {
            if (o == null)
            {
                throw new ObjectNotFoundException("No object supplied.");
            }

            var idProperty = o.GetType().GetProperty("Id");
            if (idProperty != null)
            {
                var idValue = idProperty.GetValue(o);
                if (idValue is Guid guidValue)
                {
                    return guidValue;
                }
            }

            throw new ObjectNotFoundException("Id property not found or not of type Guid.", nameof(o));
        }

        /// <summary>
        /// Add multiple tags to one object.
        /// </summary>
        /// <param name="o">The object for which to add the tags to.</param>
        public async Task BulkAddTagsAsync(object o, IEnumerable<T> tags)
        {
            if (o == null || tags == null) throw new ObjectNotFoundException("No object or tags supplied.");

            Func<object, T, Task> bulkAddDelegate = async (sourceObj, tag) =>
            {
                await SetTagAsync(sourceObj, tag);
            };
            // so that it won't block the calling thread while adding tags,
            await Task.WhenAll(tags.Select(tag => bulkAddDelegate(o, tag)));
        }

        #endregion


    }
}
