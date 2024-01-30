using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;
using System;
using System.Threading.Tasks;
using Xunit;

namespace ObjectMetaDataTagging.Test
{
    public class TagMapperTests
    {
        [Fact]
        public async Task TagMapper_ShouldMapTagsFromOneTypeToAnother()
        {
            // Arrange
            var tagMapper = new TagMapper<BaseTag, TestTagType>();
            var taggingService = new InMemoryTaggingService<BaseTag>();
            var obj1 = new PersonTransaction { Amount = 1500, Sender = "Richard", Receiver = "Bob" };
            var sourceTagToMap = new BaseTag("TestTag", "Warning", "A string tag");

            await taggingService.SetTagAsync(obj1, sourceTagToMap);

            // Act
            var mappedObject = await tagMapper.MapTagsBetweenTypes(sourceTagToMap, new TestTagType());

            // Assert
            Assert.NotNull(mappedObject);
            Assert.NotEqual(sourceTagToMap.GetType().Name, mappedObject.GetType().Name);
            Assert.Equal(sourceTagToMap.Name, mappedObject.Name);
            Assert.Equal(sourceTagToMap.Value, mappedObject.Value);
            Assert.Equal(sourceTagToMap.Description, mappedObject.Description);

            Assert.Equal("Test", mappedObject.TestProperty1);
            Assert.Equal("Test2", mappedObject.TestProperty2);
        }

        public class PersonTransaction
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Sender { get; set; }
            public string Receiver { get; set; }
            public double Amount { get; set; }
        }

        public class TestTagType : BaseTag
        {
            public string TestProperty1 { get; set; }
            public string TestProperty2 { get; set; }

            public TestTagType()
            {
                TestProperty1 = "Test";
                TestProperty2 = "Test2";
            }
        }
    }
}
