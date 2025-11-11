using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Monolit.DataBase;
using System.Security.Claims;

namespace Monolit.Features.Carts
{
    public class GetCartEndpoint : EndpointWithoutRequest<CartDto>
    {
        private readonly MonolitDbContext _context;

        public GetCartEndpoint(MonolitDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/cart");
            Claims("UserId");
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var userId = Guid.Parse(User.FindFirstValue("UserId"));

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Course)
                .Where(c => c.UserId == userId)
                .Select(c => new CartDto
                {
                    Items = c.Items.Select(i => new CartItemDto
                    {
                        CourseId = i.CourseId,
                        CourseName = i.Course.Name,
                        CoursePrice = i.Course.Price
                    }).ToList()
                })
                .FirstOrDefaultAsync(ct);

            if (cart == null)
            {
                cart = new CartDto();
            }

            await SendAsync(cart, cancellation: ct);
        }
    }
}
