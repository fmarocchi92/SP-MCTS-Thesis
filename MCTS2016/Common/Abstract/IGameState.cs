/// <summary>
/// IGameState identify an abstract game state
/// </summary>

namespace Common.Abstract
{

    using System;
    using System.Collections.Generic;

    public interface IGameState
    {
        int playerJustMoved { get; }

        int currentPlayer { get; }

        int numberOfPlayers { get; }

        int size { get; }

        // get the play result
        //double GetResult(int player);

        // generate a random move for the current player
        IGameMove GetRandomMove();

        // clone the current state
        IGameState Clone();

        // get available moves
        List<IGameMove> GetMoves();

        // get board (for non blind AI)
        List<int> GetBoard();

        // get board cell
        int GetBoard(int x, int y);

        // current player performs a move
        void DoMove(IGameMove move);

        // represent the game state as a string
        string ToString();

        // represent the game state as a string
        string PrettyPrint();

        // return the current winner
        //  0 -> tie
        // -1 -> still running
        // otherwise it returns the current winner
        int Winner();

        // true if the game is finished
        bool EndState();

        bool isTerminal();

        // reset the game state and set the current player
        void Restart(int _player = 1);

        // the state has changed due to the move
        bool StateChanged();

        //
        int GetPositionIndex(int x, int y);

        double GetResult(int player);

        // when no moves are available the current player has to pass, this switches the players
        void Pass();

        int GetScore(int player);

        IGameMove GetSimulationMove();
    }
}

