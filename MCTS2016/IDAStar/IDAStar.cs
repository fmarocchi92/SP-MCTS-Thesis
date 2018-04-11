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
        double bestScore;

        public int NodeCount { get => nodeCount; set => nodeCount = value; }

        public List<IPuzzleMove> Solve(IPuzzleState rootState, int maxNodes, int tableSize, int maxDepth)
        {
            this.maxDepth = maxDepth;
            this.maxNodes = maxNodes;
            nodeCount = 0;
            firstLevelTable = new TranspositionTable(tableSize);
            secondLevelTable = new TranspositionTable(tableSize);
            double threshold = rootState.GetHeuristicEvaluation();
            result = null;
            bestScore = double.MaxValue;
            double value = 1;
            double lastIterationNodeCount = 0;
            AStarNode rootNode = new AStarNode(rootState, null, null);
            while (value > RESULT)
            {
                lastIterationNodeCount = nodeCount;
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
                if(threshold == double.MaxValue)
                {
                    //Debug.WriteLine("Unsolvable");
                    value = NOT_FOUND;
                }
            }
            //SinglePlayerMCTSMain.Log("Last iteration nodes: " + (nodeCount - lastIterationNodeCount));
            return BuildSolution(result);
        }

        private double Search(AStarNode node, double cost, double threshold)
        {
            double h = node.state.GetHeuristicEvaluation();
            if(h < bestScore && cost > 0)
            {
                bestScore = h;
                result = node;
            }
            nodeCount++;
            if (nodeCount > maxNodes || cost > maxDepth)
            {
                return NOT_FOUND;
            }
            double value = cost + h;
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
            if(moves.Count == 0)
            {                
                //Debug.WriteLine(node.state);
            }
            List<AStarNode> childNodes = new List<AStarNode>();
            foreach(IPuzzleMove move in moves)
            {
                IPuzzleState clone = node.state.Clone();
                clone.DoMove(move);
                childNodes.Add(new AStarNode(clone, move, node));
            }
            childNodes.Sort((x, y) => (x.state.GetHeuristicEvaluation().CompareTo(y.state.GetHeuristicEvaluation())));
            foreach(AStarNode childNode in childNodes)
            {
                IPuzzleMove move = childNode.move;
                IPuzzleState clone = childNode.state;
                //Debug.WriteLine(clone.PrettyPrint());
                //clone.DoMove(move);
                //if (((SokobanPushMove)move).IsGoalMacro)
                //{
                //    Debug.WriteLine("GOAL_MACRO:");
                //    Debug.WriteLine(node.state);
                //    Debug.WriteLine(move);
                //    Debug.WriteLine(clone);
                //}
                int childHash = clone.GetHashCode();
                //Debug.WriteLine(move);
                //Debug.WriteLine(clone.PrettyPrint());
                entry = RetrieveFromTables(childHash);
                if (entry != null && entry.Visited)//loop
                    continue;

                //if (!visited.Contains(clone))
                //visited.Add(clone);
                int childDepth = (int)(threshold - (cost + move.GetCost()));
                if (entry != null && childDepth <= entry.Depth)
                {
                    value = entry.Score;
                }
                else
                {
                    StoreInTranspositionTable(childHash, clone.GetHeuristicEvaluation() + cost + move.GetCost(), childDepth, true);
                    value = Search(childNode, cost + move.GetCost(), threshold);
                }
                if (value == RESULT)
                {
                    return RESULT;
                }
                if(value < minValue && value > threshold)
                {
                    minValue = value;
                }
            }

            entry = RetrieveFromTables(currentHash);
            int depth = (int)(threshold - (cost));
            if (entry == null /*|| minValue < double.MaxValue*/) //TODO deadlock backpropagation
            {//valid state
                StoreInTranspositionTable(currentHash, minValue, depth, false);
            }
            else
            {
                if(depth >= entry.Depth){
                    entry.Score = minValue;
                    entry.Depth = depth;
                }
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
            if (entry != null)
            {
                //for(int x = 0; x < state.GetBoard().Count; x++)
                //{
                //    if(state.GetBoard()[x] != entry.state.GetBoard()[x])
                //    {
                //        Debug.WriteLine("Entry in table:\n" + entry.state);
                //        Debug.WriteLine("state:\n" + state+"\n\n");
                //        break;
                //    }
                //}
                
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
