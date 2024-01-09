using PetProj.CLL;
using PetProj.DLL;
using PetProj.Models;
using PetProj.SLL;
using PetProj.Utils.Authentication;

using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PetProj.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class AuthController : ControllerBase
	{
		private readonly IConfiguration _configuration;
		private readonly IUserDbProvider _userDbProvider;
		private readonly IEmailSender _emailSender;
		private readonly IConfirmEmailRedisProvider _confirmEmailRedisProvider;
		public AuthController(IConfiguration configuration, IUserDbProvider userDbProvider, IEmailSender emailSender, IConfirmEmailRedisProvider confirmEmailRedisProvider)
		{
			_configuration = configuration;
			_userDbProvider = userDbProvider;
			_emailSender = emailSender;	
			_confirmEmailRedisProvider = confirmEmailRedisProvider;
		}

		[HttpPost]
		[Route("register")]
		public async Task<IActionResult> Register(UserDto request)
		{
			if(await _userDbProvider.IsUserExistByEmailAsync(request.Email, out Account? _))
			{
				return Conflict("Email already exists.");
			}

			var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
			string code = Enumerable.Range(0, 6).Aggregate("", (str, _) => str + Random.Shared.Next(0, 10));

			await _confirmEmailRedisProvider.CreateCodeForEmailAsync(request.Email, code, passwordHash);
			await _emailSender.SendConfirmationEmailForRegistrationAsync(request.Email, code);

			return Ok();
		}

		[HttpPost]
		[Route("register/confirm")]
		public async Task<IActionResult> ConfirmRegister(string email, string requestCode)
		{
			(Account user, string userCode) = await _confirmEmailRedisProvider.GetCodeForEmailAsync(email);
			if(userCode != requestCode)
			{
				return BadRequest("code is not correct");
			}
			await _userDbProvider.CreateNewUserAsync(user);
			await _confirmEmailRedisProvider.RemoveCodeForEmailAsync(email);

			string jwt = CreateJwtToken(user.Email);
			return Ok(jwt);
		}

		[HttpPost]
		[Route("login")]
		public async Task<IActionResult> Login(UserDto request)
		{
			if (!await _userDbProvider.IsUserExistByEmailAsync(request.Email, out Account? user))
			{
				return BadRequest("User not found");
			}
			if (!BCrypt.Net.BCrypt.Verify(request.Password, user!.PasswordHash))
			{
				return BadRequest("Password is not correct");
			}

			string jwt = CreateJwtToken(user.Email);
			return Ok(jwt);
		}

		private string CreateJwtToken(string email)
		{
			var claims = new List<Claim>() { new Claim("Email", email) };

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration.GetSection("AppSettings:JwtToken").Value!));
			var cred = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var token = new JwtSecurityToken(
				claims: claims,
				issuer: AuthOptions.ISSUER,
				audience: AuthOptions.AUDIENCE,
				expires: DateTime.UtcNow.AddDays(1),
				signingCredentials: cred);

			string jwt = new JwtSecurityTokenHandler().WriteToken(token);
			return jwt;
			//new JwtSecurityTokenHandler().ValidateToken(jwt, new TokenValidationParameters()
			//{
			//	ValidateIssuer = true,
			//	ValidateAudience = true,
			//	ValidateLifetime = true,
			//	ValidateIssuerSigningKey = true,
			//	ValidIssuer = "https://localhost:7210/",
			//	ValidAudience = "https://localhost:7210/",
			//	IssuerSigningKey = key,
			//}, out SecurityToken t);
		}
	}
}

