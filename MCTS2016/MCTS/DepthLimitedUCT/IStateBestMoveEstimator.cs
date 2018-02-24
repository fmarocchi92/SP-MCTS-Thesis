using Common.Abstract;

namespace MCTS.Standard.Utils.DepthLimitedUCT
{
    public interface IStateBestMoveEstimator
    {
        IGameMove EstimateMove(IGameState state);
    }
}
