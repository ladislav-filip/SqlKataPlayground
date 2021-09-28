#region Info

// FileName:    QueryBuildAddresWithCityType.cs
// Author:      Ladislav Filip
// Created:     27.09.2021

#endregion

using System.Linq;
using SqlKata;
using SqlKata.Execution;
using SqlKataMySql.Domains;
using SqlKataMySql.Extensions.SqlKataExtensions;
using SqlKataMySql.Persistence;
using SqlKataMySql.Samples.Models;

namespace SqlKataMySql.Samples
{
    public class QueryBuildAddressesWithCityType : QueryBuildBase
    {
        public QueryBuildAddressesWithCityType(ICustomQueryFactory customQueryFactory) : base(customQueryFactory)
        {
        }
        
        public void LimitFivePluralize()
        {
            var qf = _customQueryFactory.Query();
            var data = qf.QueryBy<Address>()
                .SelectBy<AddressModel>()
                .JoinBy<CityType, Address>()
                .LeftJoinBy<User, Address>(nameof(Address.CreateByUserId))
                .Limit(5)
                .OrderBy(nameof(Address.City))
                .Get<AddressModel>()
                .ToArray();
                
            
            Print(data);
        }
        
        public void LimitFive()
        {
            var qf = _customQueryFactory.Query();
            var data = qf.Query("Addresses")
                .Join("CityTypes", "Addresses.CityTypeId", "CityTypes.CityTypeId")
                .LeftJoin("Users", "Users.UserId", "Addresses.CreateByUserId")
                .Limit(5)
                .OrderBy(nameof(Address.City))
                .Get<AddressModel>()
                .ToArray();

            Print(data);
        }
        
        public void LimitFiveForJosefPluralize()
        {
            var qf = _customQueryFactory.Query();
            var data = qf.QueryBy<Address>()
                .SelectBy<AddressModel>()
                .JoinBy<CityType, Address>()
                .JoinBy<User>(j => j.OnBy<User, Address>(nameof(Address.CreateByUserId)).WhereBy<User, Join>(nameof(User.Name), "Josef"))
                .Limit(5)
                .OrderBy(nameof(Address.City))
                .Get<AddressModel>()
                .ToArray();

            Print(data);
        }
        
        public void LimitFiveForJosef()
        {
            var qf = _customQueryFactory.Query();
            var data = qf.Query("Addresses")
                .Join("CityTypes", "Addresses.CityTypeId", "CityTypes.CityTypeId")
                .Join("Users", j => j.On("Users.UserId", "Addresses.CreateByUserId").Where("Users.Name", "Josef"))
                .Limit(5)
                .OrderBy(nameof(Address.City))
                .Get<AddressModel>()
                .ToArray();

            Print(data);
        }
    }
}