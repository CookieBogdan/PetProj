using PetProj.Models.Account;

namespace PetProj.CLL;

public class FakeConfirmEmailRedisProvider : IConfirmEmailRedisProvider
{
	private readonly static List<AccountRegistrationCache> redis = [];
	public async Task SaveAccountRegistrationCacheAsync(AccountRegistrationCache accountCache)
	{
		//[TODO]: 2 query
		redis.Add(accountCache);
		await Task.CompletedTask;
	}

	public async Task RemoveAccountRegistrationCacheAsync(string email)
	{
		var accountCache = redis.FirstOrDefault(a => a.Email == email)
			?? throw new ArgumentNullException($"account registration cache with email: {email}, not exist. ");

		redis.Remove(accountCache);
		await Task.CompletedTask;
	}

	public async Task<AccountRegistrationCache?> GetAccountRegistrationCacheAsync(string email)
	{
		var accountCache = redis.FirstOrDefault(u => u.Email == email);
		return await Task.FromResult(accountCache);
	}
}
