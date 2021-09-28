#region Info

// FileName:    QExtensions.cs
// Author:      Ladislav Filip
// Created:     28.09.2021

#endregion

using Humanizer;
using SqlKata;

namespace SqlKataMySql.Extensions.SqlKataExtensions
{
    public static class QExtensions
    {
        public static TQ WhereBy<TEntityJoin, TQ>(this TQ q, string column, object value, bool pluralize = true) where TQ : BaseQuery<TQ>
        {
            var singularJoin = typeof(TEntityJoin).Name;
            var tableJoin = pluralize ? singularJoin.Pluralize() : singularJoin;
            q = q.Where(tableJoin + "." + column, value);
            return q;
        }
    }
}