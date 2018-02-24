using System;
using Common.Abstract;
using System.Collections.Generic;

namespace Common.Abstract
{
    public interface ISimulationStrategy
    {
        IGameMove selectMove(IGameState gameState);

        string getTypeName();

        string getFriendlyName();
    }
}

