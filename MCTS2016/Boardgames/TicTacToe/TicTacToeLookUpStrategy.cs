using System;
using Common.Abstract;

namespace BoardGames
{
    /// <summary>
    /// Tic tac toe player using a lookup table of preferred moves.
    /// From https://www3.ntu.edu.sg/home/ehchua/programming/java/JavaGame_TicTacToe_AI.html
    /// </summary>
    public class TicTacToeLookUpStrategy : ITicTacToeSimulationStrategy
    {
        // uses rows,cols notation (in the code we use x,y)
        private int[,] preferredMoves = new int[9, 2]
        { 
            { 1, 1 },
            { 0, 0 },
            { 0, 2 },
            { 2, 0 },
            { 2, 2 },
            { 0, 1 },
            { 1, 0 },
            { 1, 2 },
            { 2, 1 }
        };

        public IGameMove selectMove(IGameState gameState)
        {
            int i = 0;
            while (i < (gameState.size * gameState.size) &&
                   gameState.GetBoard(preferredMoves[i, 1], preferredMoves[i, 0]) != 0)
            {
                i++;
            }

            return (IGameMove)gameState.GetPositionIndex(preferredMoves[i, 1], preferredMoves[i, 0]);
        }

        public string getTypeName()
        {
            return this.GetType().Name;
        }

        public string getFriendlyName()
        {
            return "LookUp";
        }
    }
}

