#region Info

// FileName:    QueryFactoryExtensions.cs
// Author:      Ladislav Filip
// Created:     28.09.2021

#endregion

using Humanizer;
using SqlKata;
using SqlKata.Execution;

namespace SqlKataMySql.Extensions.SqlKataExtensions
{
    public static class QueryFactoryExtensions
    {
        public static Query QueryBy<TEntity>(this QueryFactory queryFactory, bool pluralize = true)
        {
            var table = pluralize ? typeof(TEntity).Name.Pluralize() : typeof(TEntity).Name;
            return queryFactory.Query(table);
        }
    }
}