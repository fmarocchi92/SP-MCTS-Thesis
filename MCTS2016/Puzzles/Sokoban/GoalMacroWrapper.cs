using MCTS2016.Puzzles.SameGame;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Puzzles.Sokoban
{    
    class GoalMacroWrapper
    {
        [DllImport("GoalMacros.dll", EntryPoint ="GetMacros", CharSet =CharSet.Ansi, CallingConvention =CallingConvention.Cdecl)]
        [return: MarshalAs(UnmanagedType.LPStr)]
        public static extern string GetMacros(string filepath);

        public static GoalMacroTree MacroTree()
        {
            string s = GetMacros("../Release/Levels/tmp.txt");
            GoalMacroTree tree = JsonConvert.DeserializeObject<GoalMacroTree>(s);
            GoalMacroNode node = null;
            for (int j=0;j< tree.Roots.Length;j++)
            {
                node = tree.Roots[j];
                while (node.Entries.Length > 0)
                {
                    s = "";
                    for (int i = 0; i < node.StonesPosition.Length; i++)
                    {
                        if (i % 16 == 0 && i != 0)
                        {
                            s += "\n";
                        }
                        if (i == node.Entries[0].GoalPosition)
                            s += "x";
                        else if (i == node.Entries[0].EntrancePosition)
                            s += "-";
                        else
                            s += node.StonesPosition[i];
                        
                    }
                    Debug.WriteLine(s);
                    Debug.WriteLine("GoalPosition: "+node.Entries[0].GetGoalPosition());
                    Debug.WriteLine("EntrancePosition: " + node.Entries[0].GetEntrancePosition());
                    List<Position> boxes = node.GetBoxPositions();
                    foreach (Position box in boxes)
                        Debug.WriteLine("Box in "+box);
                    node = node.Entries[0].Next;
                    Debug.WriteLine("\n\n");
                }   
            }
            return tree;
        }
    }
}
