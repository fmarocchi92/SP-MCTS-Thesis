using MCTS2016.Common.Abstract;
using MCTS2016.Puzzles.Sokoban;
using MCTS2016.SP_MCTS.Optimizations.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.IDAStar
{
    class IDAStarSearch
    {
        AStarNode result;
        List<IPuzzleState> visited;
        TranspositionTable firstLevelTable;
        TranspositionTable secondLevelTable;
        double RESULT = 0;
        double NOT_FOUND = -1;
        int nodeCount;
        int maxDepth;
        int maxNodes;

        public List<IPuzzleMove> Solve(IPuzzleState rootState, int maxNodes, int tableSize, int maxDepth)
        {
            this.maxDepth = maxDepth;
            this.maxNodes = maxNodes;
            nodeCount = 0;
            firstLevelTable = new TranspositionTable(tableSize);
            secondLevelTable = new TranspositionTable(tableSize);
            double threshold = rootState.GetResult();
            result = null;
            double value = 1;
            AStarNode rootNode = new AStarNode(rootState, null, null);
            while (value > RESULT)
            {
                visited = new List<IPuzzleState>() { rootState.Clone() };
                value = Search(rootNode, 0, threshold);
                if (value != RESULT && value > threshold)
                {
                    threshold = value;
                }
                if(nodeCount > maxNodes)
                {
                    value = NOT_FOUND;
                }
            }
            return BuildSolution(result);
        }

        private double Search(AStarNode node, double cost, double threshold)
        {
            nodeCount++;
            if (nodeCount > maxNodes)
            {
                return NOT_FOUND;
            }
            double value = cost + node.state.GetResult();
            int currentHash = node.state.GetHashCode();
            TranspositionTableEntry entry;
            if (node.state.EndState())
            {
                result = node;
                return RESULT;
            }
            if (value > threshold)
            {
                entry = RetrieveFromTables(currentHash);
                if (entry != null)
                {
                    entry.Visited = false;
                }
                //visited.RemoveAt(visited.Count() - 1);
                return value;
            }
            double minValue = double.MaxValue;
            List<IPuzzleMove> moves = node.state.GetMoves();
            foreach(IPuzzleMove move in moves)
            {
                IPuzzleState clone = node.state.Clone();
                //Debug.WriteLine(clone.PrettyPrint());
                clone.DoMove(move);
                currentHash = clone.GetHashCode();
                //Debug.WriteLine(move);
                //Debug.WriteLine(clone.PrettyPrint());
                entry = RetrieveFromTables(currentHash);
                if (entry != null && entry.Visited)//loop
                    continue;
                
                //if (!visited.Contains(clone))
                    //visited.Add(clone);

                if (entry != null)
                {
                    if (threshold - cost <= entry.Depth)
                    {
                        return entry.Score;
                    }
                }
                StoreInTranspositionTable(currentHash, clone.GetResult() + cost + move.GetCost(), (int)(threshold - (cost + move.GetCost())), true);
                value = Search(new AStarNode(clone, move, node), cost + move.GetCost(), threshold);
                
                if (value == RESULT)
                {
                    return RESULT;
                }
                if(value < minValue && value > threshold)
                {
                    minValue = value;
                }
            }

            entry = RetrieveFromTables(node.state.GetHashCode());
            if (entry == null /*|| minValue < double.MaxValue*/) //TODO deadlock backpropagation
            {//valid state
                StoreInTranspositionTable(currentHash, minValue, (int)(threshold - (cost)), false);
            }
            else
            {
                entry.Visited = false;
            }
            return minValue;
        }

        List<IPuzzleMove> BuildSolution(AStarNode node)
        {
            List<IPuzzleMove> solution = new List<IPuzzleMove>();
            if (node == null)
            {
                return solution;
            }
            while(node.parent!= null)
            {
                solution.Add(node.move);
                node = node.parent;
            }
            solution.Reverse();
            return solution;
        }

        TranspositionTableEntry RetrieveFromTables(int hashkey)
        {
            TranspositionTableEntry entry = firstLevelTable.Retrieve(hashkey);
            if (entry == null)
            {
                entry = secondLevelTable.Retrieve(hashkey);
            }
            return entry;
        }

        void StoreInTranspositionTable(int hashKey, double score, int depth, bool visited)
        {
            TranspositionTableEntry entry = firstLevelTable.GetAnyEntryWithIndex(hashKey);
            TranspositionTableEntry newEntry = new TranspositionTableEntry(hashKey, score, depth, visited);
            if (entry == null)
            {
                firstLevelTable.Store(newEntry);
            }
            else if (depth > entry.Depth)
            {
                firstLevelTable.Store(newEntry);
                secondLevelTable.Store(entry);
            }
            else
            {
                secondLevelTable.Store(newEntry);
            }
        }
    }
}
