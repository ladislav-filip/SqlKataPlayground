#region Info

// FileName:    IMongoUrlFilterParserFactory.cs
// Author:      Ladislav Filip
// Created:     03.10.2021

#endregion

using System;
using System.Collections.Generic;
using Microsoft.Extensions.Primitives;

namespace QueryFilterMongo.Persistence.QueryFilterNs
{
    public interface IMongoUrlFilterParserFactory
    {
        UrlFilterParserDynamic<TEntity> Create<TEntity>(Dictionary<string, Type> allowFields);
        DynamicQueryParams<TEntity> Parse<TEntity>(Dictionary<string, Type> allowFields, IDictionary<string, StringValues> filter);
    }
}