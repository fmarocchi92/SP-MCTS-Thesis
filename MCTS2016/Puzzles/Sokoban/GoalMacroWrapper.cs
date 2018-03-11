using MCTS2016.Common.Abstract;
using MCTS2016.IDAStar;
using MCTS2016.Puzzles.SameGame;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Puzzles.Sokoban
{    
    class GoalMacroWrapper
    {
        [DllImport("GoalMacros.dll", EntryPoint ="GetMacros", CharSet =CharSet.Ansi, CallingConvention =CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string GetMacros(string filepath);

        static AbstractSokobanState state;
        static IDAStarSearch idaStar = new IDAStarSearch();

        public static GoalMacroTree BuildMacroTree(AbstractSokobanState state)
        {
            GoalMacroWrapper.state = state;
            File.WriteAllText("level.tmp",state.State.ToString());
            

            string s = GetMacros("level.tmp");
            GoalMacroTree tree = JsonConvert.DeserializeObject<GoalMacroTree>(s);


            //GoalMacroNode node = null;

            //for (int j=0;j< tree.Roots.Length;j++)
            //{
            //    node = tree.Roots[j];

            //    while (node.Entries.Length > 0)
            //    {
            //        s = "";
            //        for (int i = 0; i < node.StonesPosition.Length; i++)
            //        {
            //            if (i % 16 == 0 && i != 0)
            //            {
            //                s += "\n";
            //            }
            //            if (i == node.Entries[0].GoalPosition)
            //                s += "x";
            //            else if (i == node.Entries[0].EntrancePosition)
            //                s += "-";
            //            else
            //                s += node.StonesPosition[i];

            //        }
            //        Debug.WriteLine(s);
            //        Debug.WriteLine("GoalPosition: "+node.Entries[0].GetGoalPosition());
            //        Debug.WriteLine("EntrancePosition: " + node.Entries[0].GetEntrancePosition());
            //        List<Position> boxes = node.GetBoxPositions();
            //        foreach (Position box in boxes)
            //            Debug.WriteLine("Box in "+box);
            //        node = node.Entries[0].Next;
            //        Debug.WriteLine("\n\n");
            //    }   
            //}
            HashSet<Position> goalsInRoom = new HashSet<Position>();
            foreach (GoalMacroNode n in tree.Roots)
            {
                CompressTree(n, new Dictionary<int, GoalMacroNode>(), goalsInRoom);
            }
            tree.GoalsInRoom = goalsInRoom;
            return tree;
        }

        static void CompressTree(GoalMacroNode node, Dictionary<int,GoalMacroNode> map,HashSet<Position> goals)
        {
            int hash = node.ComputeHashKey();
            GoalMacroNode oldNode;
            if (map.TryGetValue(hash, out oldNode))
            {
                node = oldNode;
            }
            else
            {
                map.Add(hash, node);
                foreach (GoalMacroEntry entry in node.Entries)
                {
                    goals.Add(entry.GetGoalPosition());
                    entry.GoalMacros = BuildMacroMove(entry.GetEntrancePosition(), entry.GetGoalPosition(), node.GetBoxPositions());
                    CompressTree(entry.Next, map, goals);
                }
            }
            
        }


        static List<GoalMacro> BuildMacroMove(Position entrance, Position goal, List<Position> boxesInGoal)
        {
            List<GoalMacro> macros = new List<GoalMacro>();
            AddToMacros(macros, new Position(entrance.X + 1, entrance.Y), goal, entrance, boxesInGoal);            
            AddToMacros(macros, new Position(entrance.X - 1, entrance.Y), goal, entrance, boxesInGoal);            
            AddToMacros(macros, new Position(entrance.X, entrance.Y + 1), goal, entrance, boxesInGoal);            
            AddToMacros(macros, new Position(entrance.X, entrance.Y - 1), goal, entrance, boxesInGoal);
            return macros;
        }

        static void AddToMacros(List<GoalMacro> macros, Position position, Position goal, Position entrance, List<Position> boxesInGoal)
        {
            AbstractSokobanState clone = (AbstractSokobanState)state.Clone();
            clone.ClearBoardForGoalMacro(boxesInGoal,goal, entrance);
            
            if (state.State.Board[position.X, position.Y] == SokobanGameState.EMPTY || state.State.Board[position.X, position.Y] == SokobanGameState.GOAL)
            {
                clone.SetPlayerPosition(position);
                AbstractSokobanState clearState = new AbstractSokobanState(clone.State.ToString(), clone.RewardType, false, false, false, false, clone.SimulationStrategy, clone.Rng);
                SokobanPushMove pushMove = SolveMacro(clearState);
                if (pushMove != null)
                {
                    macros.Add(new GoalMacro(position, pushMove));
                }
            }
        }

        static SokobanPushMove SolveMacro(AbstractSokobanState s)
        {
            s.UseGoalCut = false;
            s.UseGoalMacro = false;
            s.UseTunnelMacro = false;
            List<IPuzzleMove> solution = idaStar.Solve(s, 10000, 10000, 700);
            if(solution.Count == 0)
            {
                return null;
            }
            List<SokobanGameMove> moveList = new List<SokobanGameMove>();
            foreach(SokobanPushMove push in solution)
            {
                moveList.AddRange(push.MoveList);
            }
            return new SokobanPushMove(moveList, new Position(s.State.PlayerX, s.State.PlayerY), moveList[solution.Count-1].BoxIndex);
        }
    }
}
