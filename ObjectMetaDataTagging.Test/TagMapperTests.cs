using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;

namespace ObjectMetaDataTagging.Test
{
    public class TagMapperTests
    {
        [Fact]
        public async Task TagMapper_ShouldMapTagsFromOneTypeToAnother()
        {
            // Arrange
            var taggingService = new InMemoryTaggingService<BaseTag>(new TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs>());
            var obj1 = new PersonTranscation { Amount = 1500, Sender = "Richard", Receiver = "Bob" };
            var tag = new BaseTag("TestTag", "Warning", "A string tag");
            var tagMapper = new TagMapper<TestTagType>();

            await taggingService.SetTagAsync(obj1, tag);

            // Act
            var mappedObject = await tagMapper.MapTagsBetweenTypes(tag);

            // Assert
            Assert.NotNull(mappedObject);
            Assert.NotEqual(mappedObject.GetType().Name, tag.GetType().Name);
         
        }
        public class PersonTranscation
        {
            public Guid Id { get; set; } = Guid.NewGuid();
            public string Sender { get; set; }
            public string Receiver { get; set; }
            public double Amount { get; set; }
        }

        public class TestTagType : BaseTag
        {
            public string TestProperty { get; set; }

            public TestTagType() : base("DefaultName", "DefaultValue")
            {
                TestProperty = "Test";
            }

            public TestTagType(string name, object value, string description = "Test test test")
                : base(name, value, description)
            {
            }
        }


    }

}
