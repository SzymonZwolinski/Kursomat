using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Carts.Api.Data;
using MassTransit;
using Shared.Messaging.Events;

namespace Microservices.Carts.Api.EventHandlers
{
    public class OrderCompletedCartCleaner : IConsumer<OrderCompletedEvent>
    {
        private readonly CartsDbContext _context;

        public OrderCompletedCartCleaner(CartsDbContext context)
        {
            _context = context;
        }

        public async Task Consume(ConsumeContext<OrderCompletedEvent> context)
        {
            var message = context.Message;
            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == message.UserId);

            if (cart != null && cart.Items.Any())
            {
                _context.CartItems.RemoveRange(cart.Items);
                await _context.SaveChangesAsync();
            }
        }
    }
}
