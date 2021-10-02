using System;
using System.Collections.Generic;
using System.Diagnostics;
using QueryFilterMongo.Persistence;

namespace QueryFilterMongo.Samples
{
    public abstract class QueryBuildBase
    {
        protected readonly MongoContext _context;

        protected QueryBuildBase(MongoContext context)
        {
            _context = context;
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