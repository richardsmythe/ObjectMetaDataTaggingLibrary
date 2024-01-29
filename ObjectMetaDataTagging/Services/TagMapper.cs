using ObjectMetaDataTagging.Interfaces;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Services
{
    public class TagMapper<TSource, TTarget> : ITagMapper<TSource, TTarget>
    {
        public async Task<TTarget> MapTagsBetweenTypes<TSource, TTarget>(TSource sourceObject, TTarget targetObject)
        {
            if (sourceObject == null)
            {
                throw new ArgumentNullException(nameof(sourceObject));
            }

            if (targetObject == null)
            {
                throw new ArgumentNullException(nameof(targetObject));
            }

            var targetType = typeof(TTarget);
            var sourceType = sourceObject.GetType();

            var sourceObjProperties = sourceType.GetProperties();
            var targetObjProperties = targetType.GetProperties()
                .Where(prop => prop.CanWrite)
                .ToDictionary(prop => prop.Name);

            foreach (var sourceProp in sourceObjProperties)
            {
                if (!targetObjProperties.TryGetValue(sourceProp.Name, out var targetProp))
                {
                    continue;
                }

                var value = sourceProp.GetValue(sourceObject);

                if (value != null && targetProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                {
                    targetProp.SetValue(targetObject, value);
                }
                else if (targetProp.PropertyType == typeof(Task) && sourceProp.PropertyType == typeof(Task))
                {
                    if (value is Task task)
                    {
                        await task.ConfigureAwait(false);
                    }
                }
            }

            return targetObject;
        }
    }
}