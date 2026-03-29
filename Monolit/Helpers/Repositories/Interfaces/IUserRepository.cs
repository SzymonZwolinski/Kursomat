using Monolit.Entities;

namespace Monolit.Helpers.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<Guid> CreateAccountAsync(User account, CancellationToken ct);
        Task<User?> GetUserByIdAsync(Guid id, CancellationToken ct);
        Task<User?> GetUserByLoginAsync(string login, CancellationToken ct);
        Task<IEnumerable<User>> GetAllUsersAsync(CancellationToken ct);
        Task<bool> UpdateUserAsync(User user, CancellationToken ct);
        Task<bool> DeleteUserAsync(Guid id, CancellationToken ct);
    }
}
