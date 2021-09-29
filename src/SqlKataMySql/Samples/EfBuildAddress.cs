#region Info
// FileName:    EfBuildAddress.cs
// Author:      Ladislav Filip
// Created:     28.09.2021
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SqlKataMySql.Persistence;

namespace SqlKataMySql.Samples
{
    public class EfBuildAddress
    {
        private readonly KataDbContext _dbContext;

        public EfBuildAddress(KataDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void ByCityContains(string searchCity)
        {
            var data = _dbContext.Addresses.Where(p => p.City.Contains(searchCity)).OrderBy(p => p.City).AsNoTracking().ToArray();
            Print(data);
        }

        public void GetJoined()
        {
            var data = _dbContext.Addresses.Include(i => i.CityType).Include(i => i.CreateByUser).OrderBy(p => p.City).AsNoTracking()
                .ToArray();
            Print(data);
        }

        public void GetFromView()
        {
            var data = _dbContext.AddressViews.OrderBy(p => p.City).ToArray();
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