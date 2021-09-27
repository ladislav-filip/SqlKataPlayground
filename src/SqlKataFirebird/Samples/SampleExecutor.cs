#region Info
// FileName:    SampleExecutor.cs
// Author:      Ladislav Filip
// Created:     26.09.2021
#endregion

using System;
using System.Text;
using SqlKataFirebird.Infrastructure;

namespace SqlKataFirebird.Samples
{
    public class SampleExecutor
    {
        private readonly IConnectionFactory _connectionFactory;

        public SampleExecutor(IConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public void Run()
        {
            Console.WriteLine("Execute...");
        }
    }
}