using System;
using System.Linq;
using MongoDB.Driver;
using QueryFilterMongo.Domains;

namespace QueryFilterMongo.Persistence
{
    public class MongoContext
    {
        private readonly IMongoDatabase _database;

        private IMongoCollection<Address> _addresses;

        public MongoContext(string connString = "mongodb://root:lok@localhost:27117")
        {
            IMongoClient mongoClient = new MongoClient(connString);
            _database = mongoClient.GetDatabase("query-filter");
            
            CreateCollections();
        }

        private void CreateCollections()
        {
            var collections = _database.ListCollectionNames().ToList();
            if (collections.All(p => p != nameof(Addresses)))
            {
                Console.WriteLine("Create collection 'addresses'...");
                _database.CreateCollection(nameof(Addresses));
            }
        }

        public IMongoCollection<Address> Addresses => _addresses ??= _database.GetCollection<Address>(nameof(Addresses));

        public void ListCollections()
        {
            Console.WriteLine("List collections...");
            _database.ListCollectionNames().ToList().ForEach(Console.WriteLine);
        }
    }
}