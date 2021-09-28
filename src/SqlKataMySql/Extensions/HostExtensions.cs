#region Info

// FileName:    HostExtensions.cs
// Author:      Ladislav Filip
// Created:     26.09.2021

#endregion

using System;
using System.Threading.Tasks;
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

            var seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
            //await seeder.SeedAsync();

            SimpleAddressesRun(scope);
            SimpleAddressesWithIncludeRun(scope);


            Console.WriteLine("Console run...");

            return host;
        }
        
        private static void SimpleAddressesRun(IServiceScope scope)
        {
            var qb = scope.ServiceProvider.GetRequiredService<QueryBuildAddresses>();
            qb.LimitFive();
            qb.LimitFiveByCity("Praha");
            qb.LimitFiveByCityContains("os");
            qb.LimitFiveByNumber(50);
        }

        private static void SimpleAddressesWithIncludeRun(IServiceScope scope)
        {
            var qb = scope.ServiceProvider.GetRequiredService<QueryBuildAddressesWithCityType>();
            qb.LimitFivePluralize();
            qb.LimitFive();
            qb.LimitFiveForJosef();
            qb.LimitFiveForJosefPluralize();
        }
    }
}