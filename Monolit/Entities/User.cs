using System.ComponentModel.DataAnnotations;

namespace Monolit.Entities
{
	public class User
	{
		public Guid Id { get; set; } = default!;
		public string Login { get; set; } = default!;
		public string PasswordHash { get; set; } = default!;
		public string Email { get; set; } = default!;
		
		public User(string login, string passwordHash, string email)
		{
			Login = login;
			PasswordHash = passwordHash;
			Email = email;
		}
	}
}
