using Common.Abstract;
using MCTS2016.Common.Abstract;
using MCTS2016.SP_MCTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Puzzles.SameGame
{
    class SamegameRandomStrategy : ISPSimulationStrategy
    {
        public string getFriendlyName()
        {
            return "Samegame Random";
        }

        public string getTypeName()
        {
            return GetType().Name;
        }

        public IPuzzleMove selectMove(IPuzzleState gameState)
        {
            return gameState.GetRandomMove();
        }
    }
}
