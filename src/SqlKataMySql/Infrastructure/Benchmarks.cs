#region Info
// FileName:    Benchmarks.cs
// Author:      Ladislav Filip
// Created:     28.09.2021
#endregion

using System;
using BenchmarkDotNet.Attributes;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using SqlKataMySql.Persistence;
using SqlKataMySql.Samples;

namespace SqlKataMySql.Infrastructure
{
    public class Benchmarks
    {
        private QueryBuildAddresses _queryBuildAddresses;
        private EfBuildAddress _efBuildAddress;
        private QueryBuildAddressesWithCityType _queryBuildAddressesWithCityType;
        private DapperBuildAddress _dapperBuildAddress;

        private const int MaxLoop = 30;

        public Benchmarks()
        {
            var connection = new MySqlConnection(Program.ConnectionString);
            _queryBuildAddresses = new QueryBuildAddresses(new CustomQueryFactory(connection));
            _queryBuildAddressesWithCityType = new QueryBuildAddressesWithCityType(new CustomQueryFactory(connection));

            _dapperBuildAddress = new DapperBuildAddress(connection);

            var serverVersion = new MySqlServerVersion(new Version(8, 0, 25));
            var optionsBuilder = new DbContextOptionsBuilder<KataDbContext>();
            optionsBuilder.UseMySql(Program.ConnectionString, serverVersion)
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors();
            
            var kataContext = new KataDbContext(optionsBuilder.Options);
            _efBuildAddress = new EfBuildAddress(kataContext);
            
            Console.WriteLine("Initialize benchmark...");
        }

        [Benchmark]
        public void ByCityContains()
        {
            for(var i=0; i<MaxLoop; i++) _queryBuildAddresses.ByCityContains("os");
        }
        
        [Benchmark]
        public void EfByCityContains()
        {
            for(var i=0; i<MaxLoop; i++) _efBuildAddress.ByCityContains("os");
        }
        
        [Benchmark]
        public void DapperByCityContains()
        {
            for(var i=0; i<MaxLoop; i++) _dapperBuildAddress.ByCityContains("os");
        }

        [Benchmark]
        public void GetJoined()
        {
            for(var i=0; i<MaxLoop; i++) _queryBuildAddressesWithCityType.GetJoined();
        }
        
        [Benchmark]
        public void EfGetJoined()
        {
            for(var i=0; i<MaxLoop; i++) _efBuildAddress.GetJoined();
        }
        
        [Benchmark]
        public void DapperGetJoined()
        {
            for(var i=0; i<MaxLoop; i++) _dapperBuildAddress.GetJoined();
        }
    }
}