//#define PROFILING

using System;
using System.Collections.Generic;
using Common.Abstract;
using MCTS.Standard.Utils;
using MCTS2016.Common.Abstract;
using MCTS2016.Optimizations.UCT;
using MCTS2016.SP_MCTS.Optimizations.UCT;
using MCTS2016.SP_MCTS.Optimizations.Utils;

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

        public OptMCTSAlgorithm(ISPTreeNodeCreator treeCreator, int iterations, int memoryBudget, bool stopOnResult)
        {
            this.treeCreator = treeCreator;
            this.stopOnResult = stopOnResult;
            this.memoryBudget = memoryBudget;
            objectPool = new ObjectPool(iterations, iterations, ((Opt_SP_UCTTreeNodeCreator)treeCreator).NodeRecycling, memoryBudget);
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
            int nodeCount = 0;
            bool looped;
            if (!search)
            {
                search = true;
            }
            
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

            #if PROFILING
                long beforeMemory = GC.GetTotalMemory(false);
                long afterMemory = GC.GetTotalMemory(false);
                long usedMemory = afterMemory - beforeMemory;
                long averageUsedMemoryPerIteration = 0;
            #endif
            
            for (int i = 0; i < iterations; i++)
            {
                looped = false;
                ISPTreeNode node = rootNode;
                IPuzzleState state = rootState.Clone();
                
                HashSet<IPuzzleState> visitedStatesInRollout = new HashSet<IPuzzleState>() { state.Clone() };
                
                // Clear lists of moves used for RAVE updates && best rollout
                currentRollout.Clear();

                // Select
                while (!node.HasMovesToTry() && node.HasChildren())
                {
                    // UCB1-Tuned and RAVE Optimizations
                    node = node.SelectChild();
                    state.DoMove(node.Move);
                    visitedStatesInRollout.Add(state.Clone());
                    // RAVE Optimization && best rollout
                    currentRollout.Add(node.Move);

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
                            looped = true;
                            //while (node.GetUntriedMoves().Count() > 0 && visitedStatesInRollout.Contains(state))
                            //{
                            //    state = backupState.Clone();
                            //    move = node.GetUntriedMoves()[RNG.Next(node.GetUntriedMoves().Count())];
                            //    state.DoMove(move);
                            //    node.RemoveUntriedMove(move);
                            //}
                            //if (!visitedStatesInRollout.Contains(state)) //found valid move
                            //{
                            //    node = node.AddChild(move, state);
                            //    currentRollout.Add(move);
                            //    nodeCount++;
                            //}
                            //else //all moves visited
                            //{
                            //    state = backupState;
                            //}
                        }
                        else
                        {
                            node = node.AddChild(objectPool, move, state);
                            // RAVE Optimization && best rollout
                            currentRollout.Add(move);
                            nodeCount++;
                        }
                        visitedStatesInRollout.Add(state.Clone());
                    }
                    else
                    {
                        state.Pass();
                    }
                }
                
                //if a node is a dead end remove it from the tree
                if(!node.HasChildren() && !node.HasMovesToTry() && !state.EndState())
                {
                    if(node.Parent == null)//unsolvable level. The tree has been completely explored. Return current best score
                    {
                        break;
                    }
                    node.Parent.RemoveChild(node);
                    nodeCount--;

                    if (!node.Parent.HasChildren())
                    {
                        var tempNode = node.Parent;
                        LRUQueueManager.LRURemoveElement(ref tempNode, ref head, ref tail);    
                    }
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
                            looped = true;
                            //state = backupState.Clone();
                            //List<IPuzzleMove> availableMoves = state.GetMoves();
                            //while(availableMoves.Count()>0 && visitedStatesInRollout.Contains(state)) { //keep trying different moves until we end up in an unvisited state
                            //    state = backupState.Clone();
                            //    move = availableMoves[RNG.Next(availableMoves.Count())];
                            //    availableMoves.Remove(move);
                            //    state.DoMove(move);
                            //}
                            //if (availableMoves.Count() == 0 && visitedStatesInRollout.Contains(state))//all states have already been visited
                            //{
                            //    break;
                            //}
                        }
                        // RAVE Optimization && best rollout
                        currentRollout.Add(move);
                        visitedStatesInRollout.Add(state.Clone());
                    }
                    else
                    {
                        state.Pass();
                    }
                }
                
                //Keep topScore and update bestRollout
                double result = state.GetResult();
                if (result > topScore || result == topScore && currentRollout.Count < bestRollout.Count)
                {
                    topScore = result;
                    bestRollout = currentRollout;
                    if (state.EndState() && stopOnResult)
                    {
                        break;
                    }
                }

                // Backpropagate
                while (node != null)
                {
                    if (looped)
                    {
                        //TODO penalize score for loops?
                    }
                    
                    // RAVE Optimization
                    node.Update(result, currentRollout);
                    node = node.Parent;

                    // Node Recycling Optimization
                    if (((Opt_SP_UCTTreeNode)node).NodeRecycling)
                    {
                        // Non-leaf node pushed back to LRU queue when updated
                        if (node != rootNode && node != null)
                        {
                            LRUQueueManager.LRUAddLast(ref node, ref head, ref tail);
                        }
                    }
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
            return bestMove;
        }

        public ISPTreeNodeCreator TreeCreator
        {
            get { return treeCreator; }
        }

        public void Abort()
        {
            search = false;
        }
    }
}