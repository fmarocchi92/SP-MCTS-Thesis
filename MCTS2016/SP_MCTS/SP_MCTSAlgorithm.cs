using System.Text;
using System.Threading.Tasks;
using MCTS2016.SP_MCTS.SP_UCT;
using MCTS2016.Common.Abstract;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Diagnostics;
using System.Linq;
using Common;

namespace MCTS2016.SP_MCTS
{
    class SP_MCTSAlgorithm
    {
        private ISPTreeNodeCreator treeCreator;
        private bool search = true;

        private List<IPuzzleMove> bestRollout;
        private double topScore = double.MinValue;
        private bool stopOnResult;

        private int iterationsExecuted;

        public int IterationsExecuted { get => iterationsExecuted; set => iterationsExecuted = value; }

        public SP_MCTSAlgorithm(ISPTreeNodeCreator treeCreator, bool stopOnResult)
        {
            this.treeCreator = treeCreator;
            this.stopOnResult = stopOnResult;
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
            if (!search)
            {
                search = true;
            }

            ISPTreeNode rootNode = treeCreator.GenRootNode(rootState);

            long beforeMemory = GC.GetTotalMemory(false);
            long afterMemory = GC.GetTotalMemory(false);
            long usedMemory = afterMemory - beforeMemory;
            long averageUsedMemoryPerIteration = 0;
            iterationsExecuted = 0;

            bool looped=false;


            for (iterationsExecuted = 0; iterationsExecuted < iterations; iterationsExecuted++)
            {
                looped = false;
                ISPTreeNode node = rootNode;
                IPuzzleState state = rootState.Clone();
                HashSet<IPuzzleState> visitedStatesInRollout = new HashSet<IPuzzleState>() { state.Clone() };
                List<IPuzzleMove> currentRollout = new List<IPuzzleMove>();
                // Select
                while (!node.HasMovesToTry() && node.HasChildren())
                {
                    node = node.SelectChild();
                    state.DoMove(node.Move);
                    visitedStatesInRollout.Add(state.Clone());
                    currentRollout.Add(node.Move);
                }
                IPuzzleState backupState = state.Clone();


                // Expand
                if (node.HasMovesToTry())
                {
                    IPuzzleMove move = node.SelectUntriedMove();
                    if (move != -1)
                    {
                        state.DoMove(move);
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
                            node = node.AddChild(move, state);
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
                        currentRollout.Add(move);
                        visitedStatesInRollout.Add(state.Clone());
                    }
                    else
                    {
                        break;
                    }
                }
                //Keep topScore and update bestRollout
                double result = state.GetResult();
                if (result > topScore || result == topScore && currentRollout.Count() < bestRollout.Count())
                {
                    topScore = result;
                    bestRollout = currentRollout ;
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
                    node.Update(result);
                    node = node.Parent;
                }
                if (!search)
                {
                    search = true;
                    return null;
                }

                afterMemory = GC.GetTotalMemory(false);
                usedMemory = afterMemory - beforeMemory;
                averageUsedMemoryPerIteration = usedMemory / (iterationsExecuted + 1);

                var outStringToWrite = string.Format(" MCTS search: {0:0.00}% [{1} of {2}] - Total used memory b(mb): {3}({4}) - Average used memory per iteration b(mb): {5}({6:N7})", (float)((iterationsExecuted + 1) * 100) / (float)iterations, iterationsExecuted + 1, iterations, usedMemory, usedMemory / 1024 / 1024, averageUsedMemoryPerIteration, (float)averageUsedMemoryPerIteration / 1024 / 1024);
#if DEBUG
                if (iterations > 1000000000)
                {
                    Console.Write(outStringToWrite);
                    Console.SetCursorPosition(0, Console.CursorTop);
                }
#endif

            }
            IPuzzleMove bestMove;
            if (bestRollout != null && bestRollout.Count() > 0) //Remove first move from rollout so that if the topScore is not beaten we can just take the next move on the next search
            {
                bestMove = bestRollout.First<IPuzzleMove>();
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