using Common;
using Common.Abstract;
using MCTS.Standard.Utils;
using MCTS2016.Common.Abstract;
using MCTS2016.Optimizations.UCT;

namespace MCTS2016.SP_MCTS.Optimizations.UCT
{
    public class Opt_SP_UCTTreeNodeCreator : ISPTreeNodeCreator
    {
        private double const_C;
        private double const_D;
        private MersenneTwister rnd;
        private bool ucb1Tuned;
        private bool rave;
        private bool nodeRecycling;
        private int raveThreshold;

        public bool NodeRecycling
        {
            get => nodeRecycling;
            set => nodeRecycling = value;
        }

        public Opt_SP_UCTTreeNodeCreator(double constant, double const_D, MersenneTwister rng, bool ucb1Tuned, bool rave, int raveThreshold, bool nodeRecycling)
        {
            rnd = rng;
            this.const_C = constant;
            this.const_D = const_D;
            this.ucb1Tuned = ucb1Tuned;
            this.rave = rave;
            this.nodeRecycling = nodeRecycling;
            this.raveThreshold = raveThreshold;
        }

        public ISPTreeNode GenRootNode(IPuzzleState rootState)
        {
            return new Opt_SP_UCTTreeNode(null, null, rootState, rnd, ucb1Tuned, rave, raveThreshold, nodeRecycling, const_C, const_D, true);
        }

        public override string ToString()
        {
            return "SP_UCT-C" + const_C+"-D"+const_D;
        }
    }
}
