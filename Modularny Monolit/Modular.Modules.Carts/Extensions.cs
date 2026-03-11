using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Carts.Data;
using Modular.Modules.Carts.Contracts;

namespace Modular.Modules.Carts
{
    public static class Extensions
    {
        public static IServiceCollection AddCartsModule(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<CartsDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<ICartsApi, CartsApi>();
            return services;
        }
    }
}
