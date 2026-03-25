using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Carts.Api.Data;

namespace Carts.Api.Endpoints;

public record RemoveFromCartRequest(Guid UserId, Guid CourseId);

[ApiController]
[AllowAnonymous]
public class RemoveFromCartEndpoint : ControllerBase
{
    private readonly CartsDbContext _dbContext;

    public RemoveFromCartEndpoint(CartsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpDelete("/api/carts/{UserId}/items/{CourseId}")]
    public async Task<IActionResult> HandleAsync([FromRoute] RemoveFromCartRequest req, CancellationToken ct)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == req.UserId, ct);

        if (cart is null)
        {
            return NotFound();
        }

        var item = cart.Items.FirstOrDefault(i => i.CourseId == req.CourseId);
        if (item is not null)
        {
            cart.Items.Remove(item);
            await _dbContext.SaveChangesAsync(ct);
        }

        return NoContent();
    }
}
