using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Sales.Data;

namespace Modular.Modules.Sales
{
    public static class Extensions
    {
        public static IServiceCollection AddSalesModule(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<SalesDbContext>(options => options.UseSqlServer(connectionString));
            return services;
        }
    }
}