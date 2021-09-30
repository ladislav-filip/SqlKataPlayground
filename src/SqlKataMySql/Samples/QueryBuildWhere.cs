#region Info

// FileName:    QueryBuildWhere.cs
// Author:      Ladislav Filip
// Created:     30.09.2021

#endregion

using System;
using System.Linq;
using System.Threading.Tasks;
using SqlKata.Execution;
using SqlKataMySql.Domains;
using SqlKataMySql.Extensions.SqlKataExtensions;
using SqlKataMySql.Persistence;

namespace SqlKataMySql.Samples
{
    public class QueryBuildWhere : QueryBuildBase
    {
        public QueryBuildWhere(ICustomQueryFactory customQueryFactory) : base(customQueryFactory)
        {
        }

        public async Task GetByDate()
        {
            // https://sqlkata.com/docs/where-date
            
            var qf = _customQueryFactory.Query();
            var data = (await qf.QueryBy<Address>()
                .WhereDate(nameof(Address.DateCreated), ">",  DateTime.Today.AddDays(-7))
                .GetAsync<Address>()).ToArray();
            Print(data);
        }
        
        public async Task GetByTime()
        {
            var qf = _customQueryFactory.Query();
            var data = (await qf.QueryBy<Address>()
                .WhereTime(nameof(Address.DateCreated), ">",  DateTime.Today.AddDays(-7))
                .GetAsync<Address>()).ToArray();
            Print(data);
        }
    }
}