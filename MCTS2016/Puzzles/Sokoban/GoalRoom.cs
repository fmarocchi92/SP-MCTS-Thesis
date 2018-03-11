using MCTS2016.Puzzles.SameGame;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Puzzles.Sokoban
{
    class GoalRoom
    {
        List<Position> goals;
        List<Position> entrances;
        List<Position> squares;
        int roomIndex;

        public List<Position> Goals { get => goals; set => goals = value; }
        public List<Position> Entrances { get => entrances; set => entrances = value; }
        public List<Position> Squares { get => squares; set => squares = value; }
        public int RoomIndex { get => roomIndex; set => roomIndex = value; }

        public GoalRoom(List<Position> goals, int roomIndex)
        {
            this.goals = goals;
            this.roomIndex = roomIndex;
            squares = new List<Position>(goals);
        }
    }
}
