#region Info

// FileName:    User.cs
// Author:      Ladislav Filip
// Created:     27.09.2021

#endregion

using System.ComponentModel.DataAnnotations.Schema;

namespace SqlKataMySql.Domains
{
    [Table("Users")]
    public class User
    {
        public int UserId { get; set; }

        public string Name { get; set; }

        public string Surname { get; set; }
    }
}