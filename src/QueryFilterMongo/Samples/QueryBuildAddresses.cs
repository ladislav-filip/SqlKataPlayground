using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Extensions.Primitives;
using MongoDB.Driver;
using QueryFilterMongo.Domains;
using QueryFilterMongo.Persistence;
using QueryFilterMongo.Persistence.QueryFilterNs;

namespace QueryFilterMongo.Samples
{
    public class QueryBuildAddresses : QueryBuildBase
    {
        public QueryBuildAddresses(MongoContext context) : base(context)
        {
        }
        
        public async Task GetByQuerySamplesFilter()
        {
            // https://helpdesk.fullsys.cz/dokuwiki/skoda_vyvoj/ppsnet/programovani_postupy_a_rady/predavani_datoveho_filtru_v_url_query_stringu?s[]=url&s[]=filtr#operatory
            
            await GetByQueryFilter(CreateFilter("?limit=3&city=cont:os&sort=city"));                 // LIKE
            // await GetByQueryFilter(CreateFilter("?limit=3&city=cont:os&sort=city&number=gt:50"));    // LIKE and GREATER
            // await GetByQueryFilter(CreateFilter("?limit=3&offset=5&city=cont:os&sort=city"));        // OFFSET
            // await GetByQueryFilter(CreateFilter("?limit=3&city=cont:os&sort=city&number=neq:100"));  // LIKE and NOT EQUAL
            
            // await GetByQueryFilter(CreateFilter("?limit=3&city=in:Kladno,Jicin&sort=city"));         // IN clausule
            // await GetByQueryFilter(CreateFilter("?sort=city&number=in:10,20,30"));                   // IN clausule
            // await GetByQueryFilter(CreateFilter("?limit=2&sort=city&number=10,20,30"));              // IN clausule pomocí "="
            // await GetByQueryFilter(CreateFilter("?limit=2&sort=city&number=nin:10,20,30"));          // NOT IN clausule
            // await GetByQueryFilter(CreateFilter("?limit=3&city=cont:os&sort=city:DESC,number"));     // ORDER BY multiple
            //
            // var dt = DateTime.Today.AddDays(-5).ToString("yyyy-MM-ddTHH:mm:ss");
            // var url = $"?limit=3&datecreated=gte:{dt}&sort=datecreated:desc";
            // await GetByQueryFilter(CreateFilter(url));
            // url = $"?limit=3&datecreated=lte:{dt}&sort=datecreated:desc";
            // await GetByQueryFilter(CreateFilter(url));
            
            // await GetByQueryFilter(CreateFilter("?limit=3&datecreated=prevweek&sort=city"));        // PREVWEEK

            // IS NULL nefunguje na DateTime
            // GetByQueryFilter(CreateFilter("?limit=3&offset=5&datecreated=nill&sort=city"));
            
            // IS NULL nefunguje na number
            // GetByQueryFilter(CreateFilter("?limit=3&offset=5&CitiziensCount=nill&sort=city"));
            
            // IS NULL string nefunguje
            // await GetByQueryFilter(CreateFilter("?limit=3&offset=5&city=nill&sort=city"));              
            // IS NOT NULL string nefunguje
            // GetByQueryFilter(CreateFilter("?limit=3&offset=5&city=nnill&sort=city"));                   
            
            
        }
        
        public async Task GetByQueryFilter(IDictionary<string, StringValues> filter)
        {
            var allowFields = new Dictionary<string, Type>
            {
                { nameof(Address.City), typeof(string) },
                { nameof(Address.Street), typeof(string) },
                { nameof(Address.Number), typeof(int) },
                { nameof(Address.DateCreated), typeof(DateTime)}
            };
            
            var queryParams = new UrlFilterParserDynamic<Address>(allowFields).Parse(filter);
            var totalCount = await _context.Addresses.CountDocumentsAsync(queryParams.Filter);
            var dataQuery = await _context.Addresses.FindAsync(queryParams.Filter, queryParams.FindOptions);
            var data = await dataQuery.ToListAsync();

            Print(data);
#if DEBUG
            Console.WriteLine($"Total count is {totalCount}");
#endif            
        }
        
        private static IDictionary<string, StringValues> CreateFilter(string url)
        {
#if DEBUG
            Console.WriteLine(url);
#endif
            var urlQuery = HttpUtility.ParseQueryString(url);
            IDictionary<string, StringValues> filter = new Dictionary<string, StringValues>();
            var preFilter = urlQuery.AllKeys.ToDictionary(k => k, k => urlQuery[k]);
            foreach (var (key, value) in preFilter)
            {
                // var values = value.Split(',');
                filter.Add(key, new []{value});
            }

            return filter;
        }
    }
}