#region Info
// FileName:    EfBuildAddress.cs
// Author:      Ladislav Filip
// Created:     28.09.2021
#endregion

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
            var data = _dbContext.Addresses.Where(p => p.City.Contains(searchCity)).AsNoTracking().ToArray();
        }

        public void GetJoined()
        {
            var data = _dbContext.Addresses.Include(i => i.CityType).Include(i => i.CreateByUser).AsNoTracking()
                .ToArray();
        }
    }
}