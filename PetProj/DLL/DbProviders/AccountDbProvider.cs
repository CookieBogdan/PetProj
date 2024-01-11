using Microsoft.EntityFrameworkCore;
using PetProj.Models.Account;

namespace PetProj.DLL.DbProviders;

public class AccountDbProvider : IAccountDbProvider
{
	private readonly ApplicationDbContext _dbContext;
	public AccountDbProvider(ApplicationDbContext dbContext)
	{
		_dbContext = dbContext;
	}

	public async Task CreateAccountAsync(Account user)
	{
		await _dbContext.Accounts.AddAsync(user);
		await _dbContext.SaveChangesAsync();
	}

	//UNDONE: without tracking
	public async Task<Account?> GetAccountByEmailAsync(string email)
	{
		var account = await _dbContext
			.Accounts
			.FirstOrDefaultAsync(a => a.Email == email);

		return account;
	}

	public async Task UpdateAccountRefreshToken(int accountId, string refreshToken, DateTime refreshTokenExpityTime)
	{
		await _dbContext
			.Accounts
			.Where(a => a.Id == accountId)
			.ExecuteUpdateAsync(p => p
				.SetProperty(a => a.RefreshToken, refreshToken)
				.SetProperty(a => a.RefreshTokenExpityTime, refreshTokenExpityTime)
			);
	}
}
