#region Info
// FileName:    KataDbContext.cs
// Author:      Ladislav Filip
// Created:     27.09.2021
#endregion

using Microsoft.EntityFrameworkCore;
using SqlKataMySql.Domains;

namespace SqlKataMySql.Persistence
{
    public class KataDbContext : DbContext
    {
        private readonly DbContextOptions<KataDbContext> _options;

        public KataDbContext(DbContextOptions<KataDbContext> options) : base(options)
        {
            _options = options;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }

        public DbSet<Address> Addresses { get; set; }
    }
}