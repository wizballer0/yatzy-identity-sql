using System;
using web_api_auth.Models;

namespace web_api_auth.Models
{
	public class GameData
	{
		public int GameId { get; set; }
		public IDictionary<string, string> Players { get; set; }
		public IList<Turn> Turns { get; set; }
		public Turn? LastTurn { get; set; }
		public int CurrentNumberOfRolls { get; set; }
		public Roll? LastRoll { get; set; }

		public GameData(int gameId)
		{
			GameId = gameId;
			Players = new Dictionary<string, string>();
		}
	}
}

