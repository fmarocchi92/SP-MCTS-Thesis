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
        PositiveBM
    }

    enum SimulationType
    {
        Random,
        E_Greedy
    }
}
