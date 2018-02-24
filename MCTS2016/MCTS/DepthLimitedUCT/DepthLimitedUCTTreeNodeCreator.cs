using Common.Abstract;

namespace MCTS.Standard.Utils.DepthLimitedUCT
{
    public class DepthLimitedUCTTreeNodeCreator : ITreeNodeCreator
    {
        private double constant;
        private int maxDepth;
        private IStateBestMoveEstimator stateEstimator;

        public DepthLimitedUCTTreeNodeCreator(int maxDepth, IStateBestMoveEstimator stateEstimator, double constant = 1.0)
        {
            this.constant = constant;
            this.maxDepth = maxDepth;
            this.stateEstimator = stateEstimator;
        }

        public ITreeNode GenRootNode(IGameState rootState)
        {
            return new DepthLimitedUCTTreeNode(null, null, rootState, 0, maxDepth, stateEstimator, constant);
        }

        public override string ToString()
        {
            return "DLUCT(" + constant + "," + maxDepth + ")";
        }
    }
}
