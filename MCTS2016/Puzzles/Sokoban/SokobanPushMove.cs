using MCTS2016.Common.Abstract;
using MCTS2016.Puzzles.SameGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Puzzles.Sokoban
{
    public class SokobanPushMove : IPuzzleMove
    {
        private Position playerPosition;
        private int boxIndex;

        private List<SokobanGameMove> moveList;

        private bool isGoalMacro;

        internal List<SokobanGameMove> MoveList { get => moveList; set => moveList = value; }
        internal Position PlayerPosition { get => playerPosition; set => playerPosition = value; }
        public bool IsGoalMacro { get => isGoalMacro; set => isGoalMacro = value; }

        public SokobanPushMove(List<SokobanGameMove> moves, Position playerPosition, int boxIndex)
        {
            this.boxIndex = boxIndex;
            MoveList = moves;
            PlayerPosition = playerPosition;
            isGoalMacro = false;
        }

        //public SokobanPushMove(List<IPuzzleMove> moves, Position position, int boxIndex)
        //{

        //    this.position = position;
        //    this.boxIndex = boxIndex;
        //}

        public override double GetCost()
        {
            int pushCount = 0;
            for(int i=moveList.Count-1; i >= 0; i--)
            {
                if(moveList[i]>3)
                    pushCount++;
                else
                {
                    //break;
                }
            }
            return pushCount;
        }

        public override string ToString()
        {
            string s = "";
            foreach(SokobanGameMove m in moveList)
            {
                s += m;
            }
            return playerPosition+"[box:"+boxIndex+"]:"+ s;
        }

        public override bool Equals(object obj)
        {
            var move = obj as SokobanPushMove;
            return move != null && GetHashCode() == obj.GetHashCode();
        }

        public override int GetHashCode()
        {
            var hashCode = 808966213;
            hashCode = hashCode * -1521134295 + playerPosition.GetHashCode();
            hashCode = hashCode * -1521134295 + boxIndex.GetHashCode();
            hashCode = hashCode * -1521134295 + moveList.Last<SokobanGameMove>().GetHashCode();
            return hashCode;
        }
    }
}
