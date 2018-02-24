using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Common.Abstract
{
    public class IPuzzleMove
    {
        public int move;

        public IPuzzleMove()
        {
        }

        public IPuzzleMove(int move)
        {
            this.move = move;
        }

        public static implicit operator int(IPuzzleMove move)
        {
            return move.move;
        }

        public static explicit operator IPuzzleMove(int move)
        {
            return new IPuzzleMove(move);
        }

        public static List<int> toIntMoves(List<IPuzzleMove> moves)
        {
            return moves.Select((IPuzzleMove move) =>
            {
                return move.move;
            }).ToList();
        }

        public static List<IPuzzleMove> toIPuzzleMoves(List<int> moves)
        {
            return moves.Select((int move) =>
            {
                return new IPuzzleMove(move);
            }).ToList();
        }
    }
}
