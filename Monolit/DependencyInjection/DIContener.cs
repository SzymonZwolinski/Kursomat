using Microsoft.Extensions.DependencyInjection;
using Monolit.Features.Carts.EventHandlers;
using Monolit.Features.Courses.EventHandlers;
using Monolit.Features.Orders.Events;
using Monolit.Helpers.DomainEvents;
using Monolit.Helpers.Etc;
using Monolit.Helpers.Etc.Interfaces;
using Monolit.Helpers.Repositories;
using Monolit.Helpers.Repositories.Interfaces;

namespace Monolit.DependencyInjection
{
    public static class DIContener
    {
        public static void RegisterServices(IServiceCollection services)
        {
            RegisterRepositories(services);
            RegisterEtc(services);
            RegisterDomainEvents(services);
        }

        private static void RegisterRepositories(IServiceCollection services)
        {
            services.AddScoped<IUserRepository, UserRepository>();
        }

        private static void RegisterEtc(IServiceCollection services)
        {
            services.AddScoped<IPasswordHasher, PasswordHasher>();
        }

        private static void RegisterDomainEvents(IServiceCollection services)
        {
            services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
            services.AddScoped<IDomainEventHandler<OrderCompletedEvent>, OrderCompletedCartCleaner>();
            services.AddScoped<IDomainEventHandler<OrderCompletedEvent>, OrderCompletedCourseGranter>();
        }
    }
}
