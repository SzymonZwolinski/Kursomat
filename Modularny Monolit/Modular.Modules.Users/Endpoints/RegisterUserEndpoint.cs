using FastEndpoints;
using Modular.Modules.Users.Data;
using Modular.Modules.Users.Entities;
using Modular.Modules.Users.Helpers.Interfaces;
using System.Threading;
using System.Threading.Tasks;
using System;

namespace Modular.Modules.Users.Endpoints
{
    public class RegisterUserRequest
    {
        public string Login { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string Password { get; set; } = default!;
    }

    internal class RegisterUserEndpoint : Endpoint<RegisterUserRequest>
    {
        private readonly UsersDbContext _context;
        private readonly IPasswordHasher _passwordHasher;

        public RegisterUserEndpoint(UsersDbContext context, IPasswordHasher passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public override void Configure()
        {
            Post("/api/users/register");
            AllowAnonymous();
        }

        public override async Task HandleAsync(RegisterUserRequest req, CancellationToken ct)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Login = req.Login,
                Email = req.Email,
                PasswordHash = _passwordHasher.HashPassword(req.Password)
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(ct);

            await Send.OkAsync(ct);
        }
    }
}
