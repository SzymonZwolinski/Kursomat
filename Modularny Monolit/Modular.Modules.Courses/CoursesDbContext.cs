using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Modular.Modules.Courses.Data
{
    internal class CoursesDbContext : DbContext
    {
        public DbSet<Entities.Course> Courses { get; set; }

        public CoursesDbContext(DbContextOptions<CoursesDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("courses");
            base.OnModelCreating(modelBuilder);
        }
    }
}