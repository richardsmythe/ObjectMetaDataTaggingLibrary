using ObjectMetaDataTagging.Models.QueryModels;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;
using Xunit;

namespace ObjectMetaDataTagging.Test
{
    public class DynamicQueryBuilderTests
    {
        [Fact]
        public void BuildDynamicQuery_ShouldFilterTagsCorrectlyUsingLogicalOperatorAnd()
        {
            // Arrange
            var dynamicQueryBuilder = new DynamicQueryBuilder<List<BaseTag>, string, BaseTag>();

            var tags = GenerateTags();
            var filterName = "Tag1";
            var filterType = "TypeA";
            var customFilter = new CustomFilter(filterName, filterType);

            // Act
            var filteredResults = dynamicQueryBuilder.BuildDynamicQuery(
                tags,
                tag => tag.Name == customFilter.Name,
                tag => tag.Type == customFilter.Type,
                LogicalOperator.AND
            );

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
