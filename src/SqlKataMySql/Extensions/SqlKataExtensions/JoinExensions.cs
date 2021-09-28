#region Info

// FileName:    JoinExensions.cs
// Author:      Ladislav Filip
// Created:     28.09.2021

#endregion

using Humanizer;
using SqlKata;

namespace SqlKataMySql.Extensions.SqlKataExtensions
{
    public static class JoinExensions
    {
        public static Join OnBy<TEntityJoin, TEntityOn>(this Join join, string foreignKeyName = null, bool pluralize = true)
        {
            var singularJoin = typeof(TEntityJoin).Name;
            var tableJoin = pluralize ? singularJoin.Pluralize() : singularJoin;

            var singularOn = typeof(TEntityOn).Name;
            var tableOn = pluralize ? singularOn.Pluralize() : singularOn;
            
            var first = tableJoin + "." + singularJoin + "Id";
            var second = string.IsNullOrEmpty(foreignKeyName) ? tableOn + "." + singularJoin + "Id" : tableOn + "." + foreignKeyName;

            join = join.On(first, second);
            
            return join;
        }
    }
}