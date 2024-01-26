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
            services.AddScoped(typeof(IDynamicQueryBuilder<,,>), typeof(DynamicQueryBuilder<,,>));
            services.AddSingleton<ITagFactory, TagFactory>();
            services.AddSingleton(typeof(ITagMapper<>), typeof(TagMapper<>));
            services.AddScoped<IObjectMetaDataTaggingFacade<BaseTag>, ObjectMetaDataTaggingFacade<BaseTag>>();

            return services;
        }
    }
}
