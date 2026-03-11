using Microsoft.EntityFrameworkCore;
using Modular.Modules.Carts.Entities;

namespace Modular.Modules.Carts.Data
{
    public class CartsDbContext : DbContext
    {
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }

        public CartsDbContext(DbContextOptions<CartsDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("carts");
            base.OnModelCreating(modelBuilder);
        }
    }
}
