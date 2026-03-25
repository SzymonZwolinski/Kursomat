namespace Modular.Modules.Users.Entities
{
    public class User
    {
        public Guid Id { get; set; } = default!;
        public string Login { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
    }
}