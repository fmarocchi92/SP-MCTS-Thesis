using MCTS2016.SP_MCTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MCTS2016.Common.Abstract;
using MCTS2016.IDAStar;
using Common;

namespace MCTS2016.Puzzles.Sokoban
{
    class SokobanIDAstarStrategy:ISPSimulationStrategy
    {
        IDAStarSearch idaStar;
        List<IPuzzleMove> moveList;
        int maxNodes;
        int tableSize;
        int maxDepth;
        double epsilon;

        public SokobanIDAstarStrategy(int maxNodes, int tableSize, int maxDepth, double epsilon)
        {
            this.maxNodes = maxNodes;
            this.tableSize = tableSize;
            this.maxDepth = maxDepth;
            this.epsilon = epsilon;
            idaStar = new IDAStarSearch();
        }

        public string getFriendlyName()
        {
            return "SokobanIdaStrategy - maxNodes: " + maxNodes + " tableSize: " + tableSize + " maxDepth: " + maxDepth;
        }

        public string getTypeName()
        {
            return GetType().Name;
        }

        public IPuzzleMove selectMove(IPuzzleState gameState)
        {
            //if (moveList != null)
            //{
            //    if (moveList.Count > 0)
            //    {
            //        IPuzzleMove move = moveList[0];
            //        moveList.RemoveAt(0);
            //        return move;
            //    }
            //    else
            //    {
            //        moveList = null;
            //        return (IPuzzleMove)(-1);
            //    }
            //}
            //else
            //{
            //    moveList = idaStar.Solve(gameState, maxNodes, tableSize, maxDepth);
            //    if (moveList.Count == 0)
            //    {
            //        return (IPuzzleMove)(-1);
            //    }
            //    IPuzzleMove move = moveList[0];
            //    moveList.RemoveAt(0);
            //    return move;
            //}
            if (RNG.NextDouble() < epsilon)
            {
                return gameState.GetMoves()[RNG.Next(gameState.GetMoves().Count)];
            }
            moveList = idaStar.Solve(gameState, maxNodes, tableSize, maxDepth);
            if (moveList.Count > 0)
            {
                return moveList[0];
            }
            else
            {
                return (IPuzzleMove)(-1);
            }
        }
    }
}
