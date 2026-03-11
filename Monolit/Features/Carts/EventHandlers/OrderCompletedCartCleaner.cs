using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;
using Monolit.Features.Orders.Events;
using Monolit.Helpers.DomainEvents;

namespace Monolit.Features.Carts.EventHandlers
{
    public class OrderCompletedCartCleaner : IDomainEventHandler<OrderCompletedEvent>
    {
        private readonly MonolitDbContext _context;

        public OrderCompletedCartCleaner(MonolitDbContext context)
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
