using FastEndpoints;
using Modular.Modules.Users.Data;
using Modular.Modules.Users.Entities;

namespace Modular.Modules.Users.Endpoints
{
    public class RegisterUserRequest
    {
        public string Login { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
    }

    internal class RegisterUserEndpoint : Endpoint<RegisterUserRequest>
    {
        private readonly UsersDbContext _context;

        public RegisterUserEndpoint(UsersDbContext context)
        {
            _context = context;
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
                PasswordHash = req.Password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(ct);

            await SendOkAsync(ct);
        }
    }
}