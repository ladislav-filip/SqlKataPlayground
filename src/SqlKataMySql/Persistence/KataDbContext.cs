#region Info
// FileName:    KataDbContext.cs
// Author:      Ladislav Filip
// Created:     27.09.2021
#endregion

using System.Linq;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // https://khalidabuhakmeh.com/how-to-add-a-view-to-an-entity-framework-core-dbcontext
            // https://docs.microsoft.com/cs-cz/ef/core/modeling/keyless-entity-types?tabs=data-annotations
            modelBuilder
                .Entity<AddressView>()
                .ToView("AddressesView")
                .HasKey(k => k.AddressId);
        }

        public DbSet<Address> Addresses { get; set; }

        public DbSet<CityType> CityTypes { get; set; }

        public DbSet<User> Users { get; set; }

        public DbSet<AddressView> AddressViews { get; set; }
    }
}