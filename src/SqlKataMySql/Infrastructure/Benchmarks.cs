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
    [MinColumn, MaxColumn]
    public class Benchmarks
    {
        private QueryBuildAddresses _queryBuildAddresses;
        private EfBuildAddress _efBuildAddress;
        private QueryBuildAddressesWithCityType _queryBuildAddressesWithCityType;
        private DapperBuildAddress _dapperBuildAddress;

        private const int MaxLoop = 20;

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

        private readonly string[] _contains = { "os", "Nad", "er", "Bratislava", "Praha", "Brno", "Vary", "men", "Kladno", "os", "Nad", "er", "Bratislava", "Praha", "Brno", "Vary", "men", "Kladno" };
        
        [Benchmark]
        public void ByCityContains()
        {
            foreach (var contain in _contains)
            {
                _queryBuildAddresses.ByCityContains(contain);                
            }
        }
        
        [Benchmark]
        public void EfByCityContains()
        {
            foreach (var contain in _contains)
            {
                _efBuildAddress.ByCityContains(contain);                
            }
        }
        
        [Benchmark]
        public void DapperByCityContains()
        {
            foreach (var contain in _contains)
            {
                _dapperBuildAddress.ByCityContains(contain);                
            }
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
        public void EfGetJoinedByView()
        {
            for(var i=0; i<MaxLoop; i++) _efBuildAddress.GetFromView();
        }
        
        [Benchmark]
        public void DapperGetJoined()
        {
            for(var i=0; i<MaxLoop; i++) _dapperBuildAddress.GetJoined();
        }
    }
}