using Common.Abstract;

namespace MCTS.Standard.Utils.UCT
{
    public class UCTTreeNodeCreator : ITreeNodeCreator
    {
        private double constant;

        public UCTTreeNodeCreator(double constant = 1.0)
        {
            this.constant = constant;
        }

        public ITreeNode GenRootNode(IGameState rootState)
        {
            return new UCTTreeNode(null, null, rootState, constant);
        }

        public override string ToString()
        {
            return "UCT" + constant;
        }
    }
}
