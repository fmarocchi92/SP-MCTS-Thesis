using System;
using Common.Abstract;
using MCTS.Standard.Utils;
using MCTS.Standard.Utils.UCT;

namespace BoardGames
{
	public class TicTacToeMCTSStrategy : ITicTacToeSimulationStrategy
    {
        private MCTSAlgorithm mcts;

        public int iterations { get; set; }

        public TicTacToeMCTSStrategy(int iterations, MCTSAlgorithm mcts = null)
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

