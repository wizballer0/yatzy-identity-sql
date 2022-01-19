using System;
using System.ComponentModel.DataAnnotations;

namespace web_api_auth.Models
{
    public class UserDetails
    {
        [Required]
        public string UserName { get; set; }

        [Required]
        public string Password { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string PlayerName { get; set; }
    }
}

