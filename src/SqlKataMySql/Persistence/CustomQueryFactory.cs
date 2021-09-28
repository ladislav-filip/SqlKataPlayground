#region Info
// FileName:    CustomQueryFactory.cs
// Author:      Ladislav Filip
// Created:     27.09.2021
#endregion

using System;
using System.Data.Common;
using System.Diagnostics;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace SqlKataMySql.Persistence
{
    public class CustomQueryFactory : ICustomQueryFactory
    {
        private readonly QueryFactory _queryFactory;
        
        public CustomQueryFactory(DbConnection connection)
        {
            _queryFactory = new QueryFactory(connection, new MySqlCompiler());
            EnableLog();
        }
        
        public QueryFactory Query()
        {
            return _queryFactory;
        }

        [Conditional("DEBUG")]
        private void EnableLog()
        {
            _queryFactory.Logger = compiled => { Console.WriteLine(compiled.ToString()); };
        }
    }
}