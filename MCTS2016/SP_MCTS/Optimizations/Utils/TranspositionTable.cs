using MCTS2016.Common.Abstract;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.SP_MCTS.Optimizations.Utils
{
    public class TranspositionTable
    {
        private TranspositionTableEntry[] table;

        public TranspositionTable(int tableSize)
        {
            table = new TranspositionTableEntry[tableSize];
        }

        public TranspositionTableEntry Retrieve(int hashkey)
        {
            TranspositionTableEntry entry = table[(uint)hashkey % table.Length];
            if(entry != null && entry.HashKey == hashkey)
            { 
                return entry;
            }
            else if(entry != null)
            {
                Debug.WriteLine("Hash Retrieve Conflict");
            }
            return null;
        }

        public TranspositionTableEntry GetAnyEntryWithIndex(int hashkey)
        {
            return table[(uint)hashkey % table.Length];
        }

        public void Store(TranspositionTableEntry entry)
        {
            if(table[(uint)entry.HashKey % table.Length] !=null && entry.HashKey != table[(uint)entry.HashKey % table.Length].HashKey)
                Debug.WriteLine("Hash Store Conflict");
            table[(uint)entry.HashKey % table.Length] = entry;
        }
    }

    public class TranspositionTableEntry
    {
        private int hashKey;
        private double score;
        private int depth;
        private bool visited;

        public int HashKey { get => hashKey; set => hashKey = value; }
        public double Score { get => score; set => score = value; }
        public int Depth { get => depth; set => depth = value; }
        public bool Visited { get => visited; set => visited = value; }

        public TranspositionTableEntry(int hashKey, double score, int depth, bool visited)
        {
            this.hashKey = hashKey;
            this.score = score;
            this.depth = depth;
            this.visited = visited;
        }
    }
}
