using System.Collections.Generic;
using MCTS.Standard.Utils.UCT;
using Common.Abstract;

namespace MCTS.Standard.Utils.DepthLimitedUCT
{
    public class DepthLimitedUCTTreeNode : UCTTreeNode
    {
        private int maxDepth;
        private int depth;
        private IStateBestMoveEstimator stateEstimator;

        public DepthLimitedUCTTreeNode(IGameMove move, DepthLimitedUCTTreeNode parent, IGameState state, int depth, int maxDepth, IStateBestMoveEstimator stateEstimator, double constant = 1.0)
            : base(move, parent, state, constant, false)
        {
            this.maxDepth = maxDepth;
            this.depth = depth;
            this.stateEstimator = stateEstimator;
            if (depth <= maxDepth)
            {
                untriedMoves = state.GetMoves();
            }
            else
            {
                untriedMoves = new List<IGameMove>();
                if (!state.isTerminal())
                {
                    IGameMove estimatedMove = stateEstimator.EstimateMove(state);
                    state.DoMove(estimatedMove);
                    AddChild(estimatedMove, state);
                }
                else
                {
                    ITreeNode node = this;
                    while (node != null)
                    {
                        node.Update(state.GetResult(node.PlayerWhoJustMoved));
                        node = node.Parent;
                    }
                }
            }
        }

        public override ITreeNode AddChild(IGameMove move, IGameState state)
        {
            DepthLimitedUCTTreeNode n = new DepthLimitedUCTTreeNode(move, this, state, depth + 1, maxDepth, stateEstimator, constant);
            untriedMoves.Remove(move);
            childNodes.Add(n);
            return n;
        }
    }
}
