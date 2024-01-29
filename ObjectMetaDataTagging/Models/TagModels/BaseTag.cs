namespace ObjectMetaDataTagging.Models.TagModels
{
    /// <summary>
    /// A base tag providing basic tag object properties that derived classes
    /// may require.
    /// </summary>
    public class BaseTag
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime DateCreated { get; private set; }
        public DateTime? DateLastUpdated { get; set; }
        public string Description { get; set; }
        public object Value { get; set; }
        public List<BaseTag> ChildTags { get; } = new List<BaseTag>();
        public List<object> Parents { get; private set; } = new List<object>();
        public string Type { get; private set; }

        public BaseTag(){}
        public BaseTag(string name, object value, string description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentNullException(nameof(name));
            }

            Id = Guid.NewGuid();
            Name = name;
            Description = description;
            SetValue(value);
            DateCreated = DateTime.UtcNow;
        }

        private void SetValue(object value)
        {
            Value = value;
            Type = value?.GetType().Name;
        }

        public void AddChildTag(BaseTag childTag)
        {
            if (childTag == null)
                throw new ArgumentNullException(nameof(childTag));

            ChildTags.Add(childTag);
            childTag.Parents.Add(this);
        }

        public override string ToString()
        {
            return Name;
        }
    }   
}