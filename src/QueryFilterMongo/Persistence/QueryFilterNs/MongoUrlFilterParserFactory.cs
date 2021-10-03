using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace QueryFilterMongo.Persistence.QueryFilterNs
{
    public class MongoUrlFilterParserFactory : IMongoUrlFilterParserFactory
    {
        public UrlFilterParserDynamic<TEntity> Create<TEntity>(Dictionary<string, Type> allowFields)
        {
            var result = new UrlFilterParserDynamic<TEntity>(allowFields);
            return result;
        }

        public DynamicQueryParams<TEntity> Parse<TEntity>(Dictionary<string, Type> allowFields, IDictionary<string, StringValues> filter)
        {
            var parser = Create<TEntity>(allowFields);
            return parser.Parse(filter);
        }
    }
}