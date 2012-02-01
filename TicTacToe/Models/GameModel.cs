using System;

namespace TicTacToe.Models
{
    public class GameModel
    {
        public string NewGameId { get; set; }

        public GameModel()
        {
            NewGameId = Guid.NewGuid().ToString("d");
        }
    }
}