using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Modular.Modules.Sales.Data
{
    public class SalesDbContext : DbContext
    {
        public DbSet<Entities.Order> Orders { get; set; }

        public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("sales");
            base.OnModelCreating(modelBuilder);
        }
    }
}