using MCTS2016.Common.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.IDAStar
{
    class AStarNode
    {
        public IPuzzleState state;
        public IPuzzleMove move;
        public AStarNode parent;
        public double cost;

        public AStarNode(IPuzzleState state, IPuzzleMove move, AStarNode parent)
        {
            this.state = state;
            this.move = move;
            this.parent = parent;
        }

        internal AStarNode Clone()
        {
            return new AStarNode(state, move, parent);
        }
    }
}
