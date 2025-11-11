using FastEndpoints;
using Monolit.Helpers.Repositories.Interfaces;

namespace Monolit.Features.User.Get
{
    public record GetUserRequest(Guid Id);
    public record GetUserResponse(Guid Id, string Login, string Email);

    public class GetUser : Endpoint<GetUserRequest, GetUserResponse>
    {
        private readonly IUserRepository _userRepository;

        public GetUser(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public override void Configure()
        {
            Get("/api/user/{id}");
            AllowAnonymous();
        }

        public override async Task HandleAsync(GetUserRequest req, CancellationToken ct)
        {
            var user = await _userRepository.GetUserByIdAsync(req.Id, ct);
            if (user is null)
            {
                await Send.NotFoundAsync(ct);
                return;
            }

            var response = new GetUserResponse(user.Id, user.Login, user.Email);
            await Send.OkAsync(response, ct);
        }
    }
}
