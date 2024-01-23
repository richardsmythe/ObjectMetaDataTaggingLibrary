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
        [Theory]
        [InlineData(1500)]
        [InlineData(2500)]
        public async void SetTagAsync_ShouldAddToDictionary_AndRaiseEventIfConditionIsTrue(int amount)
        {
            // Arrange
            var mockAddedHandler = new Mock<IAsyncEventHandler<AsyncTagAddedEventArgs>>();
            var taggingEventManager = new TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs>(
                mockAddedHandler.Object, null, null
            );

            var taggingService = new InMemoryTaggingService<BaseTag>(taggingEventManager);

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
                        taggingEventManager.RaiseTagAdded(new AsyncTagAddedEventArgs(o, suspiciousTag)).Wait();
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
                mockAddedHandler.Verify(
                    handler => handler.HandleAsync(It.Is<AsyncTagAddedEventArgs>(e => e.TaggedObject == obj && e.Tag.Name == "Suspicious")),
                    Times.Once);
                var suspiciousTagId = tagDictionary.Values.FirstOrDefault(t => t.Name == "Suspicious")!.Id;
                Assert.Equal("Suspicious", tagDictionary[suspiciousTagId].Name);
                Assert.Equal(suspiciousTagId, tagDictionary[suspiciousTagId].Id);
                Assert.Equal("This object has been tagged as suspicious.", tagDictionary[suspiciousTagId].Description);
            }
            else
            {
                mockAddedHandler.Verify(
                    handler => handler.HandleAsync(It.IsAny<AsyncTagAddedEventArgs>()),
                    Times.Never);
                Assert.DoesNotContain("Suspicious", tagDictionary.Values.Select(t => t.Name));
            }
        }

        [Fact]
        public async void RemoveTagAsync_ShouldRemoveGivenTagFromDictionary_AndRaiseTagRemovedEvent()
        {
            // Arrange
            var mockRemovedHandler = new Mock<IAsyncEventHandler<AsyncTagRemovedEventArgs>>();
            var taggingEventManager = new TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs>(
                null, mockRemovedHandler.Object, null
            );

            var taggingService = new InMemoryTaggingService<BaseTag>(taggingEventManager);
            var obj = new PersonTranscation { Amount = 1244, Sender = "Richard", Receiver = "Jon" };
            var tag = new BaseTag("TestTag", "Warning", "A string tag");
            var tag2 = new BaseTag("TestTag2", "Warning", "A string tag");

            // Act
            await taggingService.SetTagAsync(obj, tag);
            await taggingService.SetTagAsync(obj, tag2);
            await taggingService.RemoveTagAsync(obj, tag.Id);

            taggingEventManager.RaiseTagRemoved(new AsyncTagRemovedEventArgs(obj, tag)).Wait();

            // Assert
            mockRemovedHandler.Verify(
                handler => handler.HandleAsync(It.Is<AsyncTagRemovedEventArgs>(e => e.TaggedObject == obj && e.Tag == tag)),
                Times.Once);

            Assert.False(taggingService.data.ContainsKey(tag.Id));
        }

        [Fact]
        public async void UpdateTagAsync_ShouldUpdateGivenTagInDictionary_AndRaiseTagRemovedEvent()
        {
            // Arrange
            var mockUpdateHandler = new Mock<IAsyncEventHandler<AsyncTagUpdatedEventArgs>>();
            var taggingEventManager = new TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs>(
                null, null, mockUpdateHandler.Object
            );

            var taggingService = new InMemoryTaggingService<BaseTag>(taggingEventManager);
            var obj = new PersonTranscation { Amount = 1244, Sender = "Richard", Receiver = "Jon" };
            var tag = new BaseTag("TestTag", "Warning", "A string tag");
            var tag2 = new BaseTag("TestTag2", "Warning", "A string tag");
            var modifiedTag = new BaseTag("TestUpdatedTag", 652, "Updated Tag");

            // Act
            await taggingService.SetTagAsync(obj, tag);
            await taggingService.SetTagAsync(obj, tag2);
            await taggingService.UpdateTagAsync(obj, tag.Id, modifiedTag);
            var tagCount = (await taggingService.GetAllTags(obj)).Count();

            taggingEventManager.RaiseTagUpdated(new AsyncTagUpdatedEventArgs(obj, tag, modifiedTag)).Wait();

            // Assert
            mockUpdateHandler.Verify(
                handler => handler.HandleAsync(It.Is<AsyncTagUpdatedEventArgs>(e => e.TaggedObject == obj && e.NewTag == modifiedTag)),
                Times.Once);

            // Assert
            mockUpdateHandler.Verify(
                handler => handler.HandleAsync(It.Is<AsyncTagUpdatedEventArgs>(e => e.TaggedObject == obj && e.NewTag == modifiedTag)),
                Times.Once);

            Assert.True(taggingService.data.TryGetValue(obj, out var tags));
            Assert.False(taggingService.data.ContainsKey(modifiedTag.Id));
            Assert.Equal(tagCount, tags.Count);
        }

        [Fact]
        public void HasTag_ShouldReturnTrue_WhenObjectHasTag()
        {
            // Arrange
            var taggingService = new InMemoryTaggingService<BaseTag>(new TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs>());
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
            var taggingService = new InMemoryTaggingService<BaseTag>(new TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs>());
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
            var taggingService = new InMemoryTaggingService<BaseTag>(new TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs>());
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
