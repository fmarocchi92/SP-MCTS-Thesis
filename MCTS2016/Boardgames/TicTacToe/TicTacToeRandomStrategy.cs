using System;
using Common;
using Common.Abstract;
using System.Collections.Generic;

namespace BoardGames
{
    public class TicTacToeRandomStrategy : ITicTacToeSimulationStrategy
    {
        public IGameMove selectMove(IGameState gameState)
        {
            List<IGameMove> moves = gameState.GetMoves();
			return (IGameMove) moves[RNG.Next(moves.Count)];
        }

        public string getTypeName()
        {
            return this.GetType().Name;
        }

        public string getFriendlyName()
        {
            return "Random";
        }
    }
}

