using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Carts.Data;

namespace Modular.Modules.Carts.Contracts
{
    internal class CartsApi : ICartsApi
    {
        private readonly CartsDbContext _context;

        public CartsApi(CartsDbContext context)
        {
            _context = context;
        }

        public async Task<CartDetailsDto?> GetCartAsync(Guid userId, CancellationToken ct = default)
        {
            var cart = await _context.Carts
                .AsNoTracking()
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId, ct);

            if (cart == null) return null;

            return new CartDetailsDto
            {
                UserId = cart.UserId,
                CourseIds = cart.Items.Select(i => i.CourseId).ToList()
            };
        }
    }
}
