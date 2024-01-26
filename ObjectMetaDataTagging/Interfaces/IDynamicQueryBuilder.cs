using ObjectMetaDataTagging.Models.QueryModels;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ObjectMetaDataTagging.Interfaces
{
    public interface IDynamicQueryBuilder<TItem>
    {
        IDynamicQueryBuilder<TItem> WithPropertyFilter(Func<TItem, bool> propertyFilter);
        IDynamicQueryBuilder<TItem> SetLogicalOperator(LogicalOperator logicalOperator);
        IQueryable<TItem> BuildDynamicQuery(List<TItem> source);
    }
}
