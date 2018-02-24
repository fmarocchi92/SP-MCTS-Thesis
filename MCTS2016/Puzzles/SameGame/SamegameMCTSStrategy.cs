using Common;
using Common.Abstract;
using MCTS.Standard.Utils;
using MCTS.Standard.Utils.UCT;
using MCTS2016.Common.Abstract;
using MCTS2016.SP_MCTS;
using MCTS2016.SP_MCTS.SP_UCT;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Puzzles.SameGame
{
    class SamegameMCTSStrategy : ISP_MCTSSimulationStrategy
    {
        private SP_MCTSAlgorithm mcts;
        private MersenneTwister rng;
        private double maxTimeInMinutes;

        public int iterations { get; set; }

        public SamegameMCTSStrategy(MersenneTwister rng,int iterations = 1000, double maxTimeInMinutes = 5, SP_MCTSAlgorithm mcts = null, double const_C = 4.31, double const_D = 96.67)
        {
            if (mcts == null)
            {
                mcts = new SP_MCTSAlgorithm(new SP_UCTTreeNodeCreator(const_C, const_D, rng),false);
            }
            this.mcts = mcts;
            this.iterations = iterations;
            this.maxTimeInMinutes = maxTimeInMinutes;
        }

        public string getFriendlyName()
        {
            return string.Format("MCTS[{0}]", this.iterations);
        }

        public string getTypeName()
        {
            return GetType().Name;
        }

        public IPuzzleMove selectMove(IPuzzleState gameState)
        {
            return mcts.Search(gameState, iterations);
        }
    }
}
