using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Users.Data;

namespace Modular.Modules.Users
{
    public static class Extensions
    {
        public static IServiceCollection AddUsersModule(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<UsersDbContext>(options => options.UseSqlServer(connectionString));
            return services;
        }
    }
}