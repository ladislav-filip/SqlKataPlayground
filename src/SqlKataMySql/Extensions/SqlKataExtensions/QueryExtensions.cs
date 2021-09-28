#region Info

// FileName:    QueryExtensions.cs
// Author:      Ladislav Filip
// Created:     28.09.2021

#endregion

using System;
using System.Reflection;
using Humanizer;
using SqlKata;
using SqlKataMySql.Infrastructure;

namespace SqlKataMySql.Extensions.SqlKataExtensions
{
    public static class QueryExtensions
    {
        private const string InnerJoin = "inner join";
        private const string LeftJoin = "left join";
        private const string RightJoin = "right join";
        
        public static Query SelectBy<TModel>(this Query query)
        {
            var type = typeof(TModel);
            var props = type.GetProperties();

            foreach (var pi in props)
            {
                var attr = pi.GetCustomAttribute<FieldAttribute>();

                if (attr != null)
                {
                    query = query.Select(attr.FieldName + " as " + pi.Name);
                }
                else
                {
                    query = query.Select(pi.Name);
                }
            }
            
            return query;
        }

        public static Query JoinBy<TEntityJoin>(this Query query, Func<Join, Join> callback, bool pluralize = true)
        {
            var singularJoin = typeof(TEntityJoin).Name;
            var tableJoin = pluralize ? singularJoin.Pluralize() : singularJoin;

            query = query.Join(tableJoin, callback);
            
            return query;
        }
        
        public static Query JoinBy<TEntityJoin, TEntityOn>(this Query query, string foreignKeyName = null, bool pluralize = true)
        {
            return JoinByInternal<TEntityJoin, TEntityOn>(query, InnerJoin, foreignKeyName, pluralize);
        }

        public static Query LeftJoinBy<TEntityJoin, TEntityOn>(this Query query, string foreignKeyName = null, bool pluralize = true)
        {
            return JoinByInternal<TEntityJoin, TEntityOn>(query, LeftJoin, foreignKeyName, pluralize);
        }
        
        public static Query RightJoinBy<TEntityJoin, TEntityOn>(this Query query, string foreignKeyName = null, bool pluralize = true)
        {
            return JoinByInternal<TEntityJoin, TEntityOn>(query, RightJoin, foreignKeyName, pluralize);
        }
        
        private static Query JoinByInternal<TEntityJoin, TEntityOn>(Query query, string type, string foreignKeyName = null, bool pluralize = true)
        {
            var singularJoin = typeof(TEntityJoin).Name;
            var tableJoin = pluralize ? singularJoin.Pluralize() : singularJoin;

            var singularOn = typeof(TEntityOn).Name;
            var tableOn = pluralize ? singularOn.Pluralize() : singularOn;

            var first = tableJoin + "." + singularJoin + "Id";
            var second = string.IsNullOrEmpty(foreignKeyName) ? tableOn + "." + singularJoin + "Id" : tableOn + "." + foreignKeyName;

            query = query.Join(tableJoin, first, second, type: type);

            return query;
        }
    }
}