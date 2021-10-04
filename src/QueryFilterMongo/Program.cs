using System;
using System.Threading.Tasks;
using Fullsys.Pps.Utils.Infrastructure.Mongo.QueryFilter;
using QueryFilterMongo.Persistence;
using QueryFilterMongo.Samples;

namespace QueryFilterMongo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var context = new MongoContext();
            var seeder = new Seeder(context);
            var mongoFilterFactory = new MongoUrlFilterParserFactory();
            var builder = new QueryBuildAddresses(context, mongoFilterFactory);

            //seeder.Seed();
            context.ListCollections();
            
            await builder.GetByQuerySamplesFilter();

            // Console.WriteLine("List addresses...");
            // context.Addresses.Find(_ => true).ToList().ForEach(d =>
            // {
            //     Console.WriteLine(ObjectDumper.Dump(d));
            //     Console.WriteLine("-");
            // });
            
            
            Console.WriteLine("Finnish.");
        }
    }
}