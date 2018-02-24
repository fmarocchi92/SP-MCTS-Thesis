using MCTS2016.SP_MCTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCTS2016.Common.Abstract;
using Common;

namespace MCTS2016.Puzzles.Sokoban
{
    public class SokobanInertiaStrategy : ISPSimulationStrategy
    {

        private MersenneTwister rnd;
        private double inertiaProbability;
        public SokobanInertiaStrategy(MersenneTwister rnd, double inertiaProbability)
        {
            this.rnd = rnd;
            this.inertiaProbability = inertiaProbability;
        }

        public string getFriendlyName()
        {
            throw new NotImplementedException();
        }

        public string getTypeName()
        {
            throw new NotImplementedException();
        }

        public IPuzzleMove selectMove(IPuzzleState gameState)
        {
            
            if (rnd.NextDouble() < inertiaProbability)
            {
                List<IPuzzleMove> moves = gameState.GetMoves();
                SokobanGameState state = (SokobanGameState)gameState;
                foreach (IPuzzleMove m in moves)
                {
                    SokobanPushMove push = (SokobanPushMove)m;
                    if (push.MoveList.Count() == 0)
                    {
                        return m;
                    }
                }
            }
            return gameState.GetRandomMove();
        }
    }
}
