using ObjectMetaDataTagging.Models.QueryModels;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;

namespace ObjectMetaDataTagging.Test
{
    public class DynamicQueryBuilderTests
    {
        [Fact]
        public void BuildDynamicQuery_ShouldFilterTagsCorrectlyUsingLogicalOperatorAnd()
        {
            // Arrange
            var dynamicQueryBuilder = new DynamicQueryBuilder<BaseTag>();

            var tags = GenerateTags();
            var filterName = "Tag1";
            var filterType = "TypeA";
            var customFilter = new CustomFilter(filterName, filterType);

            // Act
            var filteredResults = dynamicQueryBuilder
                .WithPropertyFilter(t => t.Name == customFilter.Name)
                .WithPropertyFilter(t => t.Type == customFilter.Type)
                .SetLogicalOperator(LogicalOperator.AND)
                .BuildDynamicQuery(tags);             
            

            // Assert
            Assert.All(filteredResults,
                tag =>
                {
                    Assert.Equal(filterName, tag.Name);
                    Assert.Equal(filterType, tag.Type);
                }
            );
        }

        private List<BaseTag> GenerateTags()
        {
            var tags = new List<BaseTag>();
            var random = new Random();

            for (int i = 1; i <= 5; i++)
            {
                var tagName = $"Tag{i}";
                var tagType = $"Type{(char)('A' + random.Next(5))}";
                var tagDescription = $"Description {i}";

                tags.Add(new BaseTag(tagName, tagType, tagDescription));
            }

            return tags;
        }

        public class CustomFilter : DefaultFilterCriteria
        {
            public CustomFilter(string name, string type)
                : base(name, type)
            {
            }
        }
    }
}
