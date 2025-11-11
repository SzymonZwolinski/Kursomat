using Microsoft.EntityFrameworkCore;
using Monolit.Entities;

namespace Monolit.DataBase
{
	public class MonolitDbContext : DbContext
	{
		DbSet<User> Users { get; set; }
		public DbSet<Course> Courses { get; set; }

		public MonolitDbContext(DbContextOptions<MonolitDbContext> options) 
			: base(options)
		{
		}
	}
}
