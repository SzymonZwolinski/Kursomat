using Microsoft.EntityFrameworkCore;
using Monolit.Entities;

namespace Monolit.DataBase
{
	public class MonolitDbContext : DbContext
	{
		DbSet<User> Users { get; set; }
		public DbSet<Course> Courses { get; set; }
		public DbSet<Cart> Carts { get; set; }
		public DbSet<CartItem> CartItems { get; set; }
		public DbSet<Order> Orders { get; set; }
		public DbSet<OrderItem> OrderItems { get; set; }
		public DbSet<UserCourse> UserCourses { get; set; }


		public MonolitDbContext(DbContextOptions<MonolitDbContext> options) 
			: base(options)
		{
		}
	}
}
