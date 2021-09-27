#region Info
// FileName:    Seeder.cs
// Author:      Ladislav Filip
// Created:     27.09.2021
#endregion

using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Threading.Tasks;
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
            await SeedAddressesAsync();
            Console.WriteLine();
            Console.WriteLine();
        }

        private async Task SeedAddressesAsync()
        {
            const string filePath = "./Persistence/SeederData/Addresses.json";
            var json = await File.ReadAllTextAsync(filePath);
            dynamic data = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
            var rnd = new Random();

            if (data != null)
            {
                foreach (var addr in (IEnumerable<dynamic>)data.Addresses)
                {
                    var ent = new Address { City = addr.OBEC, Street = addr.ULICE, Zip = addr.PSC, Number = rnd.Next(1, 200)};
                    _dbContext.Addresses.Add(ent);
                }

                await _dbContext.SaveChangesAsync();
            }
        }
    }
}