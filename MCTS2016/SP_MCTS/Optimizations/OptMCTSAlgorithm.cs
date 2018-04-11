//#define PROFILING

using System;
using System.Collections.Generic;
using Common.Abstract;
using MCTS.Standard.Utils;
using MCTS2016.Common.Abstract;
using MCTS2016.Optimizations.UCT;
using MCTS2016.SP_MCTS.Optimizations.UCT;
using MCTS2016.SP_MCTS.Optimizations.Utils;
using Common;
using System.Diagnostics;
using MCTS2016.Puzzles.Sokoban;

namespace MCTS2016.SP_MCTS.Optimizations
{
    public class OptMCTSAlgorithm
    {
        private ISPTreeNodeCreator treeCreator;
        private bool search = true;
        private bool showMemoryUsage = false;
        private ObjectPool objectPool;
        private int memoryBudget;
        
        private List<IPuzzleMove> bestRollout;
        private double topScore = double.MinValue;
        private bool stopOnResult;
        private bool avoidCycles;
        private int iterationsExecuted;
        private bool useNodeElimination;
        private int iterationsForFirstSolution;
        private int solutionCount;
        private int solutionHash;
        private HashSet<int> solutionHashes;
        private int nodeCount;
        public List<int> visits = new List<int>();
        public List<int> raveVisits = new List<int>();
        public int maxDepth;
        public int nodesEliminated;
        public int nodesNotExpanded;

        public OptMCTSAlgorithm(ISPTreeNodeCreator treeCreator, int iterations, int memoryBudget, bool stopOnResult, bool avoidCycles, bool useNodeElimination)
        {
            this.treeCreator = treeCreator;
            this.stopOnResult = stopOnResult;
            this.memoryBudget = memoryBudget;
            bool nodeRecycling = ((Opt_SP_UCTTreeNodeCreator)treeCreator).NodeRecycling;
            this.avoidCycles = avoidCycles;
            this.useNodeElimination = useNodeElimination;
            objectPool = new ObjectPool(iterations, iterations, nodeRecycling, this.memoryBudget);
        }
        
        public List<IPuzzleMove> Solve(IPuzzleState rootState, int iterations, double maxTimeInMinutes = 5)
        {
            topScore = double.MinValue;
            bestRollout = null;
            IPuzzleMove bestMove = Search(rootState, iterations, maxTimeInMinutes);
            List<IPuzzleMove> moves = new List<IPuzzleMove>() { bestMove };
            moves.AddRange(bestRollout);
            return moves;
        }

        public IPuzzleMove Search(IPuzzleState rootState, int iterations, double maxTimeInMinutes = 5)
        {
            IterationsForFirstSolution = -1;
            nodeCount = 0;
            nodesEliminated = 0;
            nodesNotExpanded = 0;
            bool looped;
            if (!search)
            {
                search = true;
            }
            double minReward = double.MaxValue;
            double maxReward = double.MinValue;
            maxDepth = 0;
            // If needed clean the pool, restore all objects in the pool to the initial value
            if (objectPool.NeedToClean)
            {
                objectPool.CleanObjectPool();
            }
            
            ISPTreeNode rootNode = treeCreator.GenRootNode(rootState);
            ISPTreeNode head = null;
            ISPTreeNode tail = null;
            
            HashSet<IPuzzleMove> allFirstMoves = new HashSet<IPuzzleMove>();
            List<IPuzzleMove> currentRollout = new List<IPuzzleMove>();
            solutionHashes = new HashSet<int>();

#if PROFILING
                long beforeMemory = GC.GetTotalMemory(false);
                long afterMemory = GC.GetTotalMemory(false);
                long usedMemory = afterMemory - beforeMemory;
                long averageUsedMemoryPerIteration = 0;
#endif
            int deadlocksInTree = 0;
            int currentDepth = 0;
            for (iterationsExecuted = 0; iterationsExecuted < iterations; iterationsExecuted++)
            {
                
                looped = false;
                ISPTreeNode node = rootNode;
                IPuzzleState state = rootState.Clone();
                //Debug.WriteLine(node.TreeToString(0));
                HashSet<IPuzzleState> visitedStatesInRollout = new HashSet<IPuzzleState>() { state.Clone() };

                // Clear lists of moves used for RAVE updates && best rollout
                solutionHash = 27;
                currentRollout = new List<IPuzzleMove>();
                allFirstMoves.Clear();

                // Select
                while (!node.HasMovesToTry() && node.HasChildren())
                {
                    // UCB1-Tuned and RAVE Optimizations
                    node = node.SelectChild();
                    state.DoMove(node.Move);
                    visitedStatesInRollout.Add(state.Clone());
                    // RAVE Optimization && best rollout
                    currentRollout.Add(node.Move);
                    UpdateSolutionHash(node.Move);
                    allFirstMoves.Add(node.Move);

                    // Node Recycling Optimization
                    if (((Opt_SP_UCTTreeNode)node).NodeRecycling)
                    {
                        // Non-leaf node removed from LRU queue during playout
                        if (node.NextLRUElem != null && node.PrevLRUElem != null)
                        {
                            LRUQueueManager.LRURemoveElement(ref node, ref head, ref tail);                       
                        }
                    }
                }
                IPuzzleState backupState = state.Clone();

                if(!node.HasChildren() && !node.HasMovesToTry())
                {
                    deadlocksInTree++;
                }
                else
                {
                    Debug.Write("");
                }

                // Expand
                if (node.HasMovesToTry())
                {
                    IPuzzleMove move = node.SelectUntriedMove();
                    if (move != -1)
                    {
                        state.DoMove(move);
                        
                        // Node Recycling Optimization
                        if (((Opt_SP_UCTTreeNode)node).NodeRecycling)
                        {
                            if (memoryBudget == nodeCount && head != null)
                            {
                                head.ChildRecycle();
                                nodeCount--;
                                // Change LRU queue head when it becomes a leaf node
                                if (!head.HasChildren())
                                {
                                    LRUQueueManager.LRURemoveFirst(ref head, ref tail);
                                }
                            }
                        }

                        if (visitedStatesInRollout.Contains(state))
                        {
                            if (avoidCycles)
                            {
                                while (node.GetUntriedMoves().Count > 0 && visitedStatesInRollout.Contains(state))
                                {
                                    state = backupState.Clone();
                                    move = node.GetUntriedMoves()[RNG.Next(node.GetUntriedMoves().Count)];
                                    state.DoMove(move);
                                    node.RemoveUntriedMove(move);
                                }
                                if (!visitedStatesInRollout.Contains(state)) //found valid move
                                {
                                    node = node.AddChild(objectPool, move, state);
                                    UpdateSolutionHash(move);
                                    currentRollout.Add(move);
                                    allFirstMoves.Add(move);
                                    nodeCount++;
                                }
                                else //all moves visited
                                {
                                    nodesNotExpanded++;
                                    state = backupState;
                                }
                            }
                            else
                            {
                                nodesNotExpanded++;
                                looped = true;
                            }
                        }
                        else
                        {
                            node = node.AddChild(objectPool, move, state);
                            // RAVE Optimization && best rollout
                            UpdateSolutionHash(move);
                            currentRollout.Add(move);
                            allFirstMoves.Add(move);
                            nodeCount++;
                        }
                        visitedStatesInRollout.Add(state.Clone());
                    }
                    else
                    {
                        state.Pass();
                    }
                }
                else
                {
                    nodesNotExpanded++;
                }
                 

                // Rollout
                while (!state.isTerminal() && !looped)
                {
                    var move = state.GetSimulationMove();
                    backupState = state.Clone();
                    if (move != -1)
                    {
                        state.DoMove(move);
                        if (visitedStatesInRollout.Contains(state))
                        {
                            if (avoidCycles)
                            {
                                state = backupState.Clone();
                                List<IPuzzleMove> availableMoves = state.GetMoves();
                                while (availableMoves.Count > 0 && visitedStatesInRollout.Contains(state))
                                { //keep trying different moves until we end up in an unvisited state
                                    state = backupState.Clone();
                                    move = availableMoves[RNG.Next(availableMoves.Count)];
                                    availableMoves.Remove(move);
                                    state.DoMove(move);
                                }
                                if (availableMoves.Count == 0 && visitedStatesInRollout.Contains(state))//all states have already been visited
                                {
                                    break;
                                }
                            }
                            else
                            {
                                looped = true;
                            }
                        }
                        // RAVE Optimization && best rollout
                        UpdateSolutionHash(move);
                        currentRollout.Add(move);
                        allFirstMoves.Add(move);
                        visitedStatesInRollout.Add(state.Clone());
                    }
                    else //simulation ended
                    {
                        break;
                        //state.Pass();
                    }
                }
                
                //Keep topScore and update bestRollout
                double result = state.GetResult();
                minReward = Math.Min(result, minReward);
                maxReward = Math.Max(result, maxReward);
                if (state.EndState() && !solutionHashes.Contains(solutionHash))
                {
                    solutionHashes.Add(solutionHash);
                    solutionCount++;
                    if (iterationsForFirstSolution < 0)
                    {
                        iterationsForFirstSolution = iterationsExecuted+1;
                    }
                }
                if (result > topScore || result == topScore && currentRollout.Count < bestRollout.Count)
                {
                    topScore = result;
                    bestRollout = currentRollout;
                    if (state.EndState() && stopOnResult)
                    {
                        iterationsExecuted++;
                        break;
                    }
                }


                // Backpropagate
                currentDepth = 0;
                while (node != null)
                {
                    if (looped)
                    {
                        //TODO penalize score for loops?
                    }
                    ISPTreeNode parent = node.Parent;
                    //if a node is a dead end remove it from the tree
                    if (!node.HasChildren() && !node.HasMovesToTry() && !state.EndState() && useNodeElimination)
                    {
                        if (node.Parent == null)//unsolvable level. The tree has been completely explored. Return current best score
                        {
                            //SinglePlayerMCTSMain.Log("Unsolvable Level");
                            //Console.WriteLine("\nUnsolvable Level");
                            break;
                        }
                        node.Parent.RemoveChild(node);
                        nodeCount--;
                        nodesEliminated++;
                        currentDepth--;
                    }

                    // RAVE Optimization
                    node.Update(result, allFirstMoves);
                    node = parent;
                    currentDepth++;
                    // Node Recycling Optimization
                    if (((Opt_SP_UCTTreeNode)rootNode).NodeRecycling)
                    {
                        // Non-leaf node pushed back to LRU queue when updated
                        if (node != rootNode && node != null && node.HasChildren())
                        {
                            LRUQueueManager.LRUAddLast(ref node, ref head, ref tail);
                        }
                    }
                }

                maxDepth = Math.Max(maxDepth, currentDepth);

                if (!rootNode.HasChildren() && !rootNode.HasMovesToTry())
                {
                    break;
                }

                if (!search)
                {
                    search = true;
                    return null;
                }

                #if PROFILING
                    afterMemory = GC.GetTotalMemory(false);
                    usedMemory = afterMemory - beforeMemory;
                    averageUsedMemoryPerIteration = usedMemory / (i + 1);

                    var outStringToWrite = string.Format(" optMCTS search: {0:0.00}% [{1} of {2}] - Total used memory B(MB): {3}({4:N7}) - Average used memory per iteration B(MB): {5}({6:N7})\n",
                        (float)((i + 1) * 100) / (float)iterations, i + 1, iterations, usedMemory, usedMemory / 1024 / 1024, averageUsedMemoryPerIteration, 
                        (float)averageUsedMemoryPerIteration / 1024 / 1024);
                    #if DEBUG
                    if (showMemoryUsage)
                    {
                        Console.Write(outStringToWrite);
                        Console.SetCursorPosition(0, Console.CursorTop);
                    }
                    #endif
                #endif

                //Console.WriteLine(rootNode.TreeToString(0));
            }
            //Console.WriteLine();

            objectPool.NeedToClean = true;

            //#if DEBUG
            //    Console.WriteLine(rootNode.ChildrenToString());
            //    Console.WriteLine(rootNode.TreeToString(0));
            //#endif


            IPuzzleMove bestMove;
            if (bestRollout != null && bestRollout.Count > 0) //Remove first move from rollout so that if the topScore is not beaten we can just take the next move on the next search
            {
                bestMove = bestRollout[0];
                bestRollout.RemoveAt(0);
            }
            else
            {
                bestMove = rootNode.GetBestMove();
            }
            Debug.WriteLine(rootNode.TreeToString(0));
            Debug.WriteLine("Min Reward: " + minReward + " - Max Reward: " + maxReward);
            visits = new List<int>();
            raveVisits = new List<int>();
            CountVisits((Opt_SP_UCTTreeNode)rootNode, visits, raveVisits);

            visits.Sort((x, y) => (x.CompareTo(y)));
            raveVisits.Sort((x, y) => (x.CompareTo(y)));
            //string visitsString = LogVisits((Opt_SP_UCTTreeNode) rootNode);
            //SinglePlayerMCTSMain.Log("Iterations: "+IterationsExecuted+" NodeCount: " + nodeCount+" "+visitsString);
            return bestMove;
        }

        public ISPTreeNodeCreator TreeCreator
        {
            get { return treeCreator; }
        }

        public List<IPuzzleMove> BestRollout { get => bestRollout; set => bestRollout = value; }
        public int IterationsExecuted { get => iterationsExecuted; set => iterationsExecuted = value; }
        public int IterationsForFirstSolution { get => iterationsForFirstSolution; set => iterationsForFirstSolution = value; }
        public int SolutionCount { get => solutionCount; set => solutionCount = value; }
        public int NodeCount { get => nodeCount; set => nodeCount = value; }

        public void Abort()
        {
            search = false;
        }

        private void UpdateSolutionHash(IPuzzleMove move)
        {
            solutionHash = solutionHash * 13 + move.GetHashCode();
        }

        private string LogVisits(Opt_SP_UCTTreeNode node)
        {
            List<int> visits = new List<int>();

            List<int> raveVisits = new List<int>();
            CountVisits(node, visits, raveVisits);

            visits.Sort((x, y) => (x.CompareTo(y)));
            raveVisits.Sort((x, y) => (x.CompareTo(y)));

            double avgVisits = 0;
            double avgRaveVisits = 0;
            foreach(int v in visits)
            {
                avgVisits += v;
            }
            foreach(int v in raveVisits)
            {
                avgRaveVisits += v;
            }
            avgVisits /= visits.Count;
            avgRaveVisits /= raveVisits.Count;
            string s = "";
            s+= " Median visits: "+(visits.Count%2==0?visits[visits.Count/2]:(visits[visits.Count / 2]+ visits[1+visits.Count / 2])/2);
            s+= " Median raveVisits: " + (raveVisits.Count % 2 == 0 ? raveVisits[raveVisits.Count / 2] : (raveVisits[raveVisits.Count / 2] + raveVisits[1 + raveVisits.Count / 2]) / 2);
            s += " Avg visits: " + avgVisits;
            s+= " Avg raveVisits: " + avgRaveVisits;
            return s;
        }

        private void CountVisits(Opt_SP_UCTTreeNode node, List<int> visits, List<int> raveVisits)
        {
            visits.Add(node.Visits);
            raveVisits.Add(node.RAVEvisits);
            foreach(Opt_SP_UCTTreeNode child in node.ChildNodes)
            {
                CountVisits(child,visits,raveVisits);
            }
        }
    }
}