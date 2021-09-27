#region Info
// FileName:    Address.cs
// Author:      Ladislav Filip
// Created:     27.09.2021
#endregion

namespace SqlKataMySql.Domains
{
    public class Address
    {
        public int AddressId { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public string Zip { get; set; }

        public int Type { get; set; }
    }
}