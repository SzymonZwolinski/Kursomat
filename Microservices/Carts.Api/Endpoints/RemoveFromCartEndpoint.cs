using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Carts.Api.Data;

namespace Carts.Api.Endpoints;

public record RemoveFromCartRequest(Guid UserId, Guid CourseId);

public class RemoveFromCartEndpoint : Endpoint<RemoveFromCartRequest>
{
    private readonly CartsDbContext _dbContext;

    public RemoveFromCartEndpoint(CartsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public override void Configure()
    {
        Delete("/api/carts/{UserId}/items/{CourseId}");
        AllowAnonymous();
    }

    public override async Task HandleAsync(RemoveFromCartRequest req, CancellationToken ct)
    {
        var cart = await _dbContext.Carts
            .Include(c => c.Items)
            .FirstOrDefaultAsync(c => c.UserId == req.UserId, ct);

        if (cart is null)
        {
            await HttpContext.Response.SendNotFoundAsync(cancellation: ct);
            return;
        }

        var item = cart.Items.FirstOrDefault(i => i.CourseId == req.CourseId);
        if (item is not null)
        {
            cart.Items.Remove(item);
            await _dbContext.SaveChangesAsync(ct);
        }

        await HttpContext.Response.SendNoContentAsync(cancellation: ct);
    }
}
