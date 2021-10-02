using System;
using MongoDB.Driver;
using QueryFilterMongo.Persistence;

namespace QueryFilterMongo
{
    class Program
    {
        static void Main(string[] args)
        {
            var context = new MongoContext();
            var seeder = new Seeder(context);
            
            // seeder.Seed();

            context.ListCollections();
            
            Console.WriteLine("List addresses...");
            context.Addresses.Find(_ => true).ToList().ForEach(d =>
            {
                Console.WriteLine(ObjectDumper.Dump(d));
                Console.WriteLine("-");
            });
            
            
            Console.WriteLine("Finnish.");
        }
    }
}