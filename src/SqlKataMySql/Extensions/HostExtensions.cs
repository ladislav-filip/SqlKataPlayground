#region Info

// FileName:    HostExtensions.cs
// Author:      Ladislav Filip
// Created:     26.09.2021

#endregion

using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SqlKataMySql.Persistence;
using SqlKataMySql.Samples;

namespace SqlKataMySql.Extensions
{
    public static class HostExtensions
    {
        public static async Task<IHost> RunConsoleAsync(this IHost host)
        {
            await Task.CompletedTask;
            using var scope = host.Services.CreateScope();

            var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
            //await seeder.SeedAsync();

            var qb = scope.ServiceProvider.GetRequiredService<QueryBuildAddresses>();
            qb.LimitFive();
            qb.LimitFiveByCity("Praha");
            qb.LimitFiveByCityContains("os");
            qb.LimitFiveByNumber(50);
            
            // var executor = scope.ServiceProvider.GetRequiredService<SampleExecutor>();
            // executor.Run();

            Console.WriteLine("Console run...");
            
            // zatím web nebudu zkoušet...
            // host.Run();

            return host;
        }
    }
}