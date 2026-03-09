using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Carts.Data;
using Modular.Shared.Events;

namespace Modular.Modules.Carts.EventHandlers
{
    public class OrderCompletedCartCleaner : IDomainEventHandler<OrderCompletedEvent>
    {
        private readonly CartsDbContext _context;

        public OrderCompletedCartCleaner(CartsDbContext context)
        {
            _context = context;
        }

        public async Task HandleAsync(OrderCompletedEvent domainEvent, CancellationToken ct = default)
        {
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == domainEvent.UserId, ct);

            if (cart != null && cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync(ct);
            }
        }
    }
}
