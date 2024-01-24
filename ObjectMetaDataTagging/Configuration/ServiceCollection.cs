using Microsoft.Extensions.DependencyInjection;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Helpers;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Services;

namespace ObjectMetaDataTagging.Configuration
{
    public static class ServiceCollection
    {
        public static IServiceCollection AddObjectMetaDataTagging(this IServiceCollection services)
        {
            // Register defaultTaggingService as singleton so the same instance of the service is across the whole http request
            services.AddSingleton(typeof(IDefaultTaggingService<>), typeof(InMemoryTaggingService<>));

            // Register IDynamicQueryBuilder with its three type parameters
            services.AddScoped(typeof(IDynamicQueryBuilder<,,>), typeof(DynamicQueryBuilder<,,>));

            services.AddSingleton<ITagFactory, TagFactory>();

            // Register the EventManager
            services.AddSingleton<TaggingEventManager<AsyncTagAddedEventArgs, AsyncTagRemovedEventArgs, AsyncTagUpdatedEventArgs>>();

            // Register ObjectMetaDataTaggingFacade<T> for BaseTag
            services.AddScoped(typeof(ObjectMetaDataTaggingFacade<>));

            return services;
        }
    }
}
