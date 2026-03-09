using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Users.Data;
using Modular.Modules.Users.Helpers.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Modular.Modules.Users.Endpoints
{
    public class UpdateUserRequest
    {
        public Guid UserId { get; set; } = default!;
        public string Login { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? Password { get; set; } = default!;
    }

    internal class UpdateUserEndpoint : Endpoint<UpdateUserRequest>
    {
        private readonly UsersDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public UpdateUserEndpoint(UsersDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public override void Configure()
        {
            Put("/api/users/{UserId}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == req.UserId, ct);

            if (user == null)
            {
                await Send.NotFoundAsync(cancellation: ct);
                return;
            }

            user.Login = req.Login;
            user.Email = req.Email;

            if (!string.IsNullOrEmpty(req.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(req.Password);
            }

            await _context.SaveChangesAsync(ct);
            await Send.OkAsync(cancellation: ct);
        }
    }
}
