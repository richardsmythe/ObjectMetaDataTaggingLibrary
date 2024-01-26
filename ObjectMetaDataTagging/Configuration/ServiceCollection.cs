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
            services.AddSingleton(typeof(IDefaultTaggingService<>), typeof(InMemoryTaggingService<>));
            services.AddSingleton<ITagFactory, TagFactory>();
            services.AddSingleton(typeof(ITagMapper<>), typeof(TagMapper<>));
            services.AddSingleton(typeof(IDynamicQueryBuilder<>), typeof(DynamicQueryBuilder<>));
            services.AddScoped<IObjectMetaDataTaggingFacade<BaseTag>, ObjectMetaDataTaggingFacade<BaseTag>>();

            return services;
        }
    }
}
