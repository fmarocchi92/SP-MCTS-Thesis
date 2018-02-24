using System;
using Common.Abstract;

namespace BoardGames
{
    public class TicTacToeGameMove : IGameMove
    {
        private int x, y;

        public int GetX
        {
            get { return x; }
        }

        public int GetY
        {
            get { return y; }
        }

        public TicTacToeGameMove(IGameMove move, IGameState gameState)
        {
            this.x = move.move % gameState.size;
            this.y = move.move / gameState.size;
            this.move = x * gameState.size + y;
            //this.x = move.move / gameState.size - (move.move % gameState.size == 0 ? 1 : 0);
            //this.y = (move.move % gameState.size == 0 ? gameState.size : (gameState.size >= move.move ? move.move : move.move % gameState.size)) - 1;
            //this.y = this.y < 0 ? 0 : this.y;
        }

        public TicTacToeGameMove(int x, int y, IGameState gameState)
        {
            this.x = x;
            this.y = y;
            this.move = x * gameState.size + y;
        }

        public override bool Equals(object obj)
        {
            if (obj is TicTacToeGameMove)
            {
                TicTacToeGameMove gMove = obj as TicTacToeGameMove;
                return gMove.x == x && gMove.y == y;
            }
            return false;
        }

        public override int GetHashCode()
        {
            return 1000 * x + y;
        }

        public override string ToString()
        {
            return "( " + x + "," + y + " )";
        }
    }
}

