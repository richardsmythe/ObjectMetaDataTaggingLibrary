﻿
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.QueryModels;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;
using ObjectMetaDataTagging.Utilities;

namespace ObjectMetaDataTagging
{
    public class TaggingManager<T> : ITaggingManager<T> where T : BaseTag
    {

        private readonly IDefaultTaggingService<T> _taggingService;
        private readonly ITagFactory _tagFactory;
        private readonly ITagMapper<T, T> _tagMapper;
        private readonly IDynamicQueryBuilder<T> _tagQueryBuilder;
        

        public TaggingManager(
            IDefaultTaggingService<T> taggingService, 
            ITagFactory tagFactory,
          ITagMapper<T, T> tagMapper,
            IDynamicQueryBuilder<T> tagQueryBuilder

           )
        {
            _taggingService = taggingService ?? throw new ArgumentNullException(nameof(taggingService));
            _tagFactory = tagFactory ?? throw new ArgumentNullException(nameof(tagFactory));
            _tagMapper = tagMapper ?? throw new ArgumentNullException(nameof(tagMapper));
            _tagQueryBuilder = tagQueryBuilder ?? throw new ArgumentNullException(nameof(tagQueryBuilder));            
        }


        public event EventHandler<AsyncTagAddedEventArgs<T>> TagAdded
        {
            add => _taggingService.TagAdded += value;
            remove => _taggingService.TagAdded -= value;
        }

        public event EventHandler<AsyncTagRemovedEventArgs<T>> TagRemoved
        {
            add => _taggingService.TagRemoved += value;
            remove => _taggingService.TagRemoved -= value;
        }

        public event EventHandler<AsyncTagUpdatedEventArgs<T>> TagUpdated
        {
            add => _taggingService.TagUpdated += value;
            remove => _taggingService.TagUpdated -= value;
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

        public BaseTag CreateBaseTag(string name, object value, string description) => _tagFactory.CreateBaseTag(name, value, description);
        public IEnumerable<BaseTag> CreateBaseTags(IEnumerable<(string name, object value, string description)> tagList) => _tagFactory.CreateBaseTags(tagList);

        public async Task<TTarget> MapTagsBetweenTypes<TSource, TTarget>(TSource sourceObject, TTarget targetObject)
        {
            return await _tagMapper.MapTagsBetweenTypes(sourceObject, targetObject);
        }


        public async Task<IEnumerable<T>> BuildQuery(List<T> source, Func<T, bool> propertyFilter, LogicalOperator logicalOperator = LogicalOperator.OR)
        {
            IEnumerable<T> result = await Task.Run(() =>
                _tagQueryBuilder
                    .WithPropertyFilter(propertyFilter)
                    .SetLogicalOperator(logicalOperator)
                    .BuildDynamicQuery(source)
                    .ToList());

            return result;
        }

    }
}

