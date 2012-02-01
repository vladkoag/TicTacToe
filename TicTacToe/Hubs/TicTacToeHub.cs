using System.Collections.Concurrent;
using SignalR.Hubs;
using TicTacToe.Helpers;
using TicTacToe.Models;

namespace TicTacToe.Hubs
{
    [HubName("TicTacToeHub")]
    public class TicTacToeHub : Hub
    {
        /// <summary>
        /// Holds all game sessions.
        /// Key: Game GUID
        /// Value: GameSession
        /// </summary>
        private static readonly ConcurrentDictionary<string, GameSession> Games = 
            new ConcurrentDictionary<string, GameSession>();

        /// <summary>
        /// Create a new game.
        /// </summary>
        /// <param name="gameId">Game GUID</param>
        /// <returns>True, if the game has been created; otherwise False.</returns>
        public bool CreateNewGame(string gameId)
        {
            if (Games.TryAdd(gameId, new GameSession(Constants.SIZE, Context.ClientId)))
            {
                Clients[gameId].addUser();

                return true;
            }

            return false;
        }

        /// <summary>
        /// Joins to an existing game.
        /// </summary>
        /// <param name="gameId">Game GUID</param>
        /// <returns>
        /// ResultData.Success == true if success. 
        /// Otherwise ResultData.Success == false and ResultData.FailReason contains reason.
        /// </returns>
        public ResultData Join(string gameId)
        {
            ResultData r = new ResultData();

            if (string.IsNullOrEmpty(gameId))
            {
                r.FailReason = "Please enter game ID.";
                return r;
            }

            // Get the current GameSession.
            GameSession session = TicTacToeHelpers.GetGameSessionByGameId(Games, gameId);

            if (session == null)
            {
                r.FailReason = "Unable to find game ID: " + gameId;
                return r;
            }

            if (!string.IsNullOrEmpty(session.Player2Id) &&
                string.CompareOrdinal(session.Player2Id, Context.ClientId) != 0)
            {
                r.FailReason = "Unable to join. Another player joined this game.";
                return r;
            }

            // Add Player 2 to the game.
            Games[gameId].Player2Id = Context.ClientId;
            session = TicTacToeHelpers.GetGameSessionByGameId(Games, gameId);

            // Send a message to Player 1.
            Clients[session.Player1Id].player2Joined();

            // Set turn number to both players.
            SetTurn(session, 0);

            r.Success = true;

            return r;
        }

        /// <summary>
        /// Sets turn.
        /// </summary>
        /// <param name="session">Current GameSession</param>
        /// <param name="playerNumber">Player number</param>
        private void SetTurn(GameSession session, int playerNumber)
        {
            Clients[session.Player1Id].setTurn(playerNumber);
            Clients[session.Player2Id].setTurn(playerNumber);

            if (playerNumber == 0)
            {
                Clients[session.Player1Id].addMessage("Your turn.");
                Clients[session.Player2Id].addMessage("Wait. Current turn: Player " + (playerNumber + 1));
            }
            else
            {
                Clients[session.Player2Id].addMessage("Your turn.");
                Clients[session.Player1Id].addMessage("Wait. Current turn: Player " + (playerNumber + 1));
            }
        }

        /// <summary>
        /// Ends the given game.
        /// </summary>
        /// <param name="gameId">Game ID</param>
        /// <returns>True if game has been ended; otherwise False.</returns>
        public bool Leave(string gameId)
        {
            GameSession session = TicTacToeHelpers.GetGameSessionByGameId(Games, gameId);
                
            if (session == null)
                return false;

            // Remove Player1
            if (!string.IsNullOrEmpty(session.Player1Id)) Clients[session.Player1Id].endGame();

            // Remove Player2
            if (!string.IsNullOrEmpty(session.Player2Id)) Clients[session.Player2Id].endGame();
            
            GameSession gameSessionToRemove;
            Games.TryRemove(gameId, out gameSessionToRemove);
            
            return true;
        }

        /// <summary>
        /// Responses to a player move.
        /// </summary>
        /// <param name="gameId">Game ID</param>
        /// <param name="currentPlayer">Current player index</param>
        /// <param name="field">Current move</param>
        public void SelectField(string gameId, int currentPlayer, Field field)
        {
            GameSession session = TicTacToeHelpers.GetGameSessionByGameId(Games, gameId);

            GridItemState currentState;

            if (currentPlayer == 0)
            {
                // Current turn: Player 1

                // Switch to Player 2.
                Clients[session.Player2Id].setOtherPlayerField(field);
                
                // Set the current state.
                currentState = GridItemState.Player1;
            }
            else
            {
                // Current turn: Player 2

                // Switch to Player 1.
                Clients[session.Player1Id].setOtherPlayerField(field);
                
                // Set the current state.
                currentState = GridItemState.Player2;
            }

            // Update the game grid.
            session.Grid[field.x, field.y] = currentState;
            
            // Determine if the current move yields the winning state.
            GridItemState winningState = 
                TicTacToeHelpers.GetWinningState(session, field, currentState, Constants.SIZE);


            if (winningState != GridItemState.Empty)
            {
                // We have a winner!

                if (winningState == GridItemState.Player1)
                {
                    // Player 1 won!
                    Clients[session.Player1Id].addMessage("You won!");
                    Clients[session.Player2Id].addMessage("Player 1 won :-(");
                }
                else
                {
                    // Player 2 won!
                    Clients[session.Player2Id].addMessage("You won!");
                    Clients[session.Player1Id].addMessage("Player 2 won :-(");
                }

                Leave(gameId);

            }
            else
            {
                // No winner yet. Switch the turn.

                SetTurn(session, currentPlayer == 0 ? 1 : 0);
            }
        }
    }
}