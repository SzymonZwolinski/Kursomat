using FastEndpoints;
using Microsoft.EntityFrameworkCore;
using Modular.Modules.Users.Data;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Modular.Modules.Users.Endpoints
{
    internal class GetAllUsersEndpoint : EndpointWithoutRequest<List<UserDto>>
    {
        private readonly UsersDbContext _context;

        public GetAllUsersEndpoint(UsersDbContext context)
        {
            _context = context;
        }

        public override void Configure()
        {
            Get("/api/users");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var users = await _context.Users
                .AsNoTracking()
                .Select(u => new UserDto
                {
                    Id = u.Id,
                    Login = u.Login,
                    Email = u.Email
                })
                .ToListAsync(ct);

            await Send.OkAsync(users, cancellation: ct);
        }
    }
}
