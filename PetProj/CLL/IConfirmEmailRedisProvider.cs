using PetProj.Models;

namespace PetProj.CLL;

public interface IConfirmEmailRedisProvider
{
	public Task CreateCodeForEmailAsync(string email, string code, string passwordHash);
	public Task<(Account user, string code)> GetCodeForEmailAsync(string email);
	public Task RemoveCodeForEmailAsync(string email);
}