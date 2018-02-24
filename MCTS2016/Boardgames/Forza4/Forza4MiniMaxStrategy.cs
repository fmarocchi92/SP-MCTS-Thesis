using System;
using System.Linq;
using Common.Abstract;

namespace BoardGames
{
    public class Forza4MiniMaxStrategy : IForza4SimulationStrategy
    {
        // http://users.softlab.ntua.gr/~ttsiod/score4.html
        private int width = 7;
        private int height = 6;
        private int maxDepth = 7;
        private const int orangeWins = 1000000;
        private const int yellowWins = -orangeWins;

        private enum Mycell
        {
            Orange = 1,
            Yellow = -1,
            Barren = 0
		};

		public Forza4MiniMaxStrategy(int _maxDepth = 7) {
			maxDepth = _maxDepth;
		}

        public IGameMove selectMove(IGameState gameState)
        {
            int move, score;

            // Call MiniMax by adapting the board given that it uses -1 to refer to the second player
            // rather than using two as we do in the gameState
            abMinimax(true, Mycell.Orange, maxDepth, gameState.GetBoard().Select((int cell) =>
                    {
                        return cell == 2 ? -1 : cell;
                    }).ToArray(), out move, out score);

            return (IGameMove)move;
        }


        private int ScoreBoard(int[] board)
        {
            int[] counters = { 0, 0, 0, 0, 0, 0, 0, 0, 0 };

            // Horizontal spans
            for (int y = 0; y < height; y++)
            {
                int score = (int)board[width * (y) + 0] + (int)board[width * (y) + 1] + (int)board[width * (y) + 2];
                for (int x = 3; x < width; x++)
                {
                    score += (int)board[width * (y) + x];
                    counters[score + 4]++;
                    score -= (int)board[width * (y) + x - 3];
                }
            }
            // Vertical spans
            for (int x = 0; x < width; x++)
            {
                int score = (int)board[width * (0) + x] + (int)board[width * (1) + x] + (int)board[width * (2) + x];
                for (int y = 3; y < height; y++)
                {
                    score += (int)board[width * (y) + x];
                    counters[score + 4]++;
                    score -= (int)board[width * (y - 3) + x];
                }
            }
            // Down-right (and up-left) diagonals
            for (int y = 0; y < height - 3; y++)
            {
                for (int x = 0; x < width - 3; x++)
                {
                    int score = 0;
                    for (int ofs = 0; ofs < 4; ofs++)
                    {
                        int yy = y + ofs;
                        int xx = x + ofs;
                        score += (int)board[width * (yy) + xx];
                    }
                    counters[score + 4]++;
                }
            }
            // up-right (and down-left) diagonals
            for (int y = 3; y < height; y++)
            {
                for (int x = 0; x < width - 3; x++)
                {
                    int score = 0;
                    for (int ofs = 0; ofs < 4; ofs++)
                    {
                        int yy = y - ofs;
                        int xx = x + ofs;
                        score += (int)board[width * (yy) + xx];
                    }
                    counters[score + 4]++;
                }
            }

            if (counters[0] != 0)
                return yellowWins;
            else if (counters[8] != 0)
                return orangeWins;
            else
                return
                    counters[5] + 2 * counters[6] + 5 * counters[7] -
                counters[3] - 2 * counters[2] - 5 * counters[1];
        }

        private int dropDisk(int[] board, int column, Mycell color)
        {
            for (int y = height - 1; y >= 0; y--)
                if (board[width * (y) + column] == (int)Mycell.Barren)
                {
                    board[width * (y) + column] = (int)color;
                    return y;
                }
            return -1;
        }

        private void abMinimax(bool maximizeOrMinimize, Mycell color, int depth, int[] board, out int move, out int score)
        {
            if (0 == depth)
            {
                move = -1;
                score = ScoreBoard(board);
            }
            else
            {
                int bestScore = maximizeOrMinimize ? -10000000 : 10000000;
                int bestMove = -1;
                for (int column = 0; column < width; column++)
                {
                    if (board[width * (0) + column] != (int)Mycell.Barren)
                    {
                        continue;
                    }
                    int rowFilled = dropDisk(board, column, color);
                    if (rowFilled == -1)
                        continue;
                    
                    int s = ScoreBoard(board);
                    if (s == (maximizeOrMinimize ? orangeWins : yellowWins))
                    {
                        bestMove = column;
                        bestScore = s;
                        board[width * (rowFilled) + column] = (int)Mycell.Barren;
                        break;
                    }

                    int moveInner, scoreInner;

                    if (depth > 1)
                    {
                        abMinimax(!maximizeOrMinimize, color == Mycell.Orange ? Mycell.Yellow : Mycell.Orange, depth - 1, board, out moveInner, out scoreInner);
                    }
                    else
                    {
                        moveInner = -1;
                        scoreInner = s;
                    }

                    board[width * (rowFilled) + column] = (int)Mycell.Barren;

                    /* when loss is certain, avoid forfeiting the match, by shifting scores by depth... */
                    if (scoreInner == orangeWins || scoreInner == yellowWins)
                    {
                        scoreInner -= depth * (int)color;
                    }

//                    if (depth == maxDepth)
//                    {
//                        Console.WriteLine("Depth {0}, placing on {1}, score:{2}", depth, column, scoreInner);
//                    }

                    if (maximizeOrMinimize)
                    {
                        if (scoreInner >= bestScore)
                        {
                            bestScore = scoreInner;
                            bestMove = column;
                        }
                    }
                    else
                    {
                        if (scoreInner <= bestScore)
                        {
                            bestScore = scoreInner;
                            bestMove = column;
                        }
                    }
                }
                move = bestMove;
                score = bestScore;
            }
        }

        public string getTypeName()
        {
            return this.GetType().Name;
        }

        public string getFriendlyName()
        {
            return "AlphaBetaMiniMax";
        }
    }
}

