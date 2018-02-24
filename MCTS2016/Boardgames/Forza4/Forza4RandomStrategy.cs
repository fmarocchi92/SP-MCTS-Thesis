using System;
using System.Collections.Generic;
using Common.Abstract;

namespace BoardGames
{
    public class Forza4RandomStrategy : IForza4SimulationStrategy
    {
        //private Random random = new Random();

        public IGameMove selectMove(IGameState gameState)
        {
            List<IGameMove> moves = gameState.GetMoves();
            return (IGameMove)moves[(new Random()).Next(0, moves.Count)];
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
