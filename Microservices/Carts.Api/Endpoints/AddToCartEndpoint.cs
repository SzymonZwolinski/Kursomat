using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Carts.Api.Data;
using Carts.Api.Entities;

namespace Carts.Api.Endpoints;

public record AddToCartRequest(Guid UserId, Guid CourseId, decimal Price);
public record AddToCartResponse(Guid CartId);

public class AddToCartEndpoint : Endpoint<AddToCartRequest, AddToCartResponse>
{
    private readonly CartsDbContext _dbContext;

    public AddToCartEndpoint(CartsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Post("/api/carts/{UserId}/items");
        AllowAnonymous();
    }

    public override async Task HandleAsync(AddToCartRequest req, CancellationToken ct)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == req.UserId, ct);

        if (cart is null)
        {
            cart = new Cart { UserId = req.UserId };
            _dbContext.Carts.Add(cart);
        }

        if (!cart.Items.Any(i => i.CourseId == req.CourseId))
        {
            cart.Items.Add(new CartItem
            {
                Id = Guid.NewGuid(),
                CourseId = req.CourseId,
                Price = req.Price
            });
        }

        await _dbContext.SaveChangesAsync(ct);
        await HttpContext.Response.SendAsync(new AddToCartResponse(cart.UserId), 200, cancellation: ct);
    }
}
