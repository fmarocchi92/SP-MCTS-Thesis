using System;
using System.Collections.Generic;
using Common;
using Common.Abstract;
using MCTS.Standard.Utils;
using MCTS2016.Common.Abstract;
using MCTS2016.SP_MCTS;
using MCTS2016.SP_MCTS.Optimizations.Utils;
using MCTS2016.SP_MCTS.SP_UCT;

namespace MCTS2016.Optimizations.UCT
{
    public class Opt_SP_UCTTreeNode : ISPTreeNode
    {
        private static Random random = new Random();
        private Opt_SP_UCTTreeNode parent;
        protected List<Opt_SP_UCTTreeNode> childNodes;
        protected internal List<IPuzzleMove> untriedMoves;
        private double wins;
        private double squares_rewards;
        protected int visits;
        private double RAVEwins;
        protected int RAVEvisits;
        protected double const_C;
        protected double const_D;
        private double squaredReward;
        private double topScore;
        private MersenneTwister rnd;
        private bool ucb1Tuned;
        private bool rave;
        private bool nodeRecycling;

        public Opt_SP_UCTTreeNode(IPuzzleMove move, Opt_SP_UCTTreeNode parent, IPuzzleState state, MersenneTwister rng, bool ucb1Tuned, bool rave, bool nodeRecycling, double const_C = 1, double const_D = 20000, bool generateUntriedMoves = true)
        {
            Move = move;
            this.parent = parent;
            this.const_C = const_C;
            this.const_D = const_D;
            rnd = rng;
            childNodes = new List<Opt_SP_UCTTreeNode>();
            NextLRUElem = null;
            PrevLRUElem = null;
            SetActive = false;
            wins = 0;
            visits = 0;
            squares_rewards = 0;
            RAVEwins = 0;
            RAVEvisits = 0;
            squaredReward = 0;
            topScore = double.MinValue;
            this.ucb1Tuned = ucb1Tuned;
            this.rave = rave;
            this.nodeRecycling = nodeRecycling;
            if (generateUntriedMoves)
            {
                untriedMoves = state.GetMoves();
            }
        }

        public ISPTreeNode Parent
        {
            get => parent;
            set => parent = (Opt_SP_UCTTreeNode) value;
        }

        public IPuzzleMove Move { get; set; }
        
        public double ConstC
        {
            get => const_C;
            set => const_C = value;
        }

        public double ConstD
        {
            get => const_D;
            set => const_D = value;
        }

        public MersenneTwister Rnd
        {
            get => rnd;
            set => rnd = value;
        }

        public bool Ucb1Tuned
        {
            get => ucb1Tuned;
            set => ucb1Tuned = value;
        }

        public bool Rave
        {
            get => rave;
            set => rave = value;
        }
        
        public bool NodeRecycling
        {
            get => nodeRecycling;
            set => nodeRecycling = value;
        }

        public ISPTreeNode SelectChild()
        {
            childNodes.Sort((x, y) => -x.SP_UCTValue().CompareTo(y.SP_UCTValue()));
            return childNodes[0];
        }

        private double SP_UCTValue()
        {
            double RAVEScore = 0;
            double alpha = 0;
            double UCTScore;
            var ucb1min = 2.0;

            if (rave)
            {
                // Standard UCT formula, check if is correct to use parent.visits insted of parent.RAVEvisits!!!!
                RAVEScore = RAVEwins / RAVEvisits + const_C * Math.Sqrt(2 * Math.Log(parent.visits) / RAVEvisits);
                if (double.IsNaN(RAVEScore))
                    RAVEScore = 0;
                alpha = RAVEScore == 0 ? 0 : Math.Max(0, (1000 - RAVEvisits) / 1000);
            }
                        
            if (ucb1Tuned)
            {
                // UCT value is computed using UCB1-Tuned formula
                // variance is computed incrementally with formula:
                // s^2 = n * (sum(xi^2) - (sum(xi))^2) / n * (n - 1)
                ucb1min = Math.Min(1 / 4.0, (visits * squares_rewards - Math.Pow(wins, 2)) / visits * (visits - 1)
                                                + Math.Sqrt(2 * Math.Log(parent.visits) / visits));
            }
            
            UCTScore =  wins / visits + topScore*0.02 + const_C * Math.Sqrt(ucb1min * Math.Log(parent.visits) / visits)
                        +Math.Sqrt((squaredReward - visits * Math.Pow(wins / visits, 2) + const_D) / visits); //this should probably be used only in score maximization problems
            
            
            return alpha * RAVEScore + (1 - alpha) * UCTScore;
        }
        
        public virtual ISPTreeNode AddChild(IPuzzleMove move, IPuzzleState state)
        {
            untriedMoves.Remove(move);
            Opt_SP_UCTTreeNode n = new Opt_SP_UCTTreeNode(move, this, state, rnd, ucb1Tuned, rave, nodeRecycling, const_C, const_D);
            childNodes.Add(n);
            return n;
        }

        public virtual ISPTreeNode AddChild(ObjectPool objectPool, IPuzzleMove move, IPuzzleState state)
        {
            Opt_SP_UCTTreeNode n = objectPool.GetObject(move, this, state, rnd, ucb1Tuned, rave, nodeRecycling, const_C, const_D);
            untriedMoves.Remove(move);
            childNodes.Add(n);
            return n;
        }

        public void ChildRecycle()
        {
            childNodes.Sort((x, y) => x.SP_UCTValue().CompareTo(y.SP_UCTValue()));
            childNodes[0].ResetNode();
            childNodes.RemoveAt(0);
        }
        
        public void RemoveChild(ISPTreeNode child)
        {
            ((Opt_SP_UCTTreeNode) child).ResetNode();
            childNodes.Remove((Opt_SP_UCTTreeNode) child);
        }
        
        public List<IPuzzleMove> GetUntriedMoves()
        {
            return untriedMoves;
        }

        public void RemoveUntriedMove(IPuzzleMove move)
        {
            untriedMoves.Remove(move);
        }

        public void Update(double result)
        {
            throw new NotImplementedException();
        }

        public void Update(double result, List<IPuzzleMove> moveSet)
        {
            visits++;
            wins += result;
            squares_rewards += Math.Pow(result, 2);
            squaredReward += result * result;
            topScore = Math.Max(topScore, result);

            if (!rave || parent == null) return;
            foreach (Opt_SP_UCTTreeNode c in parent.childNodes)
            {
                if (c == this) continue;
                if (!moveSet.Contains(c.Move)) continue;
                c.RAVEvisits++;
                c.RAVEwins += result;
            }
        }

        public Opt_SP_UCTTreeNode NextLRUElem { get; set; }

        public Opt_SP_UCTTreeNode PrevLRUElem { get; set; }

        public bool SetActive { get; set; }
        
        public IPuzzleMove SelectUntriedMove()
        {
            return untriedMoves[random.Next(untriedMoves.Count)];
        }

        public IPuzzleMove GetBestMove()
        {
            childNodes.Sort((x, y) => -x.visits.CompareTo(y.visits));
            return childNodes.Count > 0 ? childNodes[0].Move : (IPuzzleMove)(0 - 1);
        }

        public bool HasMovesToTry()
        {
            return untriedMoves.Count != 0;
        }

        public bool HasChildren()
        {
            return childNodes.Count != 0;
        }

        public void ResetNode()
        {
            Move = null;
            parent = null;
            const_C = 1;
            const_D = 20000;
            rnd = null;
            childNodes.Clear();
            NextLRUElem = null;
            PrevLRUElem = null;
            SetActive = false;
            wins = 0;
            visits = 0;
            squares_rewards = 0;
            RAVEwins = 0;
            RAVEvisits = 0;
            squaredReward = 0;
            topScore = double.MinValue;
            ucb1Tuned = false;
            rave = false;
            nodeRecycling = false;
        }

        public override string ToString()
        {
            return "[M:" + Move + " W/V:" + wins + "/" + visits + "]";
        }

        public string TreeToString(int indent)
        {
            string s = IndentString(indent) + ToString();
            foreach (Opt_SP_UCTTreeNode c in childNodes)
            {
                s += c.TreeToString(indent + 1);
            }
            return s;
        }

        public string ChildrenToString()
        {
            string s = "";
            foreach (Opt_SP_UCTTreeNode c in childNodes)
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
    }
}
