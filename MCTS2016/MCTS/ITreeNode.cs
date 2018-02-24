using Common.Abstract;

namespace MCTS.Standard.Utils
{
    public interface ITreeNode
    {
        ITreeNode AddChild(IGameMove move, IGameState state);

        string ChildrenToString();

        IGameMove GetBestMove();

        bool HasChildren();

        bool HasMovesToTry();

        IGameMove Move { get; }

        ITreeNode Parent { get; }

        int PlayerWhoJustMoved { get; }

        ITreeNode SelectChild();

        IGameMove SelectUntriedMove();

        string ToString();

        string TreeToString(int indent);

        void Update(double result);
    }
}
