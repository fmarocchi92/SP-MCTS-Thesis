using MCTS2016.Common.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.SP_MCTS
{
    public interface ISPTreeNode
    {
        ISPTreeNode AddChild(IPuzzleMove move, IPuzzleState state);

        string ChildrenToString();

        IPuzzleMove GetBestMove();

        List<IPuzzleMove> GetUntriedMoves();

        void RemoveUntriedMove(IPuzzleMove move);

        bool HasChildren();

        bool HasMovesToTry();

        IPuzzleMove Move { get; }

        ISPTreeNode Parent { get; }

        ISPTreeNode SelectChild();

        IPuzzleMove SelectUntriedMove();

        string ToString();

        string TreeToString(int indent);

        void Update(double result);

        void RemoveChild(ISPTreeNode child);
    }
}
