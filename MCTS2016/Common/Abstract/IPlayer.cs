using System;

namespace Common.Abstract
{
    public interface IPlayer
    {
        int ComputeMove(IGameState state);
    }
}

