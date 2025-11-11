namespace Monolit.Helpers.Etc.Interfaces
{
	public interface IPasswordHasher
	{
		string HashPassword(string password);
		bool VerifyPassword(string password, string fullHash);
	}
}
