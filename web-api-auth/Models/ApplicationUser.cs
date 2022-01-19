using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace web_api_auth.Models
{
	public class ApplicationUser : IdentityUser
	{
		[Required, MaxLength(20), MinLength(3)]
		public string PlayerName { get; set; }

		public ICollection<Game> Games { get; set; }
	}
}

