using MCTS2016.Puzzles.SameGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.BFS
{
    class BFSNodePosition
    {
        public Position position;
        public int distance;

        public BFSNodePosition(Position position, int distance)
        {
            this.position = position;
            this.distance = distance;
        }
    }
}
