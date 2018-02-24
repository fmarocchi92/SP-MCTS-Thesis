using Common.Abstract;

namespace BoardGames
{
    using System.Collections.Generic;
    using System.Linq;

    public class Forza4GameMove : IGameMove
    {
        //private int move;

        //        public int Move
        //        {
        //            get { return move; }
        //        }

        public Forza4GameMove(int move)
        {
            this.move = move;
        }

        public override bool Equals(object obj)
        {
            if (obj is Forza4GameMove)
            {
                Forza4GameMove gMove = obj as Forza4GameMove;
                return gMove.move == move;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return move;
        }

        public override string ToString()
        {
            return move.ToString();
        }

        public static implicit operator int(Forza4GameMove move)
        {
            return move.move;
        }

        public static explicit operator Forza4GameMove(int move)
        {
            return new Forza4GameMove(move);
        }
    }
}
