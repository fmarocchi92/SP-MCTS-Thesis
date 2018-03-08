using System;
using System.Collections.Generic;
using Common.Abstract;
using MCTS.Standard.Utils;
using Common;
using MCTS2016.SP_MCTS;
using MCTS2016.Common.Abstract;
using MCTS2016.Optimizations.UCT;
using MCTS2016.SP_MCTS.Optimizations.Utils;

namespace MCTS2016.SP_MCTS.SP_UCT
{
    public class SP_UCTTreeNode : ISPTreeNode
    {
        private static Random random = new Random();
        private SP_UCTTreeNode parent;
        private IPuzzleMove move;
        protected List<SP_UCTTreeNode> childNodes;
        protected List<IPuzzleMove> untriedMoves;
        private double wins;
        private double score;
        protected int visits;
        protected double const_C;
        protected double const_D;
        private double squaredReward;
        private double topScore;
        private MersenneTwister rnd;

        public SP_UCTTreeNode(IPuzzleMove move, SP_UCTTreeNode parent, IPuzzleState state, MersenneTwister rng, double const_C = 1, double const_D = 20000, bool generateUntriedMoves = true)
        {
            this.move = move;
            this.parent = parent;
            this.const_C = const_C;
            this.const_D = const_D;
            rnd = rng;
            childNodes = new List<SP_UCTTreeNode>();
            wins = 0;
            visits = 0;
            squaredReward = 0;
            topScore = double.MinValue;
            if (generateUntriedMoves)
            {
                untriedMoves = state.GetMoves();
            }
        }

        public ISPTreeNode Parent
        {   
            get { return parent; }
        }

        public IPuzzleMove Move
        {
            get { return move; }
        }

        public ISPTreeNode SelectChild()
        {
            childNodes.Sort((x, y) => -x.SP_UCTValue().CompareTo(y.SP_UCTValue()));
            return childNodes[0];
        }

        private double SP_UCTValue()
        {
            return wins / visits + topScore*0.02 + const_C * Math.Sqrt(2 * Math.Log(parent.visits) / visits)
            +Math.Sqrt((squaredReward - visits * Math.Pow(wins / visits, 2) + const_D) / visits); //this should probably be used only in score maximization problems
        }

        public virtual ISPTreeNode AddChild(IPuzzleMove move, IPuzzleState state)
        {
            untriedMoves.Remove(move);
            SP_UCTTreeNode n = new SP_UCTTreeNode(move, this, state, rnd, const_C, const_D, true);
            childNodes.Add(n);
            return n;
        }

        public ISPTreeNode AddChild(ObjectPool objectPool, IPuzzleMove move, IPuzzleState state)
        {
            throw new NotImplementedException();
        }

        public void Update(double result)
        {
            visits++;
            wins += result;
            score += result;
            squaredReward += result * result;
            topScore = Math.Max(topScore, result);
        }

        public void Update(double result, HashSet<IPuzzleMove> moveSet)
        {
            throw new NotImplementedException();
        }

        public IPuzzleMove SelectUntriedMove()
        {
            return untriedMoves[rnd.Next(untriedMoves.Count)];
        }

        /// <summary>
        /// Returns the move with the highest final topScore
        /// </summary>
        /// <returns></returns>
        public IPuzzleMove GetBestMove()
        {
            childNodes.Sort((x, y) => -x.topScore.CompareTo(y.topScore));
            return childNodes.Count > 0 ? childNodes[0].move : (IPuzzleMove)(0 - 1);
        }

        public bool HasMovesToTry()
        {
            return untriedMoves.Count != 0;
        }

        public bool HasChildren()
        {
            return childNodes.Count != 0;
        }

        public override string ToString()
        {
            return "[M:" + move + " W/V/T:" + wins + "/" + visits + "/" +topScore + "]";
        }

        public string TreeToString(int indent)
        {
            string s = IndentString(indent) + ToString();
            foreach (SP_UCTTreeNode c in childNodes)
            {
                s += c.TreeToString(indent + 1);
            }
            return s;
        }

        public string ChildrenToString()
        {
            string s = "";
            foreach (SP_UCTTreeNode c in childNodes)
            {
                s += c + "\n";
            }
            return s;
        }

        private string IndentString(int indent)
        {
            string s = "\n";
            for (int i = 1; i < indent + 1; i++)
            {
                s += "| ";
            }
            return s;
        }

        public List<IPuzzleMove> GetUntriedMoves()
        {
            return untriedMoves;
        }

        public void RemoveUntriedMove(IPuzzleMove move)
        {
            untriedMoves.Remove(move);
        }

        public void RemoveChild(ISPTreeNode child)
        {
            childNodes.Remove((SP_UCTTreeNode) child);
        }

        public void ChildRecycle()
        {
            throw new NotImplementedException();
        }

        public Opt_SP_UCTTreeNode NextLRUElem { get; set; }
        public Opt_SP_UCTTreeNode PrevLRUElem { get; set; }
    }
}
