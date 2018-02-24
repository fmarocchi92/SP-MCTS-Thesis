using System;

namespace Common.Abstract
{
    using System.Collections.Generic;
    using System.Linq;

    public class IGameMove
    {
        public int move;

        public IGameMove()
        {
        }

        public IGameMove(int move)
        {
            this.move = move;
        }

        public static implicit operator int(IGameMove move)
        {
            return move.move;
        }

        public static explicit operator IGameMove(int move)
        {
            return new IGameMove(move);
        }

        public static List<int> toIntMoves(List<IGameMove> moves)
        {
            return moves.Select((IGameMove move) =>
                {
                    return move.move;
                }).ToList();
        }

        public static List<IGameMove> toIGameMoves(List<int> moves)
        {
            return moves.Select((int move) =>
                {
                    return new IGameMove(move);
                }).ToList();
        }
    }
}

