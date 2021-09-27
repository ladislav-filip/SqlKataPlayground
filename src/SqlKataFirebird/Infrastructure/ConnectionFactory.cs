#region Info
// FileName:    ConnectionFactory.cs
// Author:      Ladislav Filip
// Created:     26.12.2020
#endregion

using System.Data.Common;
using FirebirdSql.Data.FirebirdClient;
using Microsoft.Extensions.Options;

namespace SqlKataFirebird.Infrastructure
{
    public sealed class ConnectionFactory : IConnectionFactory
    {
        private readonly FbConnection _connection = new();

        public ConnectionFactory(IOptions<ConnectionStringSettings> connString)
        {
            _connection.ConnectionString = connString.Value.DefaultConnection;
        }

        public DbConnection Connection => _connection;
    }
}