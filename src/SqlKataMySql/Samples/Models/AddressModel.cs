#region Info

// FileName:    AddressModel.cs
// Author:      Ladislav Filip
// Created:     27.09.2021

#endregion

using System;
using SqlKataMySql.Infrastructure;

namespace SqlKataMySql.Samples.Models
{
    public class AddressModel
    {
        public string City { get; set; }

        public string Street { get; set; }

        [Field("CityTypes.Name")]
        public string Name { get; set; }
        
        public int CitiziensCount { get; set; }

        public DateTime DateCreated { get; set; }
    }
}