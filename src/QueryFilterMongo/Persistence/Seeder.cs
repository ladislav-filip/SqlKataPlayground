using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using Newtonsoft.Json.Converters;
using QueryFilterMongo.Domains;

namespace QueryFilterMongo.Persistence
{
    public class Seeder
    {
        private readonly MongoContext _context;

        public Seeder(MongoContext context)
        {
            _context = context;
        }

        public void Seed()
        {
            Console.WriteLine("Start seed...");
            SeedAddresses();
            Console.WriteLine("Seed successfuly.");
        }

        private void SeedAddresses()
        {
            const string filePath = "./Persistence/SeederData/Addresses.json";
            var json = File.ReadAllText(filePath);
            dynamic dataTmp = Newtonsoft.Json.JsonConvert.DeserializeObject<ExpandoObject>(json, new ExpandoObjectConverter());
            var rnd = new Random();
            
            if (dataTmp != null)
            {
                foreach (var addr in (IEnumerable<dynamic>)dataTmp.Addresses)
                {
                    var ent = new Address {
                        AddressId = Guid.NewGuid().ToString(),
                        City = addr.OBEC, Street = addr.ULICE, Zip = addr.PSC, 
                        Number = rnd.Next(1, 200),
                        // CityType = cityTypes[rndCityTypes.Next(0, cityTypes.Length)],
                        // CreateByUser = users[rndUsers.Next(0, users.Length)],
                        DateCreated = DateTime.Now.AddDays(0 - rnd.Next(1, 200))
                    };
                    _context.Addresses.InsertOne(ent);
                }
            }
        }
    }
}