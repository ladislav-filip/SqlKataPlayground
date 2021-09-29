﻿#region Info
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
            GetByQueryFilter(CreateFilter("?limit=3&city=cont:os&sort=city"));
            GetByQueryFilter(CreateFilter("?limit=3&city=cont:os&sort=city&CitiziensCount=gt:10000"));
            GetByQueryFilter(CreateFilter("?limit=3&offset=5&city=cont:os&sort=city"));
            GetByQueryFilter(CreateFilter("?limit=3&city=cont:os&sort=city&CitiziensCount=neq:10000"));
            
            // chybně generuje AND místo OR
            // GetByQueryFilter(CreateFilter("?limit=3&offset=5&city=in:Kladno,Jicin&sort=city"));
            // GetByQueryFilter(CreateFilter("?sort=city&CitiziensCount=in:10000,500000"));
            
            // vygeneruje chybně: WHERE (`a`.`CitiziensCount` <> 10000) AND (`a`.`CitiziensCount` = 500000)
            // GetByQueryFilter(CreateFilter("?sort=city&CitiziensCount=nin:10000,500000"));
            
            // multisort také nefunguje
            // GetByQueryFilter(CreateFilter("?limit=3&city=cont:os&sort=city,CitiziensCount"));
        }

        private static IDictionary<string, string[]> CreateFilter(string url)
        {
            var urlQuery = HttpUtility.ParseQueryString(url);
            IDictionary<string, string[]> filter = new Dictionary<string, string[]>();
            var preFilter = urlQuery.AllKeys.ToDictionary(k => k, k => urlQuery[k]);
            foreach (var (key, value) in preFilter)
            {
                var values = value.Split(',');
                filter.Add(key, values);
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