using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Modular.Modules.Carts.Contracts
{
    public class CartDetailsDto
    {
        public Guid UserId { get; set; }
        public IEnumerable<Guid> CourseIds { get; set; } = new List<Guid>();
    }

    public interface ICartsApi
    {
        Task<CartDetailsDto?> GetCartAsync(Guid userId, CancellationToken ct = default);
    }
}
