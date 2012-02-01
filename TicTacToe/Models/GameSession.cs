using System;

namespace TicTacToe.Models
{
    [Serializable]
    public class GameSession
    {
        public string Player1Id { get; set; }
        public string Player2Id { get; set; }
        public GridItemState[,] Grid { get; set; }

        public GameSession(int size, string player1Id)
        {
            Grid = new GridItemState[size, size];
            Player1Id = player1Id;
        }
    }
}