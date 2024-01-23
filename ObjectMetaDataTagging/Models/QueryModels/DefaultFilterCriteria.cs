namespace ObjectMetaDataTagging.Models.QueryModels
{
    public abstract class DefaultFilterCriteria
    {
        public string Name { get; set; }
        public string Type { get; set; }

        protected DefaultFilterCriteria(string name, string type)
        {
            Name = name;
            Type = type;
        }
    }
}