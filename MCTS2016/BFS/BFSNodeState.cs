using MCTS2016.Common.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.BFS
{
    public class BFSNodeState
    {
        public IPuzzleState state;
        public IPuzzleMove move;
        public BFSNodeState parent;

        public BFSNodeState(IPuzzleState state, IPuzzleMove move, BFSNodeState parent)
        {
            this.state = state;
            this.move = move;
            this.parent = parent;
        }

        internal BFSNodeState Clone()
        {
            return new BFSNodeState(state, move, parent);
        }
    }
}
