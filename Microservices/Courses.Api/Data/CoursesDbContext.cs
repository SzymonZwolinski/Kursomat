using Courses.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace Courses.Api.Data;

public class CoursesDbContext : DbContext
{
    public DbSet<Course> Courses { get; set; }
    public DbSet<UserCourse> UserCourses { get; set; }

    public CoursesDbContext(DbContextOptions<CoursesDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("courses");

        modelBuilder.Entity<Course>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
        });

        modelBuilder.Entity<UserCourse>(entity =>
        {
            entity.HasKey(e => new { e.UserId, e.CourseId });
            entity.HasOne(e => e.Course)
                  .WithMany(c => c.UserCourses)
                  .HasForeignKey(e => e.CourseId);
        });
    }
}
