using MCTS2016.Common.Abstract;
using MCTS2016.Puzzles.SameGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Puzzles.Sokoban
{
    class SokobanPushMove : IPuzzleMove
    {
        private Position playerPosition;

        private SokobanGameMove pushMove;

        private List<SokobanGameMove> moveList;

        internal List<SokobanGameMove> MoveList { get => moveList; set => moveList = value; }
        internal Position PlayerPosition { get => playerPosition; set => playerPosition = value; }

        public SokobanPushMove(List<SokobanGameMove> moves, Position playerPosition)
        {
            MoveList = moves;
            PlayerPosition = playerPosition;
        }

        public override string ToString()
        {
            string s = "";
            foreach(SokobanGameMove m in moveList)
            {
                s += m;
            }
            return playerPosition+":"+ s;
        }
    }
}
