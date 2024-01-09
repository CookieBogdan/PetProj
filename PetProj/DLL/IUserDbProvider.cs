using PetProj.Models;

namespace PetProj.DLL;

public interface IUserDbProvider
{
	public Task<bool> IsUserExistByEmailAsync(string email, out Account? user);
	public Task CreateNewUserAsync(Account user);
}
