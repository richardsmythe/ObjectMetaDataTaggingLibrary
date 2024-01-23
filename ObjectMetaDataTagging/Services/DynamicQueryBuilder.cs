using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.QueryModels;
using System.Linq.Expressions;

namespace ObjectMetaDataTagging.Services
{
    /// <summary>
    /// Provides a dynamic query builder for filtering items based on custom filter criteria.
    /// </summary>
    /// <typeparam name="TProperty1">Type of the first property used in filtering.</typeparam>
    /// <typeparam name="TProperty2">Type of the second property used in filtering.</typeparam>
    /// <typeparam name="TItem">Type of the items being filtered.</typeparam>
    public class DynamicQueryBuilder<TProperty1, TProperty2, TItem> : 
        IDynamicQueryBuilder<TProperty1, TProperty2, TItem>
    {
        /// <summary>
        /// Builds a dynamic query to filter items based on custom filter criteria.
        /// </summary>
        /// <param name="source">A list of items to be filtered.</param>
        /// <param name="property1Filter">Delegate-based filter for the first property.</param>
        /// <param name="property2Filter">Delegate-based filter for the second property.</param>
        /// <param name="logicalOperator">Logical operator used to combine filter expressions.</param>
        /// <returns>An IQueryable representing the filtered results.</returns>
        public IQueryable<TItem> BuildDynamicQuery(
            List<TItem> source,
            Func<TItem, bool>? property1Filter = null,
            Func<TItem, bool>? property2Filter = null,
            LogicalOperator logicalOperator = LogicalOperator.OR)
        {
            if (property1Filter == null && property2Filter == null)
            {
                Console.WriteLine("No filter conditions found.");
                return source.AsQueryable();
            }

            var parameter = Expression.Parameter(typeof(TItem), "item");
            Console.WriteLine($"Parameter name: {parameter.Name}");
            Expression? predicateBody = null;

            if (property1Filter != null)
            {
                predicateBody = predicateBody == null
                    ? Expression.Invoke(Expression.Constant(property1Filter), parameter)
                    : logicalOperator == LogicalOperator.AND
                        ? Expression.AndAlso(predicateBody, Expression.Invoke(Expression.Constant(property1Filter), parameter))
                        : Expression.OrElse(predicateBody, Expression.Invoke(Expression.Constant(property1Filter), parameter));
                Console.WriteLine("property1Filter: " + property1Filter.ToString());
            }

            if (property2Filter != null)
            {
                predicateBody = predicateBody == null
                    ? Expression.Invoke(Expression.Constant(property2Filter), parameter)
                    : logicalOperator == LogicalOperator.AND
                        ? Expression.AndAlso(predicateBody, Expression.Invoke(Expression.Constant(property2Filter), parameter))
                        : Expression.OrElse(predicateBody, Expression.Invoke(Expression.Constant(property2Filter), parameter));
                Console.WriteLine("property2Filter: " + property2Filter.ToString());
            }

            Console.WriteLine($"predicatebody: {predicateBody}");

            if (predicateBody == null)
            {
                Console.WriteLine("No valid filter predicates, returning all items.");
                return source.AsQueryable();
            }

            var lambda = Expression.Lambda<Func<TItem, bool>>(predicateBody, parameter);
            Console.WriteLine("Filter expression: " + lambda.ToString());

            var result = source.AsQueryable().Where(lambda);
            Console.WriteLine("Filtered items count: " + result.Count());

            return result;
        }
    }
}
