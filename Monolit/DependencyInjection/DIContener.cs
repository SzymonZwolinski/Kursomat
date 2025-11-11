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
		}

		private static void RegisterRepositories(IServiceCollection services)
		{
			services.AddScoped<IUserRepository, UserRepository>();
		}

		private static void RegisterEtc(IServiceCollection services)
		{
			services.AddScoped<IPasswordHasher, PasswordHasher>();
		}
	}
}
