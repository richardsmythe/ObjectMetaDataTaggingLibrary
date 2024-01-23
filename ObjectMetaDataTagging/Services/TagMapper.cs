using ObjectMetaDataTagging.Interfaces;

namespace ObjectMetaDataTagging.Services
{
    public class TagMapper<T> : ITagMapper<T>
    {
        /// <summary>
        /// Maps properties from the source object to the target type asynchronously, allowing tags to be mapped from one tag type to another.
        /// </summary>
        /// <param name="sourceObject">The source object from which properties will be mapped.</param>
        /// <returns>An instance of the target type with mapped properties.</returns>
        /// <exception cref="ArgumentNullException">
        /// Thrown when the <paramref name="sourceObject"/> is null.
        /// </exception>
        public async Task<T> MapTagsBetweenTypes(object sourceObject)
        {
            if (sourceObject == null)
            {
                throw new ArgumentNullException(nameof(sourceObject));
            }

            var targetType = typeof(T);
            var sourceType = sourceObject.GetType();

            var sourceObjProperties = sourceType.GetProperties();
            var targetObjProperties = targetType.GetProperties()
                .Where(prop => prop.CanWrite)
                .ToDictionary(prop => prop.Name);

            var targetInstance = Activator.CreateInstance(targetType);

            foreach (var sourceProp in sourceObjProperties)
            {
                if (!targetObjProperties.TryGetValue(sourceProp.Name, out var targetProp))
                {
                    continue;
                }

                var value = sourceProp.GetValue(sourceObject);

                if (value != null && targetProp.PropertyType.IsAssignableFrom(sourceProp.PropertyType))
                {
                    targetProp.SetValue(targetInstance, value);
                }
                else if (targetProp.PropertyType == typeof(Task) && sourceProp.PropertyType == typeof(Task))
                {
                    if (value is Task task)
                    {
                        await task.ConfigureAwait(false);
                    }
                }
            }
            return (T)targetInstance;
        }
    }
}