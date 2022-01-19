using System;
using System.Collections.Generic;
using System.Linq;

namespace web_api_auth.Models
{
    public class GameState
    {
        public int?[] dice { get; set; }
        public int throwsLeft { get; set; }
        public Dictionary<string, Dictionary<string, int?>> playerScores { get; set; }
        public bool gameFinished { get; set; }
        public string currentPlayer { get; set; }
        public string? winner { get; set; }
        public Dictionary<string, int?> futureScores { get; set; }

        public GameState
            (int?[] dice, int throwsLeft, Dictionary<string, Dictionary<string, int?>> playerScores,
            bool gameFinished, string currentPlayer, string winner,
            Dictionary<string, int?> futureScores)
        {
            this.dice = dice;
            this.throwsLeft = throwsLeft;
            this.playerScores = playerScores;
            this.gameFinished = gameFinished;
            this.currentPlayer = currentPlayer;
            this.winner = winner;
            this.futureScores = futureScores;
        }
    }
}

