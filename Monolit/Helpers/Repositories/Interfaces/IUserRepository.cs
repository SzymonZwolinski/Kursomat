using Monolit.Entities;

namespace Monolit.Helpers.Repositories.Interfaces
{
    public interface IUserRepository
    {
        Task<Guid> CreateAccountAsync(User account, CancellationToken ct);
    }
}
