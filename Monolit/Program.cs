
using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;
using Monolit.DependencyInjection;

namespace Monolit
{
	public class Program
	{
		public static void Main(string[] args)
		{
			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();
			builder.Services.AddSwaggerGen();

			builder.Services.AddFastEndpoints();
			var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

			builder.Services.AddDbContext<MonolitDbContext>(options =>
				options.UseSqlServer(connectionString));

			DIContener.RegisterServices(builder.Services);

			var app = builder.Build();

			// Configure the HTTP request pipeline.

			app.UseSwagger();
			app.UseSwaggerUI();
			app.UseFastEndpoints();

			app.UseHttpsRedirection();

			app.UseAuthorization();

			app.MapControllers();

			app.Run();
		}
	}
}
