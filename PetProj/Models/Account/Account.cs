﻿namespace PetProj.Models.Account;

public class Account
{
	public int Id { get; set; }
	public string Email { get; set; } = default!;
	public string PasswordHash { get; set; } = default!;
	public AccountLocation AccountRegistrationLocation { get; set; } = default!;
	public string? RefreshToken { get; set; }
	public DateTime? RefreshTokenExpityTime { get; set; }
}

public enum AccountLocation
{
	None = 0,
	Yandex = 1,
	Google = 2
}