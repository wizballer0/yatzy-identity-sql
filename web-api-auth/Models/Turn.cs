using System;
namespace web_api_auth.Models
{
    public class Turn
    {
        public int TurnId { get; set; }
        public string? Score { get; set; }

        public int GameId { get; set; }
        public Game Game { get; set; }

        public IList<Roll> Rolls { get; set; }
    }
}

