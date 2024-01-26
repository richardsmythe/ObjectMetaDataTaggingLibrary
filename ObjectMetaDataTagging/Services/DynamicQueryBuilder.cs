using ObjectMetaDataTagging.Interfaces;
using ObjectMetaDataTagging.Models.QueryModels;
using System.Linq.Expressions;

namespace ObjectMetaDataTagging.Services
{
    /// <summary>
    /// Provides a dynamic query builder for filtering items based on custom filter criteria.
    /// </summary>
    /// <typeparam name="TItem">Type of the items being filtered.</typeparam>
    public class DynamicQueryBuilder<TItem> : IDynamicQueryBuilder<TItem>
    {
        /// <summary>
        /// Gets or sets the list of property filters.
        /// </summary>
        private List<Func<TItem, bool>> Filters { get; } = new List<Func<TItem, bool>>();

        /// <summary>
        /// Gets or sets the logical operator used to combine filter expressions.
        /// </summary>
        private LogicalOperator LogicalOperator { get; set; } = LogicalOperator.OR;

        /// <summary>
        /// Adds a property filter to the list of filters.
        /// </summary>
        /// <param name="propertyFilter">Delegate-based filter for a property.</param>
        /// <returns>The current instance of the <see cref="DynamicQueryBuilder{TItem}"/>.</returns>
        public IDynamicQueryBuilder<TItem> WithPropertyFilter(Func<TItem, bool> propertyFilter)
        {
            Filters.Add(propertyFilter);
            return this;
        }

        /// <summary>
        /// Sets the logical operator used to combine filter expressions.
        /// </summary>
        /// <param name="logicalOperator">Logical operator to be set.</param>
        /// <returns>The current instance of the <see cref="DynamicQueryBuilder{TItem}"/>.</returns>
        public IDynamicQueryBuilder<TItem> SetLogicalOperator(LogicalOperator logicalOperator)
        {
            LogicalOperator = logicalOperator;
            return this;
        }

        /// <summary>
        /// Builds a dynamic query to filter items based on custom filter criteria.
        /// </summary>
        /// <param name="source">A list of items to be filtered.</param>
        /// <returns>An IQueryable representing the filtered results.</returns>
        public IQueryable<TItem> BuildDynamicQuery(List<TItem> source)
        {
            if (Filters.Count == 0)
            {
                Console.WriteLine("No filter conditions found");
                return source.AsQueryable();
            }

            var parameter = Expression.Parameter(typeof(TItem), "item");

            // Create a seed expression based on the logical operator
            Expression seedExpression = LogicalOperator == LogicalOperator.AND
                ? Expression.Constant(true)  // true for AND
                : Expression.Constant(false); // false for OR

            // Combine the filters using the appropriate binary operator
            Expression predicateBody = Filters
                .Select(filter => Expression.Invoke(Expression.Constant(filter), parameter))
                .Aggregate(seedExpression,
                           (current, next) => LogicalOperator == LogicalOperator.AND
                               ? Expression.AndAlso(current, next)
                               : Expression.OrElse(current, next));

            var lambda = Expression.Lambda<Func<TItem, bool>>(predicateBody, parameter);
            Console.WriteLine("Filter expression: " + lambda);

            var result = source.AsQueryable().Where(lambda);
            Console.WriteLine("Filtered items count: " + result.Count());

            return result;
        }
    }
}
