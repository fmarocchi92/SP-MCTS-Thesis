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

        [JsonIgnore]
        public HashSet<Position> GoalsInRoom { get; set; }

        public GoalMacroNode[] Roots { get => roots; set => roots = value; }
    }


    public class GoalMacroNode
    {
        int[] stonesPosition;
        int hashkey;
        GoalMacroEntry[] entries;
        List<Position> getBoxPositions;

        public int[] StonesPosition {set => stonesPosition = value; }
        public int Hashkey { get => hashkey; set => hashkey = value; }
        public GoalMacroEntry[] Entries { get => entries; set => entries = value; }

        [JsonIgnore]
        public List<Position> GetBoxPositions { get => getBoxPositions; set => getBoxPositions = value; }

        public List<Position> ComputeBoxPositions()
        {
            List<Position> boxes = new List<Position>();
            for (int i = 0; i < stonesPosition.Length; i++)
            {
                if(stonesPosition[i] == 1)
                {
                    boxes.Add(new Position(i / 16, (15 - i % 16)));
                }
            }
            boxes.Sort((x, y) => (x.X + 1000 * x.Y).CompareTo(y.X + 1000 * y.Y));
            GetBoxPositions = boxes;
            return boxes;
        }

        public int ComputeHashKey()
        {
            int hashkey = 27;
            
            foreach(Position box in GetBoxPositions)
            {
                hashkey = (hashkey * 13) + box.GetHashCode();
            }
            this.hashkey = hashkey;
            return hashkey;
        }
    }

    public class GoalMacroEntry
    {
        int goalPosition;
        int entrancePosition;
        Position getGoalPosition;
        Position getEntrancePosition;
        GoalMacroNode next;

        [JsonIgnore]
        public List<GoalMacro> GoalMacros { get; set; }

        public int GoalPosition {set => goalPosition = value; }
        public int EntrancePosition {set => entrancePosition = value; }
        public GoalMacroNode Next { get => next; set => next = value; }

        public Position GetGoalPosition { get => getGoalPosition; set => getGoalPosition = value; }
        public Position GetEntrancePosition { get => getEntrancePosition; set => getEntrancePosition = value; }

        public void ComputeGoalPosition()
        {
            GetGoalPosition = new Position(goalPosition / 16, 15 - goalPosition % 16);
        }
        public void ComputeEntrancePosition()
        {
            GetEntrancePosition = new Position(entrancePosition / 16, 15 - entrancePosition % 16);
        }
    }

    public class GoalMacro
    {
        public Position PlayerPosition { get; set; }
        public SokobanPushMove MacroMove { get; set; }

        public GoalMacro(Position playerPosition, SokobanPushMove macroMove)
        {
            PlayerPosition = playerPosition;
            MacroMove = macroMove;
        }
    }
}
