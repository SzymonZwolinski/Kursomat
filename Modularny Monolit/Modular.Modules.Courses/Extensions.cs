using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Courses.Data;
using Modular.Modules.Courses.Contracts;
using Modular.Modules.Courses.Api;

namespace Modular.Modules.Courses
{
    public static class Extensions
    {
        public static IServiceCollection AddCoursesModule(this IServiceCollection services, string connectionString)
        {
            services.AddDbContext<CoursesDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<ICoursesApi, CoursesApi>();

            return services;
        }
    }
}