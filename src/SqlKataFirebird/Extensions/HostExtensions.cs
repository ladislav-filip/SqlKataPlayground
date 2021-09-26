#region Info

// FileName:    HostExtensions.cs
// Author:      Ladislav Filip
// Created:     26.09.2021

#endregion

using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace SqlKataFirebird.Extensions
{
    public static class HostExtensions
    {
        public static IHost RunConsole(this IHost host)
        {
            using var scope = host.Services.CreateScope();

            var conf = scope.ServiceProvider.GetRequiredService<IConfiguration>();


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