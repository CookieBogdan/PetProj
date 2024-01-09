using System;
namespace PetProj.Models
{
	public class UserDto
	{
		public required string Email { get; set; } = string.Empty;
		public required string Password { get; set; } = string.Empty;
	}
}

