using Common.Abstract;
using MCTS2016.Common.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Puzzles.SameGame
{
    class SamegameGameMove : IPuzzleMove
    {

        public int x, y;

        public SamegameGameMove(int x, int y)
        {
            this.x = x;
            this.y = y;
            this.move = x * 1000 + y;
        }

        public override string ToString()
        {
            return "[X: " + x + "  Y: " + y+"]";
        }

        public static int GetX(int move)
        {
            return move / 1000;
        }
        public static int GetY(int move)
        {
            return move % 1000;
        }
    }
}
