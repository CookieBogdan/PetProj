using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;

using PetProj.CLL;
using PetProj.DLL.DbProviders;
using PetProj.Models;
using PetProj.Models.Account;
using PetProj.Models.Responses;
using PetProj.SLL;
using PetProj.Utils;

using Swashbuckle.AspNetCore.Annotations;

namespace PetProj.Controllers;

[Route("api/auth")]
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

	/// <summary>Account registration by email and password.</summary>
	[SwaggerResponse(StatusCodes.Status204NoContent)]
	[SwaggerResponse(StatusCodes.Status409Conflict)]
	[HttpPost, Route("register")]
	public async Task<IActionResult> Registration(AccountDto request)
	{
		var account = await _accountDbProvider.GetAccountByEmailAsync(request.Email);
		if (account is not null)
		{
			if (account.AccountRegistrationLocation != AccountLocation.None)
			{
				return BadRequest($"Account registrated with help: {account?.AccountRegistrationLocation}.");
			}
			return Conflict("Email already exists.");
		}

		string passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
		string code = Enumerable.Range(0, 6).Aggregate("", (str, _) => str + Random.Shared.Next(0, 10));

		var accountCache = new AccountRegistrationCache(request.Email, code, passwordHash);
		await _confirmEmailRedisProvider.SaveAccountRegistrationCacheAsync(accountCache);
		await _emailSender.SendConfirmationEmailForRegistrationAsync(request.Email, code);

		return NoContent();
	}

	/// <summary>Confirm account registration using the code from your email.</summary>
	[SwaggerResponse(StatusCodes.Status200OK, null, typeof(AuthenticatedResponse))]
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
		var accountId = await _accountDbProvider.CreateAccountAsync(new Account()
		{
			Email = accountCache.Email,
			PasswordHash = accountCache.PasswordHash,
			RefreshToken = refreshToken,
			RefreshTokenExpityTime = _jwtService.RefreshTokenValidity
		});
		await _confirmEmailRedisProvider.RemoveAccountRegistrationCacheAsync(request.Email);

		var claims = _jwtService.CreateClaims(new UserClaims(accountId, accountCache.Email));
		string accessToken = _jwtService.GenerateAccessToken(claims);

		return Ok(new AuthenticatedResponse(accessToken, refreshToken));
	}

	/// <summary>Login to account by email and password.</summary>
	[SwaggerResponse(StatusCodes.Status200OK, null, typeof(AuthenticatedResponse))]
	[SwaggerResponse(StatusCodes.Status409Conflict)]
	[HttpPost, Route("login")]
	public async Task<IActionResult> Login(AccountDto request)
	{
		var account = await _accountDbProvider.GetAccountByEmailAsync(request.Email);
		if(account?.AccountRegistrationLocation != AccountLocation.None)
		{
			return BadRequest($"Account registrated with help: {account?.AccountRegistrationLocation}.");
		}
		if (account is null || !BCrypt.Net.BCrypt.Verify(request.Password, account.PasswordHash))
		{
			return BadRequest("Account details not correct.");
		}

		string refreshToken;
		if (account.RefreshTokenExpityTime is null || account.RefreshTokenExpityTime < DateTime.UtcNow)
		{
			refreshToken = _jwtService.GenerateRefreshToken();
			await _accountDbProvider.UpdateAccountRefreshToken(account.Id, refreshToken, _jwtService.RefreshTokenValidity);
		}
		else
		{
			refreshToken = account.RefreshToken!;
		}
		var claims = _jwtService.CreateClaims(new UserClaims(account.Id, request.Email));
		string accessToken = _jwtService.GenerateAccessToken(claims);

		return Ok(new AuthenticatedResponse(accessToken, refreshToken));
	}

	/// <summary>Refresh account access token by 2 tokens.</summary>
	[SwaggerResponse(StatusCodes.Status200OK, null, typeof(AuthenticatedResponse))]
	[SwaggerResponse(StatusCodes.Status401Unauthorized)]
	[SwaggerResponse(StatusCodes.Status400BadRequest)]
	[HttpPost, Route("refresh")]
	public async Task<IActionResult> RefreshAccessToken([FromBody] string refreshToken)
	{
		if (!HttpContext.Request.Headers.TryGetValue("Authorization", out StringValues headerAuth))
		{
			return Unauthorized("AccessToken is not valid.");
		}

		string spoiledAccessToken = headerAuth.First()!.Split(' ', StringSplitOptions.RemoveEmptyEntries)[1];
		UserClaims userClaims = _jwtService.GetUserClaimsFromExpiredToken(spoiledAccessToken);

		var account = await _accountDbProvider.GetAccountByIdAsync(userClaims.Id);
		if (account is null)
		{
			return BadRequest("AccessToken payload is not valid.");
		}

		if (account.RefreshToken is null || account.RefreshTokenExpityTime is null)
		{
			return BadRequest("You must not refresh token, you should login to account.");
		}

		if (refreshToken != account.RefreshToken)
		{
			return Unauthorized("RefreshToken is not valid.");
		}

		var claims = _jwtService.CreateClaims(userClaims);
		var accessToken = _jwtService.GenerateAccessToken(claims);

		if (account.RefreshTokenExpityTime < DateTime.UtcNow)
		{
			refreshToken = _jwtService.GenerateRefreshToken();
			await _accountDbProvider.UpdateAccountRefreshToken(account.Id, refreshToken, _jwtService.RefreshTokenValidity);
		}
		return Ok(new AuthenticatedResponse(accessToken, refreshToken));
	}

	/// <summary>Logout from account and delete refreshToken from db.</summary>
	[SwaggerResponse(StatusCodes.Status204NoContent)]
	[Authorize]
	[HttpPost, Route("logout")]
	public async Task<IActionResult> Logout()
	{
		int accountId = int.Parse(HttpContext.User.Claims.First(c => c.Type == "Id").Value!);

		await _accountDbProvider.UpdateAccountRefreshToken(accountId, null, null);
		
		return NoContent();
	}
}