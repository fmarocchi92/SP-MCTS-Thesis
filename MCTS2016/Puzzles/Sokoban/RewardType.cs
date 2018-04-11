using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Puzzles.Sokoban
{
    enum RewardType
    {
        R0,
        NegativeBM,
        LogBM,
        InverseBM,
        PositiveBM,
        Boxes
    }

    enum SimulationType
    {
        Random,
        EpsilonGreedy,
        IDAstar
    }
}
