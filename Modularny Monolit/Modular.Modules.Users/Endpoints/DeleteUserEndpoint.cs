using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Users.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modular.Modules.Users.Endpoints
{
    public class DeleteUserRequest
    {
        public Guid UserId { get; set; } = default!;
    }

    internal class DeleteUserEndpoint : Endpoint<DeleteUserRequest>
    {
        private readonly UsersDbContext _context;

        public DeleteUserEndpoint(UsersDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Delete("/api/users/{UserId}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(DeleteUserRequest req, CancellationToken ct)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == req.UserId, ct);

            if (user == null)
            {
                await Send.NotFoundAsync(cancellation: ct);
                return;
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync(ct);

            await Send.OkAsync(cancellation: ct);
        }
    }
}
