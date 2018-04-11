using Common;
using Common.Abstract;
using MCTS.Standard.Utils;
using MCTS.Standard.Utils.UCT;
using MCTS2016.Common.Abstract;
using MCTS2016.SP_MCTS;
using MCTS2016.SP_MCTS.Optimizations;
using MCTS2016.SP_MCTS.Optimizations.UCT;
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
        private OptMCTSAlgorithm mcts;
        private MersenneTwister rng;
        private double maxTimeInMinutes;

        public int iterations { get; set; }
        public OptMCTSAlgorithm Mcts { get => mcts; set => mcts = value; }

        public SamegameMCTSStrategy(MersenneTwister rng, bool ucb1Tuned, bool rave, int raveThreshold, bool nodeRecycling, int memoryBudget, bool useNodeElimination, int iterations = 1000, OptMCTSAlgorithm mcts = null, double const_C = 4.31, double const_D = 96.67)
        {
            if (mcts == null)
            {
                mcts = new OptMCTSAlgorithm(new Opt_SP_UCTTreeNodeCreator(const_C, const_D, rng, ucb1Tuned, rave, raveThreshold, nodeRecycling ),iterations, memoryBudget ,false, false, useNodeElimination);
            }
            this.mcts = mcts;
            this.iterations = iterations;
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
