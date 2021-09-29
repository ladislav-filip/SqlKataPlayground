#region Info

// FileName:    AddressView.cs
// Author:      Ladislav Filip
// Created:     29.09.2021

#endregion

namespace SqlKataMySql.Domains
{
    public class AddressView
    {
        public int AddressId { get; set; }
        public string City { get; set; }

        public string Street { get; set; }
        
        public string CityTypeName { get; set; }
        
        public int CitiziensCount { get; set; }
    }
}