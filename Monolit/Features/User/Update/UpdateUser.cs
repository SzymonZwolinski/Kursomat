using FastEndpoints;
using Monolit.Helpers.Repositories.Interfaces;

namespace Monolit.Features.User.Update
{
    public record UpdateUserRequest(string Login, string Email, string? Password);

    public class UpdateUser : Endpoint<UpdateUserRequest>
    {
        private readonly IUserRepository _userRepository;

        public UpdateUser(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public override void Configure()
        {
            Put("/api/user/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(UpdateUserRequest req, CancellationToken ct)
        {
            var id = Route<Guid>("id");
            var user = new Entities.User(req.Login, req.Password, req.Email)
            {
                Id = id
            };

            var result = await _userRepository.UpdateUserAsync(user, ct);
            if (!result)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            await Send.OkAsync(ct);
        }
    }
}
