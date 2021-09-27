using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlKataMySql.Extensions;
using SqlKataMySql.Persistence;

namespace SqlKataMySql
{
    class Program
    {
        public Program(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();
            await host.RunConsoleAsync();
            Console.WriteLine("Finnish.");
        }
        
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder => { webBuilder.UseStartup<Program>(); });

        private IConfiguration Configuration { get; }
        
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = "server=localhost;port=3307;user=root;password=tukan;database=kata";
            var serverVersion = new MySqlServerVersion(new Version(8, 0, 25));
            
            services.AddDbContext<KataDbContext>(
                dbContextOptions => dbContextOptions
                    .UseMySql(connectionString, serverVersion)
                    .EnableSensitiveDataLogging() // <-- These two calls are optional but help
                    .EnableDetailedErrors()       // <-- with debugging (remove for production).
            );

            services.AddTransient<Seeder>();

            // services.AddSingleton<IConnectionFactory, ConnectionFactory>();
            // services.AddSingleton<SampleExecutor>();
        }
        
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
        }
    }
}