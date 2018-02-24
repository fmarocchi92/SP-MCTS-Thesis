using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Common.Abstract
{
    public interface IPuzzleState
    {
       
        int size { get; }

        // get the play result
        //double GetResult(int player);

        // generate a random move for the current player
        IPuzzleMove GetRandomMove();

        // clone the current state
        IPuzzleState Clone();

        // get available moves
        List<IPuzzleMove> GetMoves();

        // get board (for non blind AI)
        List<int> GetBoard();

        // get board cell
        int GetBoard(int x, int y);

        // current player performs a move
        void DoMove(IPuzzleMove move);

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

        // reset the game state
        void Restart();

        // the state has changed due to the move
        bool StateChanged();

        //
        int GetPositionIndex(int x, int y);

        double GetResult();

        // when no moves are available the current player has to pass, this switches the players
        void Pass();

        int GetScore();

        IPuzzleMove GetSimulationMove();
    }
}
