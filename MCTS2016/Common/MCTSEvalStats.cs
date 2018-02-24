using System.Collections.Generic;

namespace Common
{
    public class TrialEvalStats
    {
        public class EvalStats
        {
            public int min_backups;
            public int max_backups;

            public TwoPlayersGameStats stats;
        }

        private double _complexity = 0f;                                                /// computed complexity
        private List<TwoPlayersGameStats>   match = new List<TwoPlayersGameStats>();    /// saves all the match stats

        public double Complexity { get { return _complexity; } }

    }
}