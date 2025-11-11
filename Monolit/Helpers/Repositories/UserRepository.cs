using Monolit.DataBase;
using Monolit.Entities;
using Monolit.Helpers.Repositories.Interfaces;

namespace Monolit.Helpers.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly MonolitDbContext _dbContext;

		public UserRepository(MonolitDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public async Task<Guid> CreateAccountAsync(User user, CancellationToken ct)
        {
            _dbContext.Add(user);
            await _dbContext.SaveChangesAsync(ct);
			return user.Id;
		}
    }
}
