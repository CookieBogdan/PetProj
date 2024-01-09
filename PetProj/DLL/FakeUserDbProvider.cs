using PetProj.Models;

namespace PetProj.DLL;

public class FakeUserDbProvider : IUserDbProvider
{
	private static readonly List<Account> _users = [];
	public async Task CreateNewUserAsync(Account user)
	{
		_users.Add(user);
	}

	public Task<bool> IsUserExistByEmailAsync(string email, out Account? user)
	{
		user = _users.FirstOrDefault(u => u.Email == email);
		return Task.FromResult(user is not null);
	}
}
