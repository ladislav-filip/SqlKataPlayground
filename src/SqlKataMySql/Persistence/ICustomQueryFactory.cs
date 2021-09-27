#region Info

// FileName:    ICustomQueryFactory.cs
// Author:      Ladislav Filip
// Created:     27.09.2021

#endregion

using SqlKata.Execution;

namespace SqlKataMySql.Persistence
{
    public interface ICustomQueryFactory
    {
        QueryFactory Query();
    }
}