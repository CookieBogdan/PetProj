using PetProj.Models;

namespace PetProj.CLL;

public class FakeConfirmEmailRedisProvider : IConfirmEmailRedisProvider
{
	private readonly static List<(string email, string code, string passwordHash)> redis = [];
	public async Task CreateCodeForEmailAsync(string email, string code, string passwordHash)
	{
		redis.Add((email, code, passwordHash));
	}

	public async Task RemoveCodeForEmailAsync(string email)
	{
		var user = redis.FirstOrDefault(u => u.email == email);
		redis.Remove(user);
	}

	public async Task<(Account user, string code)> GetCodeForEmailAsync(string email)
	{
		var user = redis.FirstOrDefault(u => u.email == email);
		return (new User(user.email, user.passwordHash), user.code);
	}
}
