using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Users.Data;
using Modular.Modules.Users.Helpers;
using Modular.Modules.Users.Helpers.Interfaces;

namespace Modular.Modules.Users
{
    public static class Extensions
    {
        public static IServiceCollection AddUsersModule(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<UsersDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            return services;
        }
    }
}
