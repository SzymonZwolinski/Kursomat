using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Carts.Api.Data;

namespace Carts.Api.Endpoints;

public record GetCartRequest(Guid UserId);
public record GetCartResponse(Guid UserId, List<CartItemDto> Items);
public record CartItemDto(Guid CourseId, decimal Price);

[ApiController]
[AllowAnonymous]
public class GetCartEndpoint : ControllerBase
{
    private readonly CartsDbContext _dbContext;

    public GetCartEndpoint(CartsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("/api/carts/{UserId}")]
    public async Task<IActionResult> HandleAsync([FromRoute] GetCartRequest req, CancellationToken ct)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == req.UserId, ct);

        if (cart is null)
        {
            return NotFound();
        }

        var items = cart.Items.Select(i => new CartItemDto(i.CourseId, i.Price)).ToList();
        return Ok(new GetCartResponse(cart.UserId, items));
    }
}
