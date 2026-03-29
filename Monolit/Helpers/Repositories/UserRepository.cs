using Microsoft.EntityFrameworkCore;
﻿using Monolit.DataBase;
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

        public async Task<bool> DeleteUserAsync(Guid id, CancellationToken ct)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
            if (user is null)
            {
                return false;
            }
            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }

        public async Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken ct)
        {
            return await _dbContext.Users.ToListAsync(ct);
        }

        public async Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == id, ct);
        }

        public async Task<User?> GetUserByLoginAsync(string login, CancellationToken ct)
        {
            return await _dbContext.Users.FirstOrDefaultAsync(x => x.Login == login, ct);
        }

        public async Task<bool> UpdateUserAsync(User user, CancellationToken ct)
        {
            var userToUpdate = await _dbContext.Users.FirstOrDefaultAsync(x => x.Id == user.Id, ct);
            if(userToUpdate is null)
            {
                return false;
            }
            userToUpdate.Login = user.Login;
            userToUpdate.Email = user.Email;
            if(!string.IsNullOrEmpty(user.PasswordHash))
            {
                userToUpdate.PasswordHash = user.PasswordHash;
            }
            await _dbContext.SaveChangesAsync(ct);
            return true;
        }
    }
}
