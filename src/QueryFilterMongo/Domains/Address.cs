#region Info
// FileName:    Address.cs
// Author:      Ladislav Filip
// Created:     27.09.2021
#endregion

using System;
using MongoDB.Bson.Serialization.Attributes;

namespace QueryFilterMongo.Domains
{
    public class Address
    {
        [BsonId]
        public string AddressId { get; set; }

        public string City { get; set; }

        public string Street { get; set; }

        public string Zip { get; set; }

        public int Number { get; set; }

        public int CityTypeId { get; set; }

        public int CreateByUserId { get; set; }

        public DateTime DateCreated { get; set; }
    }
}