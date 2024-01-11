using PetProj.Models.Account;

namespace PetProj.DLL.DbProviders;

public interface IAccountDbProvider
{
	public Task<Account?> GetAccountByEmailAsync(string email);
	public Task CreateAccountAsync(Account account);
	public Task UpdateAccountRefreshToken(int accountId, string refreshToken, DateTime refreshTokenExpityTime);
}
