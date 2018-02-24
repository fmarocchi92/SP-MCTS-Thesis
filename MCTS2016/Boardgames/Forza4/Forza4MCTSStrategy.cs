using System;
using MCTS.Standard.Utils;
using MCTS.Standard.Utils.UCT;
using Common.Abstract;

namespace BoardGames
{
	public class Forza4MCTSStrategy : IForza4SimulationStrategy
    {
        private MCTSAlgorithm mcts;

        public int iterations { get; set; }

        public Forza4MCTSStrategy(int iterations, MCTSAlgorithm mcts = null)
        {
            if (mcts == null)
            {
                mcts = new MCTSAlgorithm(new UCTTreeNodeCreator());
            }

            this.mcts = mcts;
            this.iterations = iterations;
        }

        public IGameMove selectMove(IGameState gameState)
        {
            return mcts.Search(gameState, iterations);
        }

        public string getTypeName()
        {
            return this.GetType().Name;
        }

        public string getFriendlyName()
        {
            return string.Format("MCTS[{0}]", this.iterations);
        }
    }
}

