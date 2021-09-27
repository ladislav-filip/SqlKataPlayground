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

namespace SqlKataMySql.Extensions
{
    public static class HostExtensions
    {
        public static async Task<IHost> RunConsoleAsync(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var seeder = scope.ServiceProvider.GetRequiredService<Seeder>();
            await seeder.SeedAsync();
            
            // var executor = scope.ServiceProvider.GetRequiredService<SampleExecutor>();
            // executor.Run();

            Console.WriteLine("Console run...");
            Console.WriteLine("Press Enter...");
            var input = Console.ReadLine();

            if (input == "stop")
            {
                Console.WriteLine("Stopped");
                Environment.ExitCode = 5000;
                return host;
            }


            Console.WriteLine("Continue...");

            // zatím web nebudu zkoušet...
            // host.Run();

            return host;
        }
    }
}