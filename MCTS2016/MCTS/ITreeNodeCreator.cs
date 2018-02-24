using Common.Abstract;

namespace MCTS.Standard.Utils
{
    public interface ITreeNodeCreator
    {
        ITreeNode GenRootNode(IGameState rootState);
    }
}
