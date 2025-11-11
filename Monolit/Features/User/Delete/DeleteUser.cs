using FastEndpoints;
using Monolit.Helpers.Repositories.Interfaces;

namespace Monolit.Features.User.Delete
{
    public record DeleteUserRequest(Guid Id);

    public class DeleteUser : Endpoint<DeleteUserRequest>
    {
        private readonly IUserRepository _userRepository;

        public DeleteUser(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public override void Configure()
        {
            Delete("/api/user/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(DeleteUserRequest req, CancellationToken ct)
        {
            var result = await _userRepository.DeleteUserAsync(req.Id, ct);
            if (!result)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            await Send.OkAsync(ct);
        }
    }
}
