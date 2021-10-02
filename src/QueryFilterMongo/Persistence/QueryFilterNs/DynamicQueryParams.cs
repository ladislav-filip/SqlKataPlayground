using System.Collections.Generic;
using Microsoft.Extensions.Primitives;
using MongoDB.Driver;

namespace QueryFilterMongo.Persistence.QueryFilterNs
{
    public class DynamicQueryParams<TEntity>
    {
        public readonly IDictionary<string, StringValues> UnknownParams = new Dictionary<string, StringValues>();

        public FilterDefinition<TEntity> Filter;
        
        public FindOptions<TEntity> FindOptions;

        public int? Limit;
        
        public int? Offset;
    }
}