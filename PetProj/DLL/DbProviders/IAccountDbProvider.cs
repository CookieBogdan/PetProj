using PetProj.Models.Account;

namespace PetProj.DLL.DbProviders;

public interface IAccountDbProvider
{
	public Task<Account?> GetAccountByEmailAsync(string email);
	public Task<int> CreateAccountAsync(Account account);
	public Task UpdateAccountRefreshToken(int accountId, string? refreshToken, DateTime? refreshTokenExpityTime);
	public Task<Account?> GetAccountByIdAsync(int accountId);
}
