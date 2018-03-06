using MCTS.Standard.Utils;
using MCTS2016.Optimizations.UCT;

namespace MCTS2016.SP_MCTS.Optimizations.Utils
{
    public class LRUQueueManager
    {
        public static void LRUAddLast(ref ISPTreeNode node, ref ISPTreeNode head, ref ISPTreeNode tail)
        {
            if (head == null && tail == null)
            {
                node.NextLRUElem = (Opt_SP_UCTTreeNode) node;
                node.PrevLRUElem = (Opt_SP_UCTTreeNode) node;
                head = node;
                tail = node;
                return;
            }

            tail.PrevLRUElem = (Opt_SP_UCTTreeNode) node;
            node.NextLRUElem = (Opt_SP_UCTTreeNode) tail;
            node.PrevLRUElem = (Opt_SP_UCTTreeNode) node;
            tail = node;
        }

        public static void LRURemoveElement(ref ISPTreeNode node, ref ISPTreeNode head, ref ISPTreeNode tail)
        {
            if (node == head && node == tail)
            {
                node.NextLRUElem = null;
                node.PrevLRUElem = null;
                head = null;
                tail = null;
                return;
            }
            
            if (node == head)
            {
                head = node.PrevLRUElem;
                head.NextLRUElem.NextLRUElem = null;
                head.NextLRUElem.PrevLRUElem = null;
                head.NextLRUElem = (Opt_SP_UCTTreeNode) head;
                return;
            }
            
            if (node == tail)
            {
                tail = node.NextLRUElem;
                tail.PrevLRUElem.NextLRUElem = null;
                tail.PrevLRUElem.PrevLRUElem = null;
                tail.PrevLRUElem = (Opt_SP_UCTTreeNode) tail;
                return;
            }

            node.NextLRUElem.PrevLRUElem = node.PrevLRUElem;
            node.PrevLRUElem.NextLRUElem = node.NextLRUElem;
            node.NextLRUElem = null;
            node.PrevLRUElem = null;
        }

        public static void LRURemoveFirst(ref ISPTreeNode head, ref ISPTreeNode tail)
        {
            if (head == tail && head != null)
            {
                head.NextLRUElem = null;
                head.PrevLRUElem = null;
                head = null;
                tail = null;
                return;
            }
            
            head = head.PrevLRUElem;
            head.NextLRUElem.NextLRUElem = null;
            head.NextLRUElem.PrevLRUElem = null;
            head.NextLRUElem = (Opt_SP_UCTTreeNode) head;
        }
    }
}