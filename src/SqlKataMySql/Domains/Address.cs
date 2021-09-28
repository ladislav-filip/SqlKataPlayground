#region Info
// FileName:    Address.cs
// Author:      Ladislav Filip
// Created:     27.09.2021
#endregion

using System.ComponentModel.DataAnnotations.Schema;

namespace SqlKataMySql.Domains
{
    [Table("Addresses")]
    public class Address
    {
        public int AddressId { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public string Zip { get; set; }

        public int Number { get; set; }

        public int CityTypeId { get; set; }

        public CityType CityType { get; set; }

        public int CreateByUserId { get; set; }

        public User CreateByUser { get; set; }
    }
}