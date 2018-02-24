using System;
using Common.Abstract;
using System.Collections.Generic;
using MCTS2016.SP_MCTS;

namespace Common.Abstract
{
    public interface ISP_MCTSSimulationStrategy : ISPSimulationStrategy
    {
        int iterations { get; set; }
    }
}

