using Common;
using Common.Abstract;
using MCTS2016.Common.Abstract;
using MCTS2016.SP_MCTS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Puzzles.SameGame
{
    class SamegameTabuColorRandomStrategy : ISPSimulationStrategy
    {
        private int selectedColor;
        private MersenneTwister rnd;
        public SamegameTabuColorRandomStrategy(SamegameGameState initialState,MersenneTwister rng)
        {
            int[] counters = new int[initialState.GetBoard().Count];
            foreach(int value in initialState.GetBoard())
            {
                counters[value]++;
            }
            int max=0;
            int selectedValue=0;
            for(int i = 0; i < counters.Length; i++)
            {
                if (counters[i] > max)
                {
                    max = counters[i];
                    selectedValue = i;
                }
            }
            selectedColor = selectedValue;
            rnd = rng;
        }

        public SamegameTabuColorRandomStrategy(int[][] level, MersenneTwister rng)
        {
            int[] counters = new int[level.Length];
            foreach (int[] row in level)
            {
                foreach (int value in row)
                {
                    counters[value]++;
                }
            }
            int max = 0;
            int selectedValue = 0;
            for (int i = 0; i < counters.Length; i++)
            {
                if (counters[i] > max)
                {
                    max = counters[i];
                    selectedValue = i;
                }
            }
            selectedColor = selectedValue;
            rnd = rng;
        }

        public SamegameTabuColorRandomStrategy(string level, MersenneTwister rng)
        {
            string[] rows = level.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            int[][] levelArray = new int[rows.Length][];
            for(int i=0; i<levelArray.Length;i++)
            {
                levelArray[i] = new int[rows.Length];
            }
            for (int i = 0; i < levelArray.Length; i++)
            {
                for (int j = 0; j < levelArray.Length; j++)
                {
                    int.TryParse(rows[i].ElementAt(j).ToString(), out levelArray[i][j]);
                }
            }
            int[] counters = new int[level.Length];
            foreach (int[] row in levelArray)
            {
                foreach (int value in row)
                {
                    counters[value]++;
                }
            }

            //for (int i = 0; i < counters.Length; i++)
            //{
            //    counters[i] = level.Count(x => x == (char)i);
            //}

            int max = 0;
            int selectedValue = 0;
            for (int i = 0; i < counters.Length; i++)
            {
                if (counters[i] > max)
                {
                    max = counters[i];
                    selectedValue = i;
                }
            }
            selectedColor = selectedValue;
            
            rnd = rng;
        }

        public string getFriendlyName()
        {
            return "TabuColorRandom";
        }

        public string getTypeName()
        {
            return GetType().Name;
        }

        public IPuzzleMove selectMove(IPuzzleState gameState)
        {
            List<IPuzzleMove> moves = gameState.GetMoves();
            
            if (rnd.NextDouble() <= 0.00007) //epsilon greedy
            {
                return moves[rnd.Next(moves.Count)];
            }
            moves.RemoveAll(item => gameState.GetBoard(SamegameGameMove.GetX(item), SamegameGameMove.GetY(item))==selectedColor);
            if (moves.Count == 0)
            {
                moves = gameState.GetMoves();
            }
            IPuzzleMove selectedMove = moves[rnd.Next(moves.Count)];
            return selectedMove;
        }
    }
}
