using PetProj.Models.Account;

namespace PetProj.CLL;

public interface IConfirmEmailRedisProvider
{
	public Task SaveAccountRegistrationCacheAsync(AccountRegistrationCache account);
	public Task<AccountRegistrationCache?> GetAccountRegistrationCacheAsync(string email);
	public Task RemoveAccountRegistrationCacheAsync(string email);
}