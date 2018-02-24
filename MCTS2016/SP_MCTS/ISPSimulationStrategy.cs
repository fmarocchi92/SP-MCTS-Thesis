using MCTS2016.Common.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.SP_MCTS
{
    public interface ISPSimulationStrategy
    {
        IPuzzleMove selectMove(IPuzzleState gameState);

        string getTypeName();

        string getFriendlyName();
    }
}
