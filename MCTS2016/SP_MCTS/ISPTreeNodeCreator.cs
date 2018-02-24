using MCTS2016.Common.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.SP_MCTS
{
    public interface ISPTreeNodeCreator
    {
        ISPTreeNode GenRootNode(IPuzzleState rootState);
    }
}
