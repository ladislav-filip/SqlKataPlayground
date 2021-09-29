#region Info
// FileName:    Seeder.cs
// Author:      Ladislav Filip
// Created:     27.09.2021
#endregion

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Converters;
using SqlKataMySql.Domains;

namespace SqlKataMySql.Persistence
{
    public class Seeder
    {
        private readonly KataDbContext _dbContext;

        public Seeder(KataDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task SeedAsync()
        {
            await _dbContext.Database.EnsureDeletedAsync();
            await _dbContext.Database.EnsureCreatedAsync();
            await CreateAddressesView();
            
            // naplníme větším množstvím dat
            await SeedAddressesAsync();
            await SeedAddressesAsync();
            await SeedAddressesAsync();
            await SeedAddressesAsync();
            await SeedAddressesAsync();
            await SeedAddressesAsync();
            
            Console.WriteLine();
            Console.WriteLine();
        }

        private async Task CreateAddressesView()
        {
            const string sql = @"create view AddressesView
as
select a.AddressId, City, Street, Zip, Number, a.CityTypeId, CreateByUserId,
       CT.Name as CityTypeName, CitiziensCount,
       U.UserId, U.Name, Surname
from Addresses a
         inner join CityTypes CT on a.CityTypeId = CT.CityTypeId
         inner join Users U on a.CreateByUserId = U.UserId
order by City";
            await _dbContext.Database.ExecuteSqlRawAsync(sql);
        }
        
        private IEnumerable<CityType> CreateCityTypes()
        {
            var data = new CityType[]
            {
                new() { Name = "MegaCity", CitiziensCount = 10000000 },
                new() { Name = "BigCity", CitiziensCount = 1000000 },
                new() { Name = "City", CitiziensCount = 500000 },
                new() { Name = "Town", CitiziensCount = 20000 },
                new() { Name = "Village", CitiziensCount = 5000 }
            };
    
            return data;
        }

        private IEnumerable<User> CreateUsers()
        {
            var data = new User[]
            {
                new() { Name = "Daniel", Surname = "Filip" },
                new() { Name = "Petra", Surname = "Zlamana" },
                new() { Name = "Josef", Surname = "Novak" },
            };
            return data;
        }

        private async Task SeedAddressesAsync()
        {
            var cityTypes = CreateCityTypes().ToArray();
            var users = CreateUsers().ToArray();
            
            const string filePath = "./Persistence/SeederData/Addresses.json";
            var json = await File.ReadAllTextAsync(filePath);
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
            var rnd = new Random();
            var rndCityTypes = new Random(DateTime.Now.Millisecond);
            var rndUsers = new Random(DateTime.Now.Millisecond);

            if (data != null)
            {
                foreach (var addr in (IEnumerable<dynamic>)data.Addresses)
                {
                    var ent = new Address { City = addr.OBEC, Street = addr.ULICE, Zip = addr.PSC, 
                        Number = rnd.Next(1, 200),
                        CityType = cityTypes[rndCityTypes.Next(0, cityTypes.Length)],
                        CreateByUser = users[rndUsers.Next(0, users.Length)]
                    };
                    _dbContext.Addresses.Add(ent);
                }

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}