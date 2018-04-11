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

        private List<Position> blocks;
        private int color;

        public List<Position> Blocks { get => blocks;}

        public int X { get => Blocks[0].X; }
        public int Y { get => Blocks[0].Y; }
        public Position Position { get => Blocks[0]; }

        public SamegameGameMove(List<Position> blocks, int color)
        {
            this.color = color;
            this.blocks = blocks;
            if(blocks.Count == 0)
            {
                this.move = 0;
            }
            else
            {
                this.move = blocks[0].X * 1000 + blocks[0].Y;
            }
        }

        public override string ToString()
        {
            string s = "{";
            foreach(Position p in blocks)
            {
                s += p;
            }
            return s+"}";
            //return "[X: " + blocks[0].X + "  Y: " + blocks[0].Y +"]";
        }

        public static int GetX(int move)
        {
            return move / 1000;
        }
        public static int GetY(int move)
        {
            return move % 1000;
        }

        public override bool Equals(object obj)
        {
            var move = obj as SamegameGameMove;
            return move != null && GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            int hc = 27;
            if (blocks != null)
            {
                foreach (Position p in blocks)
                {
                    hc = (13 * hc) + p.GetHashCode();
                }
            }
            hc = (13 * hc) + color.GetHashCode();
            return hc;
        }

        public override double GetCost()
        {
            if(Blocks.Count == 2)
            {
                return 1;
            }
            return 1/((Blocks.Count - 2)*(Blocks.Count-2));
        }
    }
}
