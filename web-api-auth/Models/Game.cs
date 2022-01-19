using System;

namespace web_api_auth.Models
{
	public class Game
	{
		public int GameId { get; set; }

		public ICollection<ApplicationUser> Players { get; set; }

		public IList<Turn> Turns { get; set; }
	}
}

