#region Info
// FileName:    QueryBuildExample.cs
// Author:      Ladislav Filip
// Created:     27.09.2021
#endregion

using System;
using System.Collections.Generic;
using SqlKata.Execution;
using SqlKataMySql.Domains;
using SqlKataMySql.Persistence;

namespace SqlKataMySql.Samples
{
    public class QueryBuildAddresses
    {
        private readonly ICustomQueryFactory _customQueryFactory;

        public QueryBuildAddresses(ICustomQueryFactory customQueryFactory)
        {
            _customQueryFactory = customQueryFactory;
        }
        
        public void LimitFive()
        {
            var qf = _customQueryFactory.Query();
            var data = qf.Query("Addresses").Limit(5).OrderBy(nameof(Address.City)).Get<Address>();
            PrintAddresses(data);
        }

        public void LimitFiveByCity(string searchCity)
        {
            var qf = _customQueryFactory.Query();
            var data = qf.Query("Addresses")
                .Where(nameof(Address.City), searchCity)
                .Limit(5)
                .OrderBy(nameof(Address.City)).Get<Address>();
            PrintAddresses(data);
        }
        
        public void LimitFiveByCityContains(string searchCity)
        {
            var qf = _customQueryFactory.Query();
            var data = qf.Query("Addresses")
                .WhereContains(nameof(Address.City), searchCity)
                .Limit(5)
                .OrderBy(nameof(Address.City)).Get<Address>();
            PrintAddresses(data);
        }
        
        public void LimitFiveByNumber(int numberGreater)
        {
            var qf = _customQueryFactory.Query();
            var data = qf.Query("Addresses")
                .Where(nameof(Address.Number), ">", numberGreater)
                .Limit(5)
                .OrderBy(nameof(Address.City)).Get<Address>();
            PrintAddresses(data);
        }
        
        private static void PrintAddresses(IEnumerable<Address> data)
        {
            foreach (var d in data)
            {
                Console.WriteLine($"{d.Street}, {d.City} {d.Zip}, Number = {d.Number}");
            }
            Console.WriteLine();
        }
    }
}