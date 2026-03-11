using Microsoft.EntityFrameworkCore;
using Microservices.Sales.Api.Entities;

namespace Microservices.Sales.Api.Data
{
    public class SalesDbContext : DbContext
    {
        public DbSet<Order> Orders { get; set; }

        public SalesDbContext(DbContextOptions<SalesDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("sales");
            base.OnModelCreating(modelBuilder);
        }
    }
}
