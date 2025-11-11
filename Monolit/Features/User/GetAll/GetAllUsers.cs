using FastEndpoints;
using Monolit.Features.User.Get;
using Monolit.Helpers.Repositories.Interfaces;

namespace Monolit.Features.User.GetAll
{
    public record GetAllUsersResponse(IEnumerable<GetUserResponse> Users);

    public class GetAllUsers : EndpointWithoutRequest<GetAllUsersResponse>
    {
        private readonly IUserRepository _userRepository;

        public GetAllUsers(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public override void Configure()
        {
            Get("/api/user");
            AllowAnonymous();
        }

        public override async Task HandleAsync(CancellationToken ct)
        {
            var users = await _userRepository.GetAllUsersAsync(ct);
            var response = new GetAllUsersResponse(users.Select(x => new GetUserResponse(x.Id, x.Login, x.Email)));
            await Send.OkAsync(response, ct);
        }
    }
}
