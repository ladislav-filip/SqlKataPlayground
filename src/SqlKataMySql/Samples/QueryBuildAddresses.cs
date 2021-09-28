using System;
using System.Collections.Generic;
using SqlKata.Execution;
using SqlKataMySql.Domains;
using SqlKataMySql.Persistence;

namespace SqlKataMySql.Samples
{
    public class QueryBuildAddresses : QueryBuildBase
    {
        public QueryBuildAddresses(ICustomQueryFactory customQueryFactory) : base(customQueryFactory)
        {
        }
        
        public void LimitFive()
        {
            var qf = _customQueryFactory.Query();
            var data = qf.Query("Addresses").Limit(5).OrderBy(nameof(Address.City)).Get<Address>();
            Print(data);
        }

        public void LimitFiveByCity(string searchCity)
        {
            var qf = _customQueryFactory.Query();
            var data = qf.Query("Addresses")
                .Where(nameof(Address.City), searchCity)
                .Limit(5)
                .OrderBy(nameof(Address.City)).Get<Address>();
            Print(data);
        }
        
        public void ByCityContains(string searchCity)
        {
            var qf = _customQueryFactory.Query();
            var data = qf.Query("Addresses")
                .WhereContains(nameof(Address.City), searchCity)
                .OrderBy(nameof(Address.City)).Get<Address>();
            Print(data);
        }
        
        public void LimitFiveByNumber(int numberGreater)
        {
            var qf = _customQueryFactory.Query();
            var data = qf.Query("Addresses")
                .Where(nameof(Address.Number), ">", numberGreater)
                .Limit(5)
                .OrderBy(nameof(Address.City)).Get<Address>();
            Print(data);
        }
    }
}