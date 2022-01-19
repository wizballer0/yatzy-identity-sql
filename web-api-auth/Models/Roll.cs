using System;
namespace web_api_auth.Models
{
    public class Roll
    {
        public int RollId { get; set; }

        public int DiceOne { get; set; }
        public int DiceTwo { get; set; }
        public int DiceThree { get; set; }
        public int DiceFour { get; set; }
        public int DiceFive { get; set; }

        public string DiceToKeep { get; set; }

        public int TurnId { get; set; }
        public Turn Turn { get; set; }
    }
}

