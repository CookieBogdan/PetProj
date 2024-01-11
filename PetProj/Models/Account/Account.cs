namespace PetProj.Models.Account;

public class Account
{
	public int Id { get; set; }
	public string Email { get; set; } = default!;
	public string PasswordHash { get; set; } = default!;
	public string? RefreshToken { get; set; }
	public DateTime? RefreshTokenExpityTime { get; set; }
}