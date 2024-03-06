using Microsoft.Extensions.DependencyInjection;
using ObjectMetaDataTagging.Events;
using ObjectMetaDataTagging.Helpers;
using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.TagModels;
using ObjectMetaDataTagging.Services;

namespace ObjectMetaDataTagging.Configuration
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddObjectMetaDataTagging(this IServiceCollection services)
        {
            services.AddScoped(typeof(IDefaultTaggingService<>), typeof(InMemoryTaggingService<>));
            services.AddScoped<ITagFactory, TagFactory>();
            services.AddScoped(typeof(ITagMapper<,>), typeof(TagMapper<,>));
            services.AddScoped(typeof(IDynamicQueryBuilder<>), typeof(DynamicQueryBuilder<>));
            services.AddScoped<ITaggingManager<BaseTag>, TaggingManager<BaseTag>>();

            return services;
        }
    }
}
