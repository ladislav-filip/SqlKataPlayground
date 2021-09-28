#region Info

// FileName:    FieldAttribute.cs
// Author:      Ladislav Filip
// Created:     27.09.2021

#endregion

using System;

namespace SqlKataMySql.Infrastructure
{
    public class FieldAttribute : Attribute
    {
        public string FieldName { get; }

        public FieldAttribute(string fieldName)
        {
            FieldName = fieldName;
        }
    }
}