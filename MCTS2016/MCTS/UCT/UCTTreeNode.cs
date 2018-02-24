using System;
using System.Collections.Generic;
using Common.Abstract;

namespace MCTS.Standard.Utils.UCT
{
    public class UCTTreeNode : ITreeNode
    {
        private static Random random = new Random();
        private UCTTreeNode parent;
        private IGameMove move;
        protected List<UCTTreeNode> childNodes;
        protected List<IGameMove> untriedMoves;
        private double wins;
        protected int visits;
        private int playerWhoJustMoved;
        protected double constant;

        public UCTTreeNode(IGameMove move, UCTTreeNode parent, IGameState state, double constant = 1.0, bool generateUntriedMoves = true)
        {
            this.move = move;
            this.parent = parent;
            this.constant = constant;
            childNodes = new List<UCTTreeNode>();
            wins = 0;
            visits = 0;
            playerWhoJustMoved = state.playerJustMoved;
            if (generateUntriedMoves)
            {
                untriedMoves = state.GetMoves();
            }
        }

        public int PlayerWhoJustMoved
        {
            get { return playerWhoJustMoved; }
        }

        public ITreeNode Parent
        {
            get { return parent; }
        }

        public IGameMove Move
        {
            get { return move; }
        }

        public ITreeNode SelectChild()
        {
            childNodes.Sort((x, y) => -x.UCTValue().CompareTo(y.UCTValue()));
            return childNodes[0];
        }

        private double UCTValue()
        {
            return wins / visits + constant * Math.Sqrt(2 * Math.Log(parent.visits) / visits);
        }

        public virtual ITreeNode AddChild(IGameMove move, IGameState state)
        {
            UCTTreeNode n = new UCTTreeNode(move, this, state, constant);
            untriedMoves.Remove(move);
            childNodes.Add(n);
            return n;
        }

        public void Update(double result)
        {
            visits++;
            wins += result;
        }

        public IGameMove SelectUntriedMove()
        {
            return untriedMoves[random.Next(untriedMoves.Count)];
        }

        public IGameMove GetBestMove()
        {
            childNodes.Sort((x, y) => -x.visits.CompareTo(y.visits));
            return childNodes.Count > 0 ? childNodes[0].move : (IGameMove)(0 - 1);
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
            return "[M:" + move + " W/V:" + wins + "/" + visits + "]";
        }

        public string TreeToString(int indent)
        {
            string s = IndentString(indent) + ToString();
            foreach (UCTTreeNode c in childNodes)
            {
                s += c.TreeToString(indent + 1);
            }
            return s;
        }

        public string ChildrenToString()
        {
            string s = "";
            foreach (UCTTreeNode c in childNodes)
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
