using System.Collections.Concurrent;
using System.Linq;
using TicTacToe.Models;

namespace TicTacToe.Helpers
{
    public static class TicTacToeHelpers
    {
        /// <summary>
        /// Returns GameSession object for the given Game ID.
        /// </summary>
        /// <param name="games">Collection of all opened games</param>
        /// <param name="gameId">Game ID</param>
        /// <returns>GameSession if found; otherwise NULL.</returns>
        public static GameSession GetGameSessionByGameId(
            ConcurrentDictionary<string, GameSession> games, 
            string gameId)
        {
            GameSession gameSession =
                (
                    from g in games
                    where string.CompareOrdinal(g.Key, gameId) == 0
                    select g.Value
                ).FirstOrDefault();

            return gameSession;
        }

        /// <summary>
        /// Determines if the given state is the winning one.
        /// </summary>
        /// <param name="session">GameSession object</param>
        /// <param name="field">Current click position</param>
        /// <param name="state">Current state (player)</param>
        /// <param name="size">Size of the grid</param>
        /// <returns>
        /// GridItemState.Empty if no winner has been found; 
        /// otherwise GridItemState.Player1 or GridItemState.Player1
        /// </returns>
        public static GridItemState GetWinningState(
            GameSession session, 
            Field field, 
            GridItemState state, 
            int size)
        {
            // TODO: Determine whether the given state is the winning one

            return GridItemState.Empty;
        }
    }
}