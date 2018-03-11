using MCTS2016.Puzzles.SameGame;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Puzzles.Sokoban
{
    public class GoalMacroTree
    {

        GoalMacroNode[] roots;

        // declare [JsonIgnore] when a member should not be de/serialized
        public GoalMacroNode[] Roots { get => roots; set => roots = value; }
    }


    public class GoalMacroNode
    {
        int[] stonesPosition;
        int hashkey;
        GoalMacroEntry[] entries;

        public int[] StonesPosition { get => stonesPosition; set => stonesPosition = value; }
        public int Hashkey { get => hashkey; set => hashkey = value; }
        public GoalMacroEntry[] Entries { get => entries; set => entries = value; }

        public List<Position> GetBoxPositions()
        {
            List<Position> boxes = new List<Position>();
            for (int i = 0; i < StonesPosition.Length; i++)
            {
                if(StonesPosition[i] == 1)
                {
                    boxes.Add(new Position(i / 16, (15 - i % 16)));
                }
            }
            return boxes;
        }
    }

    public class GoalMacroEntry
    {
        int goalPosition;
        int entrancePosition;
        GoalMacroNode next;

        public int GoalPosition { get => goalPosition; set => goalPosition = value; }
        public int EntrancePosition { get => entrancePosition; set => entrancePosition = value; }
        public GoalMacroNode Next { get => next; set => next = value; }

        public Position GetGoalPosition()
        {
            return new Position(GoalPosition / 16 , (15 - GoalPosition % 16));
        }
        public Position GetEntrancePosition()
        {
            return new Position(entrancePosition / 16, (15 - entrancePosition % 16));
        }
        
    }
}
