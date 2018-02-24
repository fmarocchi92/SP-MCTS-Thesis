using MCTS2016.Common.Abstract;
using MCTS2016.Puzzles.Sokoban;
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
        IPuzzleState rootState;
        AStarNode result;
        List<IPuzzleState> visited;
        double RESULT=-1;
        double NOT_FOUND = -2;

        public IDAStarSearch(IPuzzleState rootState)
        {
            this.rootState = rootState;
        }

        public List<IPuzzleMove> Solve(double maxCost)
        {
            double threshold = rootState.GetResult();
            result = null;
            double value = 0;
            AStarNode rootNode = new AStarNode(rootState, null, null);
            while (value > RESULT)
            {
                visited = new List<IPuzzleState>() { rootState.Clone() };
                value = Search(rootNode, 0, threshold);
                if (value != RESULT && value > threshold)
                {
                    threshold = value;
                }
                if(threshold > maxCost)
                {
                    value = NOT_FOUND;
                }
            }
            return BuildSolution(result);
        }

        private double Search(AStarNode node, double cost, double threshold)
        {
            //Debug.WriteLine(node.state.PrettyPrint());
            double value = cost + node.state.GetResult();
            //Debug.WriteLine("Value :" + value);
            if (value > threshold)
            {
                visited.RemoveAt(visited.Count() - 1);
                return value;
            }
            if (node.state.EndState())
            {
                result = node;
                return RESULT;
            }
            double minValue = double.MaxValue;
            List<IPuzzleMove> moves = node.state.GetMoves();
            foreach(IPuzzleMove move in moves)
            {
                IPuzzleState clone = node.state.Clone();
                clone.DoMove(move);
                //Debug.WriteLine(clone.PrettyPrint());
                if (!visited.Contains(clone))
                {
                    visited.Add(clone);
                    value = Search(new AStarNode(clone, move, node), cost + 1, threshold);
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

    }
}
