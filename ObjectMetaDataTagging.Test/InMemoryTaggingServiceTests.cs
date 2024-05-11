using Moq;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Helpers;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;

namespace ObjectMetaDataTagging.Test
{
    public class InMemoryTaggingServiceTests
    {

        [Fact]
        public async Task GetTag_ShouldReturnTag_WhenTagExists()
        {
            // Arrange
            var taggingService = new InMemoryTaggingService<BaseTag>();
            var obj = new PersonTranscation { Amount = 1244, Sender = "Richard", Receiver = "Jon" };
            var tag = new BaseTag("TestTag", "Warning", "A string tag");
            await taggingService.SetTagAsync(obj, tag);

            // Act
            var retrievedTag = await taggingService.GetTag(obj, tag.Id);

            // Assert
            Assert.NotNull(retrievedTag);
            Assert.Equal(tag, retrievedTag);
        }

        [Theory]
        [InlineData(1500)]
        [InlineData(2500)]
        public async void SetTagAsync_ShouldAddToDictionary_AndRaiseEventIfConditionIsTrue(int amount)
        {
            // Arrange       
            var taggingService = new InMemoryTaggingService<BaseTag>();
            var obj = new PersonTranscation { Amount = amount, Sender = "Richard", Receiver = "Jon" };
            var tag = new BaseTag("TestTag", 43, "A numeric tag");


            // Act
            // Set the callback to simulate a situation where the tagAddedEvent is used to add a 'Suspicous' tag
            // if a transaction amount was over 2000. 
            taggingService.OnSetTagAsyncCallback = async (o, t) =>
            {
                if (amount > 2000)
                {
                    await Task.Run(() =>
                    {
                        var suspiciousTag = new BaseTag("Suspicious", null, "This object has been tagged as suspicious.");
                        taggingService.data.TryGetValue(o, out var tagDictionary);
                        tagDictionary?.TryAdd(suspiciousTag.Id, suspiciousTag);                  
                    });
                }
            };

            await taggingService.SetTagAsync(obj, tag);
            var tags = await taggingService.GetAllTags(obj);

            // Assert
            Assert.True(taggingService.data.TryGetValue(obj, out var tagDictionary));
            Assert.True(tagDictionary.ContainsKey(tag.Id));
            Assert.True(tagDictionary.Count != 0);
            Assert.True(tagDictionary.Count == tags.Count());
            Assert.Equal(tag, tags.First());

            if (obj.Amount > 2000)
            {
                var suspiciousTagId = tagDictionary.Values.FirstOrDefault(t => t.Name == "Suspicious")!.Id;
                Assert.Equal("Suspicious", tagDictionary[suspiciousTagId].Name);
                Assert.Equal(suspiciousTagId, tagDictionary[suspiciousTagId].Id);
                Assert.Equal("This object has been tagged as suspicious.", tagDictionary[suspiciousTagId].Description);
            }
            else
            {
              
                Assert.DoesNotContain("Suspicious", tagDictionary.Values.Select(t => t.Name));
            }
        }

        [Fact]
        public async void RemoveTagAsync_ShouldRemoveGivenTagFromDictionary()
        {
            // Arrange

            var taggingService = new InMemoryTaggingService<BaseTag>();
            var obj = new PersonTranscation { Amount = 1244, Sender = "Richard", Receiver = "Jon" };
            var tag = new BaseTag("TestTag", "Warning", "A string tag");
            var tag2 = new BaseTag("TestTag2", "Warning", "A string tag");

            // Act
            await taggingService.SetTagAsync(obj, tag);
            await taggingService.SetTagAsync(obj, tag2);
            await taggingService.RemoveTagAsync(obj, tag.Id);

            // Assert
          

            Assert.False(taggingService.data.ContainsKey(tag.Id));
        }

        [Fact]
        public async void UpdateTagAsync_ShouldUpdateGivenTagInDictionary()
        {
            // Arrange
            var taggingService = new InMemoryTaggingService<BaseTag>();
            var obj = new PersonTranscation { Amount = 1244, Sender = "Richard", Receiver = "Jon" };
            var tag = new BaseTag("TestTag", "Warning", "A string tag");
            var tag2 = new BaseTag("TestTag2", "Warning", "A string tag");
            var modifiedTag = new BaseTag("TestUpdatedTag", 652, "Updated Tag");

            // Act
            await taggingService.SetTagAsync(obj, tag);
            await taggingService.SetTagAsync(obj, tag2);
            await taggingService.UpdateTagAsync(obj, tag.Id, modifiedTag);
            var tagCount = (await taggingService.GetAllTags(obj)).Count();

            // Assert
            Assert.True(taggingService.data.TryGetValue(obj, out var tags));
            Assert.False(taggingService.data.ContainsKey(modifiedTag.Id));
            Assert.Equal(tagCount, tags.Count);
        }

        [Fact]
        public void HasTag_ShouldReturnTrue_WhenObjectHasTag()
        {
            // Arrange
            var taggingService = new InMemoryTaggingService<BaseTag>();
            var obj = new PersonTranscation { Amount = 1244, Sender = "Richard", Receiver = "Jon" };
            var tag = new BaseTag("TestTag", "Warning", "A string tag");

            // Act
            taggingService.SetTagAsync(obj, tag).Wait();
            var hasTag = taggingService.HasTag(obj, tag.Id);

            // Assert
            Assert.True(hasTag);
        }

        [Fact]
        public async Task GetObjectByTag_ShouldReturnAssociatedObject_WhenTagExists()
        {
            // Arrange
            var taggingService = new InMemoryTaggingService<BaseTag>();
            var obj = new PersonTranscation { Amount = 1244, Sender = "Richard", Receiver = "Jon" };
            var tag = new BaseTag("TestTag", "Warning", "A string tag");

            // Act
            await taggingService.SetTagAsync(obj, tag);
            var result = taggingService.GetObjectByTag(tag.Id);

            // Assert
            Assert.NotNull(result);
            Assert.True(result is PersonTranscation);
        }

        [Fact]
        public async Task BulkAddTagsAsync_ShouldAdd20TagsToObject()
        {
            // Arrange
            var tagFactory = new TagFactory();
            var taggingService = new InMemoryTaggingService<BaseTag>();
            var obj = new PersonTranscation { Amount = 1244, Sender = "Richard", Receiver = "Jon" };
            var tagData = new List<(string name, object value, string description)>();

            for (int i = 1; i <= 20; i++)
            {
                string tagName = $"Tag{i}";
                string tagValue = $"Value{i}";
                string tagDescription = $"Description{i}";

                tagData.Add((tagName, tagValue, tagDescription));
            }
            IEnumerable<BaseTag> tags = tagFactory.CreateBaseTags(tagData);

            // Act
            await taggingService.BulkAddTagsAsync(obj, tags);

            // Assert
            var addedTags = await taggingService.GetAllTags(obj);
            Assert.Equal(20, addedTags.Count());
        }
    }

    public class PersonTranscation
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Sender { get; set; }
        public string Receiver { get; set; }
        public double Amount { get; set; }
    }
}
