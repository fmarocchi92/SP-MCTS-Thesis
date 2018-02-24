using System;
using Common.Abstract;

namespace MCTS.Standard.Utils
{
    public class MCTSAlgorithm
    {
        private ITreeNodeCreator treeCreator;
        private bool search = true;

        public MCTSAlgorithm(ITreeNodeCreator treeCreator)
        {
            this.treeCreator = treeCreator;
        }

        public IGameMove Search(IGameState rootState, int iterations)
        {
            if (!search)
            {
                search = true;
            }
            
            ITreeNode rootNode = treeCreator.GenRootNode(rootState);

            long beforeMemory = GC.GetTotalMemory(false);
            long afterMemory = GC.GetTotalMemory(false);
            long usedMemory = afterMemory - beforeMemory;
            long averageUsedMemoryPerIteration = 0;

            for (int i = 0; i < iterations; i++)
            {
                ITreeNode node = rootNode;
                IGameState state = rootState.Clone();

                // Select
                while (!node.HasMovesToTry() && node.HasChildren())
                {
                    node = node.SelectChild();
                    state.DoMove(node.Move);
                }

                // Expand
                if (node.HasMovesToTry())
                {
                    IGameMove move = node.SelectUntriedMove();
                    if (move != -1)
                    {
                        state.DoMove(move);
                    }
                    else
                    {
                        state.Pass();
                    }
                    node = node.AddChild(move, state);
                }

                // Rollout
                while (!state.isTerminal())
                {
                    var move = state.GetSimulationMove();
                    if (move != -1)
                    {
                        state.DoMove(move);
                    }
                    else
                    {
                        state.Pass();
                    }
                }

                // Backpropagate
                while (node != null)
                {
                    node.Update(state.GetResult(node.PlayerWhoJustMoved));
                    node = node.Parent;
                }

                if (!search)
                {
                    search = true;
                    return null;
                }

                afterMemory = GC.GetTotalMemory(false);
                usedMemory = afterMemory - beforeMemory;
                averageUsedMemoryPerIteration = usedMemory / (i + 1);

                var outStringToWrite = string.Format(" MCTS search: {0:0.00}% [{1} of {2}] - Total used memory b(mb): {3}({4}) - Average used memory per iteration b(mb): {5}({6:N7})", (float)((i + 1) * 100) / (float)iterations, i + 1, iterations, usedMemory, usedMemory / 1024 / 1024, averageUsedMemoryPerIteration, (float)averageUsedMemoryPerIteration / 1024 / 1024);
                #if DEBUG
                if (iterations > 50000)
                {
                    Console.Write(outStringToWrite);
                    Console.SetCursorPosition(0, Console.CursorTop);
                }
                #endif

                //Console.WriteLine(rootNode.TreeToString(0));
            }
            //Console.WriteLine();

//#if DEBUG
//            Console.WriteLine(rootNode.ChildrenToString());
//            Console.WriteLine(rootNode.TreeToString(0));
//#endif

            return rootNode.GetBestMove();
        }

        public ITreeNodeCreator TreeCreator
        {
            get { return treeCreator; }
        }

        public void Abort()
        {
            search = false;
        }
    }
}