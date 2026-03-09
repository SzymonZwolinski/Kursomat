using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Users.Data;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modular.Modules.Users.Endpoints
{
    public class GetUserRequest
    {
        public Guid UserId { get; set; } = default!;
    }

    public class UserDto
    {
        public Guid Id { get; set; } = default!;
        public string Login { get; set; } = default!;
        public string Email { get; set; } = default!;
    }

    internal class GetUserEndpoint : Endpoint<GetUserRequest, UserDto>
    {
        private readonly UsersDbContext _context;

        public GetUserEndpoint(UsersDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/users/{UserId}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetUserRequest req, CancellationToken ct)
        {
            var user = await _context.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.Id == req.UserId, ct);

            if (user == null)
            {
                await Send.NotFoundAsync(cancellation: ct);
                return;
            }

            await Send.OkAsync(new UserDto
            {
                Id = user.Id,
                Login = user.Login,
                Email = user.Email
            }, cancellation: ct);
        }
    }
}
