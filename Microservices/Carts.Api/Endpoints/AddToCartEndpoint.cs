using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Carts.Api.Data;
using Carts.Api.Entities;

namespace Carts.Api.Endpoints;

public record AddToCartRequest(Guid UserId, Guid CourseId, decimal Price);
public record AddToCartResponse(Guid CartId);

[ApiController]
[AllowAnonymous]
public class AddToCartEndpoint : ControllerBase
{
    private readonly CartsDbContext _dbContext;

    public AddToCartEndpoint(CartsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpPost("/api/carts/{UserId}/items")]
    public async Task<IActionResult> HandleAsync([FromRoute] Guid UserId, [FromBody] AddToCartRequest req, CancellationToken ct)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == UserId, ct);

        if (cart is null)
        {
            cart = new Cart { UserId = UserId };
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
        return Ok(new AddToCartResponse(cart.UserId));
    }
}
