Object metadata tagging library
================================================================

The Object MetaData Tagging Library allows you to manage and query metadata tags associated with objects. This library provides a TaggingManager class that serves as the central point for interacting with various tagging-related functionalities.

E.g. var taggingManager = new TaggingManager<BaseTag>();

Adjust the type parameter based on your own types, but they must inhert from BaseTag.

builder.Services.AddObjectMetaDataTagging() In your program.cs file.

The TaggingManager instance provides methods for various overridable tagging operations, such as:

Add a Tag:

await taggingManager.SetTagAsync(myObject, myTag);

Bulk add Tag:

await taggingManager.BulkAddTagsAsync(targetObject, tagsToAdd);

Update a Tag:

await taggingManager.UpdateTagAsync(myObject, tagId, newTag);

Get All Tags:

var allTags = await taggingManager.GetAllTags(myObject);

Remove All Tags:

await taggingManager.RemoveAllTagsAsync(myObject);

Check if Object Has a Tag:

var hasTag = taggingManager.HasTag(myObject, tagId)

Mapping Between Object Types:

var targetObject = new TargetObjectType();
await taggingManager.MapTagsBetweenTypes(sourceObject, targetObject);

Build dynamic queries based on your own criteria:

Func<BaseTag, bool> tagFilter = tag => tag.Name == "TagName";
var filteredTags = await taggingManager.BuildQuery(allTags, tagFilter, LogicalOperator.AND);

Create Tags Using TagFactory, either single or multiple tags:

var tagFactory = new TagFactory();
var baseTag = tagFactory.CreateBaseTag("TagName", "TagValue", "TagDescription");

If you have a list of tag specifications, you can create multiple tags at once:

var tags = tagFactory.CreateBaseTags(tagList);


Event Subscription:
The TaggingManager provides events that allow you to respond to tag-related actions. These events can be useful for implementing custom logic or triggering additional actions in your application.

You can subscribe to events using standard C# event handlers:

taggingManager.TagAdded += (sender, args) => { /* Handle added tag event */ };
taggingManager.TagRemoved += (sender, args) => { /* Handle removed tag event */ };
taggingManager.TagUpdated += (sender, args) => { /* Handle updated tag event */ };

Object Graph Builder:

The ObjectGraphBuilder class is part of the tagging library and provides functionality to visualise the relationships between tagged objects in a graph format. 
This can be useful for understanding the interconnectedness of objects based on their tags.

var objectGraph = await _taggingManager.GetObjectGraph();
ObjectGraphBuilder.PrintObjectGraph(objectGraph);

A custom tagging service can be created if you don't want to use the default in-memory tagging service. 
When setting up the TaggingManager, use your custom tagging service instead of InMemoryTaggingService.

For example:

var databaseTaggingService = new DatabaseTaggingService<BaseTag>(/* ... other dependencies ... */);
var taggingManager = new TaggingManager<BaseTag>(
    databaseTaggingService,
    new TagFactory(),
    new TagMapper<BaseTag, BaseTag>(),
    new DynamicQueryBuilder<BaseTag>()
);
