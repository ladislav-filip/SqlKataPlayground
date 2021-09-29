#region Info
// FileName:    DapperBuildAddress.cs
// Author:      Ladislav Filip
// Created:     28.09.2021
#endregion

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Diagnostics;
using Dapper;
using SqlKataMySql.Domains;
using SqlKataMySql.Samples.Models;

namespace SqlKataMySql.Samples
{
    public class DapperBuildAddress
    {
        private readonly DbConnection _connection;

        public DapperBuildAddress(DbConnection connection)
        {
            _connection = connection;
        }
        
        public void ByCityContains(string searchCity)
        {
            var parameters = new DynamicParameters(new { Search = $"%{searchCity}%" });
            var data = _connection.Query<Address>("SELECT * FROM Addresses WHERE City LIKE @Search ORDER BY City", parameters);
            Print(data);
        }

        public void GetJoined()
        {
            const string sql = @"select City, Street, CT.Name, CitiziensCount 
from Addresses a
inner join CityTypes CT on a.CityTypeId = CT.CityTypeId
inner join Users U on a.CreateByUserId = U.UserId
order by City";
            var data = _connection.Query<AddressModel>(sql);
            Print(data);
        }
        
        [Conditional("DEBUG")]
        protected void Print(IEnumerable<object> data)
        {
            foreach (var d in data)
            {
                Console.WriteLine(ObjectDumper.Dump(d));
                Console.WriteLine("-");
            }
            Console.WriteLine();
        }
    }
}