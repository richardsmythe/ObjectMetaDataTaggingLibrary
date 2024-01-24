Object metadata tagging library
================================================================
To use this library, add:

builder.Services.AddObjectMetaDataTagging()

In your program.cs file.

Quick start instructions:

Developers can easily customise tagging behaviour by implementing the IDefaultTaggingService<T> interface. 
This interface defines methods for managing tags associated with objects, allowing you to tailor the functionality to meet your specific requirements. 
The DefaultTaggingService<T> in this library serves as a versatile entry point for managing tagging operations. 
It is designed to allow developers to easily switch between different implementations of the IDefaultTaggingService<T> interface without modifying their application code.

If you prefer to use the tagging features out of the box without customisation, you can directly utilise the InMemoryTaggingService<T> class provided by the library. 
This class implements the default tagging behavior, and you can use it as-is or extend its functionality by overriding specific methods.

To ensure compatibility, make sure your tag data model extends the BaseTag class provided by the library. 

Utilise the TaggingEventManager<TAdded, TRemoved, TUpdated> class to manage tag-related events like addition, removal, and updates.
Simply subscribe to the TagAdded, TagRemoved, and TagUpdated events to execute custom logic when these events occur. 

Customise event handling by implementing the IAsyncEventHandler<T> interface for specific event arguments (AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs). 
Remember to handle exceptions appropriately, using specific exception types like TagAdditionException, TagRemovalException, and TagUpdateException provided by the library.