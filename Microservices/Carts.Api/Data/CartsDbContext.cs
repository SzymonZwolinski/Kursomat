using Carts.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Carts.Api.Data;

public class CartsDbContext : DbContext
{
    public DbSet<Cart> Carts { get; set; }
    public DbSet<CartItem> CartItems { get; set; }

    public CartsDbContext(DbContextOptions<CartsDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("carts");

        modelBuilder.Entity<Cart>(entity =>
        {
            entity.HasKey(e => e.UserId);
        });

        modelBuilder.Entity<CartItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasOne(e => e.Cart)
                  .WithMany(c => c.Items)
                  .HasForeignKey(e => e.CartId);
        });
    }
}
