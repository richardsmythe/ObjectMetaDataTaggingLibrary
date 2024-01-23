using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface ITagFactory
    {
        // Factory used to allow for creation of other types of tags if necessary
        BaseTag CreateBaseTag(string name, object value, string description);
        // Create multiple BaseTags at once
        IEnumerable<BaseTag> CreateBaseTags(IEnumerable<(string name, object value, string description)> tagList);

    }
}
