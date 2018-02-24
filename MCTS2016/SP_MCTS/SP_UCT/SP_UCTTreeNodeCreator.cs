using Common;
using Common.Abstract;
using MCTS.Standard.Utils;
using MCTS2016.Common.Abstract;
using MCTS2016.SP_MCTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MCTS2016.SP_MCTS.SP_UCT
{
    public class SP_UCTTreeNodeCreator : ISPTreeNodeCreator
    {
        private double const_C;
        private double const_D;
        private MersenneTwister rnd;

        public SP_UCTTreeNodeCreator(double constant, double const_D, MersenneTwister rng)
        {
            rnd = rng;
            this.const_C = constant;
            this.const_D = const_D;
        }

        public ISPTreeNode GenRootNode(IPuzzleState rootState)
        {
            return new SP_UCTTreeNode(null, null, rootState, rnd,const_C,const_D,true);
        }

        public override string ToString()
        {
            return "SP_UCT-C" + const_C+"-D"+const_D;
        }
    }
}
