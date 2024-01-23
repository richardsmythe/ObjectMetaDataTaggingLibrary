using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Helpers
{
    public class TagFactory : ITagFactory
    {
        public BaseTag CreateBaseTag(string name, object value, string description)
        {
            return new BaseTag(name, value, description);
        }
        public IEnumerable<BaseTag> CreateBaseTags(IEnumerable<(string name, object value, string description)> tagList)
        {
            foreach (var tag in tagList)
            {
                yield return CreateBaseTag(tag.name, tag.value, tag.description);
            }
        }

    }
}
