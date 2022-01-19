using System;
using System.Web;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using web_api_auth.Models;
using web_api_auth.Data;

namespace web_api_auth.Controllers
{
    [ApiController]
    [Authorize]
    [Route("[controller]")]
    public class MainGameController : ControllerBase
    {
        private ApplicationDbContext applicationDbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        private string[] validKeys = {
                "Ones",
                "Dueces",
                "Treys",
                "Fours",
                "Fives",
                "Sixes",
                "One Pair",
                "Two Pair",
                "Trips",
                "Full House",
                "Quads",
                "Small Straight",
                "Big Straight",
                "Chance",
                "Yatzy",
                "Bonus"
            };

        public MainGameController(ApplicationDbContext dbContext, UserManager<ApplicationUser> userManager)
        {
            applicationDbContext = dbContext;
            _userManager = userManager;

        }

        // Returns data of game with corresponding GameId
        private GameData GetGameData(int gameId)
        {
            GameData gameData = new GameData(gameId);

            var players =
                applicationDbContext.Users
                    .Where(u => u.Games
                    .Any(u => u.GameId == gameId))
                    .Select(u => new { u.UserName, u.PlayerName })
                    .OrderBy(u => u.UserName)
                    .ToList();

            foreach (var player in players)
            {
                gameData.Players.Add(player.UserName, player.PlayerName);
            }

            var turns =
                applicationDbContext.Turns
                    .Where(t => t.GameId == gameId)
                    .Include(t => t.Rolls.OrderBy(r => r.RollId))
                    .OrderBy(t => t.TurnId)
                    .ToList();

            gameData.Turns = turns;

            if (turns.Count() > 0)
            {
                gameData.LastTurn = turns.Last();

                var rolls =
                    applicationDbContext.Rolls
                    .Where(r => r.TurnId == turns.Last().TurnId)
                    .OrderBy(r => r.RollId)
                    .ToList();

                gameData.CurrentNumberOfRolls = rolls.Count();

                if (rolls.Count() > 0)
                {
                    gameData.LastRoll = rolls.Last();
                }
            }

            return gameData;
        }

        [HttpGet]
        [Route("GameState")]
        public GameState GetGameState(int gameId)
        {
            var gameData = GetGameData(gameId);

            return CalculateGameState(gameData);
        }

        // Attempts to roll dice
        [HttpPost]
        [Route("RollDice")]
        public GameState RollDice(string diceToKeep, int gameId)
        {
            var gameData = GetGameData(gameId);

            if (GetCurrentPlayerId(gameData) == _userManager.GetUserId(User))
            {
                RollDiceHelper(diceToKeep, gameData);
            }

            return CalculateGameState(gameData);
        }

        private bool RollDiceHelper(string diceToKeep, GameData gameData)
        {
            Roll newRoll = new Roll();
            Random roll = new Random();

            if (gameData.CurrentNumberOfRolls == 0)
            {
                newRoll.TurnId = gameData.LastTurn.TurnId;
                newRoll.DiceOne = roll.Next(1, 7);
                newRoll.DiceTwo = roll.Next(1, 7);
                newRoll.DiceThree = roll.Next(1, 7);
                newRoll.DiceFour = roll.Next(1, 7);
                newRoll.DiceFive = roll.Next(1, 7);
                newRoll.DiceToKeep = "RRRRR";

                applicationDbContext.Add(newRoll);
                applicationDbContext.SaveChanges();

                return true;
            }
            else if (gameData.CurrentNumberOfRolls == 1 || gameData.CurrentNumberOfRolls == 2)
            {
                var lastRoll = gameData.LastRoll;

                newRoll.TurnId = gameData.LastTurn.TurnId;
                newRoll.DiceOne = diceToKeep[0].Equals('K') ? lastRoll.DiceOne : roll.Next(1, 7);
                newRoll.DiceTwo = diceToKeep[1].Equals('K') ? lastRoll.DiceTwo : roll.Next(1, 7);
                newRoll.DiceThree = diceToKeep[2].Equals('K') ? lastRoll.DiceThree : roll.Next(1, 7);
                newRoll.DiceFour = diceToKeep[3].Equals('K') ? lastRoll.DiceFour : roll.Next(1, 7);
                newRoll.DiceFive = diceToKeep[4].Equals('K') ? lastRoll.DiceFive : roll.Next(1, 7);
                newRoll.DiceToKeep = diceToKeep;

                applicationDbContext.Add(newRoll);
                applicationDbContext.SaveChanges();

                return true;
            }

            return false;
        }

        // Attempts to set score
        [HttpPost]
        [Route("SetScore")]
        public GameState SetScore(string key, int gameId)
        {
            var gameData = GetGameData(gameId);

            if (GetCurrentPlayerId(gameData) == _userManager.GetUserId(User))
            {
                SetScoreHelper(key, gameData);
            }

            return CalculateGameState(gameData);
        }

        private bool SetScoreHelper(string key, GameData gameData)
        {
            var currentPlayerScores = CalculatePlayerScores(gameData)[_userManager.GetUserId(User)];
            var scoreAlreadySet = currentPlayerScores[key] != null;
            var isValidKey = validKeys.Contains(key);
            var rollsMade = gameData.CurrentNumberOfRolls == 0;
            var currentTurnScoreAlreadySet = gameData.LastTurn.Score != null;

            if (!isValidKey || rollsMade || currentTurnScoreAlreadySet || scoreAlreadySet)
            {
                return false;
            }
            else
            {
                gameData.LastTurn.Score = key;

                if (gameData.Turns.Count < (15 * gameData.Players.Count))
                {
                    var newTurn = new Turn { GameId = gameData.LastTurn.GameId };
                    applicationDbContext.Add(newTurn);
                }

                applicationDbContext.SaveChanges();

                return true;
            }
        }

        private GameState CalculateGameState(GameData gameData)
        {
            int?[] dice = ReadDice(gameData.LastRoll);
            int throwsLeft = 3 - gameData.CurrentNumberOfRolls;

            var playerScores = CalculatePlayerScores(gameData);
            var futureScores = CalculateFutureScores(gameData);

            bool gameFinished = gameData.Turns.Count.Equals(15 * gameData.Players.Count);

            var currentPlayerName = gameData.Players[GetCurrentPlayerId(gameData)];
            string? winner = null;

            return new GameState(dice, throwsLeft, playerScores, gameFinished, currentPlayerName, winner, futureScores);
        }

        private Dictionary<string, Dictionary<string, int?>> CalculatePlayerScores(GameData gameData)
        {
            var gameScoreBoard = new Dictionary<string, Dictionary<string, int?>>();

            foreach (string player in gameData.Players.Keys)
            {
                var playerScores = ScoreTable();

                gameScoreBoard.Add(player, playerScores);
            }


            for (int i = 0; i < gameData.Turns.Count; i++)
            {
                var turn = gameData.Turns.ElementAt(i);

                if (turn.Score != null)
                {
                    var score = CalculateScore(turn.Score, ReadDice(turn.Rolls.Last()));

                    var playerScore = gameScoreBoard.ElementAt(i % gameData.Players.Count);
                    playerScore.Value[turn.Score] = score;
                }
            }

            foreach (Dictionary<string, int?> playerScores in gameScoreBoard.Values)
            {
                int bonus = CalculateBonus(playerScores);

                playerScores["Bonus"] = bonus;
            }

            return gameScoreBoard;
        }

        private int CalculateBonus(Dictionary<string, int?> playerScores)
        {
            int sum = 0;

            // Sum scores for ones through sixes
            for (int i = 0; i < 6; i++)
            {
                sum += playerScores.ElementAt(i).Value ?? 0;
            }

            return (sum > 64 ? 50 : 0);
        }

        private Dictionary<string, int?> CalculateFutureScores(GameData gameData)
        {
            var futureScores = ScoreTable();

            if (gameData.LastRoll != null)
            {
                var dice = ReadDice(gameData.LastRoll);

                foreach (string key in futureScores.Keys)
                {
                    futureScores[key] = CalculateScore(key, dice);
                }
            }

            return futureScores;
        }

        private Dictionary<string, int?> ScoreTable()
        {
            var scoreTable = new Dictionary<string, int?>();

            foreach (string key in validKeys)
            {
                scoreTable.Add(key, null);
            }

            return scoreTable;
        }

        // Returns all games user is currently in
        [HttpGet]
        [Route("PlayerGames")]
        public IList<Game> GetPlayerGames()
        {
            var gameList = applicationDbContext.Games
                .Where(g => g.Players
                .Any(p => p.UserName == _userManager.GetUserId(User)))
                .OrderBy(g => g.GameId)
                .ToList();

            return gameList;
        }

        // Returns Id of the player currently rolling
        private string GetCurrentPlayerId(GameData gameData)
        {
            // Key is UserName(Id) and Value is PlayerName
            string currentPlayerId = gameData.Players
                .ElementAt((gameData.Turns.Count() - 1) % gameData.Players.Count())
                .Key;

            return currentPlayerId;
        }

        // Returns current value of dice
        private int?[] ReadDice(Roll? lastRoll)
        {
            if (lastRoll == null)
            {
                return new int?[] { null, null, null, null, null };
            }
            else
            {
                int?[] dice = new int?[5];
                dice[0] = lastRoll.DiceOne;
                dice[1] = lastRoll.DiceTwo;
                dice[2] = lastRoll.DiceThree;
                dice[3] = lastRoll.DiceFour;
                dice[4] = lastRoll.DiceFive;

                return dice;
            }

        }

        // Returns the score for a given place and set of dice.
        private int CalculateScore(string key, int?[] dice)
        {
            int[] occurancies = { 0, 0, 0, 0, 0, 0 };

            for (int i = 0; i < dice.Length; i++)
            {
                if (dice[i] != null)
                {
                    occurancies[(int)dice[i] - 1]++;
                }
                //occurancies[(dice[i] ?? 1) - 1] = occurancies[(dice[i] ?? 1) - 1] + 1;
            }

            switch (key)
            {
                case "Ones":
                    return occurancies[0] * 1;
                case "Dueces":
                    return occurancies[1] * 2;
                case "Treys":
                    return occurancies[2] * 3;
                case "Fours":
                    return occurancies[3] * 4;
                case "Fives":
                    return occurancies[4] * 5;
                case "Sixes":
                    return occurancies[5] * 6;
                case "One Pair":
                    return CalculatePair(occurancies);
                case "Two Pair":
                    return CalculateTwoPair(occurancies);
                case "Trips":
                    return CalculateTrips(occurancies);
                case "Full House":
                    return CalculateFullHouse(occurancies);
                case "Quads":
                    return CalculateQuads(occurancies);
                case "Small Straight":
                    return CalculateSmallStraight(occurancies);
                case "Big Straight":
                    return CalculateBigStraight(occurancies);
                case "Chance":
                    return CalculateChance(occurancies);
                case "Yatzy":
                    return CalculateYatzy(occurancies);
                default:
                    return 99;
            }
        }

        private int CalculatePair(int[] i)
        {
            for (int j = 5; j >= 0; --j)
            {
                if (i[j] >= 2)
                {
                    return ((j + 1) * 2);
                }
            }
            return 0;
        }

        private int CalculateTwoPair(int[] i)
        {
            for (int j = 5; j >= 1; --j)
            {
                if (i[j] >= 2)
                {
                    for (int k = j - 1; k >= 0; k--)
                    {
                        if (i[k] >= 2)
                        {
                            return (((j + 1) * 2) + ((k + 1) * 2));
                        }
                    }
                }
            }
            return 0;
        }

        private int CalculateTrips(int[] i)
        {
            for (int j = 5; j >= 0; --j)
            {
                if (i[j] >= 3)
                {
                    return ((j + 1) * 3);
                }
            }
            return 0;
        }

        private int CalculateQuads(int[] i)
        {
            for (int j = 5; j >= 0; --j)
            {
                if (i[j] >= 4)
                {
                    return ((j + 1) * 4);
                }
            }
            return 0;
        }

        private int CalculateYatzy(int[] i)
        {
            for (int j = 5; j >= 0; --j)
            {
                if (i[j] == 5)
                {
                    return 50;
                }
            }
            return 0;
        }

        private int CalculateFullHouse(int[] i)
        {
            for (int j = 5; j >= 0; --j)
            {
                if (i[j] == 3)
                {
                    for (int k = 5; k >= 0; k--)
                    {
                        if (i[k] == 2)
                        {
                            return (((j + 1) * 3) + ((k + 1) * 2));
                        }
                    }
                }
            }
            return 0;
        }

        private int CalculateSmallStraight(int[] i)
        {
            if (i[0] == 1 && i[1] == 1 && i[2] == 1 && i[3] == 1 && i[4] == 1)
            {
                return 15;
            }
            else
            {
                return 0;
            }
        }

        private int CalculateBigStraight(int[] i)
        {
            if (i[1] == 1 && i[2] == 1 && i[3] == 1 && i[4] == 1 && i[5] == 1)
            {
                return 20;
            }
            else
            {
                return 0;
            }
        }

        private int CalculateChance(int[] i)
        {
            int sum = 0;

            for (int j = 0; j < i.Length; j++)
            {
                sum += i[j] * (j + 1);
            }

            return sum;
        }
    }
}

