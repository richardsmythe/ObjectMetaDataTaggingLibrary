using System;
using System.Threading.Tasks;
using ObjectMetaDataTagging.Exceptions;
using ObjectMetaDataTagging.Models.TagModels;

namespace ObjectMetaDataTagging.Events
{
    public class AsyncTagAddedEventArgs<T> : EventArgs
    {
        public object TaggedObject { get; }
        public T Tag { get; }

        public AsyncTagAddedEventArgs(object taggedObject, T tag)
        {
            TaggedObject = taggedObject ?? throw new ObjectNotFoundException(nameof(taggedObject));
            Tag = tag ?? throw new ObjectNotFoundException(nameof(tag));
        }
    }

    public class AsyncTagRemovedEventArgs<T> : EventArgs
    {
        public object TaggedObject { get; }
        public object Tag { get; }

        public AsyncTagRemovedEventArgs(object taggedObject, object tag)
        {
            TaggedObject = taggedObject ?? throw new ObjectNotFoundException(nameof(taggedObject));
            Tag = tag ?? throw new ObjectNotFoundException(nameof(tag));
        }
    }

    public class AsyncTagUpdatedEventArgs<T> : EventArgs
    {
        public object TaggedObject { get; }
        public BaseTag OldTag { get; }
        public BaseTag NewTag { get; }

        public AsyncTagUpdatedEventArgs(object taggedObject, BaseTag oldTag, BaseTag newTag)
        {
            TaggedObject = taggedObject ?? throw new ObjectNotFoundException(nameof(taggedObject));
            OldTag = oldTag ?? throw new ObjectNotFoundException(nameof(oldTag));
            NewTag = newTag ?? throw new ObjectNotFoundException(nameof(newTag));
        }
    }
}
