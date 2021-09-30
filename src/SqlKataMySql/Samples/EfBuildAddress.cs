#region Info
// FileName:    EfBuildAddress.cs
// Author:      Ladislav Filip
// Created:     28.09.2021
#endregion

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Web;
using Fullsys.Pps.Utils.Infrastructure.EntityFramework.QueryFilter;
using Microsoft.EntityFrameworkCore;
using SqlKataMySql.Domains;
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

        public void GetByQuerySamplesFilter()
        {
            // https://helpdesk.fullsys.cz/dokuwiki/skoda_vyvoj/ppsnet/programovani_postupy_a_rady/predavani_datoveho_filtru_v_url_query_stringu?s[]=url&s[]=filtr#operatory
            
            // GetByQueryFilter(CreateFilter("?limit=3&city=cont:os&sort=city"));                          // LIKE
            // GetByQueryFilter(CreateFilter("?limit=3&city=cont:os&sort=city&CitiziensCount=gt:10000"));  // LIKE and GREATER
            // GetByQueryFilter(CreateFilter("?limit=3&offset=5&city=cont:os&sort=city"));                 // OFFSET
            // GetByQueryFilter(CreateFilter("?limit=3&city=cont:os&sort=city&CitiziensCount=neq:10000")); // LIKE and NOT EQUAL
            // GetByQueryFilter(CreateFilter("?limit=3&offset=5&city=nill&sort=city"));                    // IS NULL string
            // GetByQueryFilter(CreateFilter("?limit=3&offset=5&city=nnill&sort=city"));                   // IS NOT NULL string
            // GetByQueryFilter(CreateFilter("?limit=3&offset=5&city=in:Kladno,Jicin&sort=city"));         // IN clausule
            // GetByQueryFilter(CreateFilter("?sort=city&CitiziensCount=in:10000,500000"));                // IN clausule
            // GetByQueryFilter(CreateFilter("?limit=2&sort=city&CitiziensCount=10000,500000"));           // IN clausule pomocí "="
            // GetByQueryFilter(CreateFilter("?limit=2&sort=city&CitiziensCount=nin:10000,500000"));       // NOT IN clausule
            // GetByQueryFilter(CreateFilter("?limit=3&city=cont:os&sort=city:DESC,CitiziensCount"));      // ORDER BY multiple
            //
            // var dt = DateTime.Today.AddDays(-5).ToString("yyyy-MM-ddTHH:mm:ss");
            // var url = $"?limit=3&datecreated=gte:{dt}&sort=datecreated:desc";
            // GetByQueryFilter(CreateFilter(url));

            // IS NULL nefunguje na DateTime
            // GetByQueryFilter(CreateFilter("?limit=3&offset=5&datecreated=nill&sort=city"));
            
            // IS NULL nefunguje na number
            // GetByQueryFilter(CreateFilter("?limit=3&offset=5&CitiziensCount=nill&sort=city"));
            
            
        }

        private static IDictionary<string, string[]> CreateFilter(string url)
        {
            var urlQuery = HttpUtility.ParseQueryString(url);
            IDictionary<string, string[]> filter = new Dictionary<string, string[]>();
            var preFilter = urlQuery.AllKeys.ToDictionary(k => k, k => urlQuery[k]);
            foreach (var (key, value) in preFilter)
            {
                // var values = value.Split(',');
                filter.Add(key, new []{value});
            }

            return filter;
        }

        public void GetByQueryFilter(IDictionary<string, string[]> filter)
        {
            var queryAble = _dbContext.AddressViews.AsQueryable();
            var urlFilter = new UrlFilterParser<AddressView>();
            var ppsQuery = urlFilter.Parse(filter);

            var data = queryAble.Where(ppsQuery)
                .QueryExecute<AddressView, int>(ppsQuery)
                .data.ToArray();
            
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