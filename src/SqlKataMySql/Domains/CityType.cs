#region Info

// FileName:    CityType.cs
// Author:      Ladislav Filip
// Created:     27.09.2021

#endregion

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace SqlKataMySql.Domains
{
    [Table("CityTypes")]
    public class CityType
    {
        public int CityTypeId { get; set; }

        public string Name { get; set; }

        public int CitiziensCount { get; set; }

        public ICollection<Address> Addresses { get; set; }
    }
}