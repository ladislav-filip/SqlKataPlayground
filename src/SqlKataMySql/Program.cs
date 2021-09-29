using System;
using System.Data.Common;
using System.Threading.Tasks;
using BenchmarkDotNet.Running;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MySqlConnector;
using SqlKataMySql.Extensions;
using SqlKataMySql.Infrastructure;
using SqlKataMySql.Persistence;
using SqlKataMySql.Samples;

namespace SqlKataMySql
{
    class Program
    {
        public const string ConnectionString = "server=localhost;port=3307;user=root;password=tukan;database=kata";
        
        public Program(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        static async Task Main(string[] args)
        {
            await Task.CompletedTask;
            
#if DEBUG
            // Samples
            var host = CreateHostBuilder(args).Build();
            await host.RunConsoleAsync();
#else
            // Benchmark
            var summary = BenchmarkRunner.Run<Benchmarks>();            
#endif
            
            Console.WriteLine("Finnish.");
        }
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Program>(); });

        private IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 25));
            
            services.AddDbContext<KataDbContext>(
                dbContextOptions => dbContextOptions
                    .UseMySql(ConnectionString, serverVersion)
                    .EnableSensitiveDataLogging() // <-- These two calls are optional but help
                    .EnableDetailedErrors()       // <-- with debugging (remove for production).
            );

            services.AddTransient<DbConnection>((serviceProvider) =>
            {
                var connection = new MySqlConnection(ConnectionString);
                return connection;
            });
            services.AddTransient<ICustomQueryFactory, CustomQueryFactory>();
            services.AddTransient<Seeder>();

            services.Scan(scan => scan.FromAssemblyOf<Program>()
                .AddClasses(classes => classes.InNamespaceOf<QueryBuildBase>())
                );
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }
}