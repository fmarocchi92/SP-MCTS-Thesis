using Common.Abstract;
using MCTS2016.Common.Abstract;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Puzzles.Sokoban
{
    class SokobanGameMove : IPuzzleMove
    {
        private string moveString;

        public SokobanGameMove(string moveString)
        {
            switch (moveString)
            {
                case "u":
                    move = 0;
                    break;
                case "r":
                    move = 1;
                    break;
                case "d":
                    move = 2;
                    break;
                case "l":
                    move = 3;
                    break;
                case "U":
                    move = 4;
                    break;
                case "R":
                    move = 5;
                    break;
                case "D":
                    move = 6;
                    break;
                case "L":
                    move = 7;
                    break;
            }
            this.moveString = moveString;
        }

        public SokobanGameMove(int move)
        {
            switch (move)
            {
                case 0:
                    moveString = "u";
                    break;
                case 1:
                    moveString = "r";
                    break;
                case 2:
                    moveString = "d";
                    break;
                case 3:
                    moveString = "l";
                    break;
                case 4:
                    moveString = "U";
                    break;
                case 5:
                    moveString = "R";
                    break;
                case 6:
                    moveString = "D";
                    break;
                case 7:
                    moveString = "L";
                    break;

            }
            this.move = move;
        }

        public override bool Equals(object obj)
        {
            var move = obj as SokobanGameMove;
            return move != null &&
                   moveString == move.moveString;
        }

        public override int GetHashCode()
        {
            return -72123235 + EqualityComparer<string>.Default.GetHashCode(moveString);
        }

        public override string ToString()
        {
            return moveString;
        }


    }


}
