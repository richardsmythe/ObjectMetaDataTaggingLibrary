using ObjectMetaDataTagging.Models.QueryModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IDynamicQueryBuilder<TProperty1, TProperty2, TItem>
    {
        IQueryable<TItem> BuildDynamicQuery(
            List<TItem> sourceObject,
            Func<TItem, bool>? property1Filter = null,
            Func<TItem, bool>? property2Filter = null,
            LogicalOperator logicalOperator = LogicalOperator.OR);
    }
}
