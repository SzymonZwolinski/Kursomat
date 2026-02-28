using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Modular.Modules.Users.Data
{
    internal class UsersDbContext : DbContext
    {
        public DbSet<Entities.User> Users { get; set; }

        public UsersDbContext(DbContextOptions<UsersDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("users");
            base.OnModelCreating(modelBuilder);
        }
    }
}