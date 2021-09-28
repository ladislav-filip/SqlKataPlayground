#region Info
// FileName:    QueryBuildExample.cs
// Author:      Ladislav Filip
// Created:     27.09.2021
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using SqlKataMySql.Persistence;

namespace SqlKataMySql.Samples
{
    public abstract class QueryBuildBase
    {
        protected readonly ICustomQueryFactory _customQueryFactory;

        protected QueryBuildBase(ICustomQueryFactory customQueryFactory)
        {
            _customQueryFactory = customQueryFactory;
        }
        
        [Conditional("DEBUG")]
        protected void Print(IEnumerable<object> data)
        {
            foreach (var d in data)
            {
                Console.WriteLine(ObjectDumper.Dump(d));
                Console.WriteLine("-");
            }
            Console.WriteLine();
        }
    }
}