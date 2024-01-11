using Microsoft.AspNetCore.Mvc;

using PetProj.CLL;
using PetProj.DLL.DbProviders;
using PetProj.Models.Account;
using PetProj.Models.Responses;
using PetProj.SLL;
using PetProj.Utils;

using Swashbuckle.AspNetCore.Annotations;

using System.Security.Claims;

namespace PetProj.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
	private readonly IAccountDbProvider _accountDbProvider;
	private readonly IEmailSender _emailSender;
	private readonly IConfirmEmailRedisProvider _confirmEmailRedisProvider;
	private readonly IJwtService _jwtService;
	public AuthController(
		IAccountDbProvider accountDbProvider,
		IEmailSender emailSender,
		IConfirmEmailRedisProvider confirmEmailRedisProvider,
		IJwtService jwtService)
	{
		_accountDbProvider = accountDbProvider;
		_emailSender = emailSender;
		_confirmEmailRedisProvider = confirmEmailRedisProvider;
		_jwtService = jwtService;
	}

	private DateTime RefreshTokenValidity => DateTime.UtcNow.AddMonths(1);

	/// <summary>Account registration by email and password.</summary>
	[SwaggerResponse(StatusCodes.Status200OK)]
	[SwaggerResponse(StatusCodes.Status409Conflict)]
	[HttpPost, Route("register")]
	public async Task<IActionResult> Registration(AccountDto request)
	{
		var account = await _accountDbProvider.GetAccountByEmailAsync(request.Email);
		if (account is not null)
		{
			return Conflict("Email already exists.");
		}

		string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
		//UNDONE: code to string
		int code = int.Parse(Enumerable.Range(0, 6).Aggregate("", (str, _) => str + Random.Shared.Next(1, 10)));

		var accountCache = new AccountRegistrationCache(request.Email, code, passwordHash);
		await _confirmEmailRedisProvider.SaveAccountRegistrationCacheAsync(accountCache);
		await _emailSender.SendConfirmationEmailForRegistrationAsync(request.Email, code);

		return Ok();
	}

	/// <summary>Confirm account registration using the code from your email.</summary>
	[SwaggerResponse(StatusCodes.Status200OK, null, typeof(TokenResponse))]
	[SwaggerResponse(StatusCodes.Status400BadRequest)]
	[HttpPost, Route("register/confirm")]
	public async Task<IActionResult> ConfirmRegistration(AccountConfirmDto request)
	{
		var accountCache = await _confirmEmailRedisProvider.GetAccountRegistrationCacheAsync(request.Email);
		if (accountCache is null)
		{
			return BadRequest("Email not exist.");
		}

		if (accountCache.Code != request.RequestCode)
		{
			return BadRequest("Code is not correct.");
		}

		string refreshToken = _jwtService.GenerateRefreshToken();
		await _accountDbProvider.CreateAccountAsync(new Account()
		{
			Email = accountCache.Email,
			PasswordHash = accountCache.PasswordHash,
			RefreshToken = refreshToken,
			RefreshTokenExpityTime = RefreshTokenValidity
		});
		await _confirmEmailRedisProvider.RemoveAccountRegistrationCacheAsync(request.Email);

		var claims = new List<Claim>() { new("Email", accountCache.Email) };
		string accessToken = _jwtService.GenerateAccessToken(claims);

		return Ok(new TokenResponse(accessToken, refreshToken));
	}

	/// <summary>Login to account by email and password.</summary>
	[SwaggerResponse(StatusCodes.Status200OK, null, typeof(TokenResponse))]
	[SwaggerResponse(StatusCodes.Status409Conflict)]
	[HttpPost, Route("login")]
	public async Task<IActionResult> Login(AccountDto request)
	{
		var account = await _accountDbProvider.GetAccountByEmailAsync(request.Email);
		if (account is null || !BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
		{
			return BadRequest("Account details not correct.");
		}

		string refreshToken;
		if (account.RefreshTokenExpityTime is null || account.RefreshTokenExpityTime < DateTime.UtcNow)
		{
			refreshToken = _jwtService.GenerateRefreshToken();
			await _accountDbProvider.UpdateAccountRefreshToken(account.Id, refreshToken, RefreshTokenValidity);
		}
		else
		{
			refreshToken = account.RefreshToken!;
		}
		var claims = new List<Claim>() { new("Email", request.Email) };
		string accessToken = _jwtService.GenerateAccessToken(claims);

		return Ok(new TokenResponse(accessToken, refreshToken));
	}
}

