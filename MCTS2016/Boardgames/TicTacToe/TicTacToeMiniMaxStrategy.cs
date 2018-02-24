using System;
using Common.Abstract;
using System.Collections.Generic;

namespace BoardGames
{
    public class TicTacToeMiniMaxStrategy : ITicTacToeSimulationStrategy
    {
        private int player_id = 0;
        private int opponent_id = 0;
        private int size = 0;
        private int[,] cells = null;
		private int max_depth;

        public TicTacToeMiniMaxStrategy(int _max_depth)
        {
			max_depth = _max_depth;
        }

        public IGameMove selectMove(IGameState state)
        {
			opponent_id = state.playerJustMoved;
			player_id = 3 - opponent_id;

            size = state.size;
            cells = new int[size, size];

            for (int y = 0; y < size; y++)
                for (int x = 0; x < size; x++)
                {
                    cells[y, x] = state.GetBoard(x, y);
                }
            int[] result = minimax(max_depth, player_id);

            return (IGameMove)state.GetPositionIndex(result[2], result[1]);
        }

        #region PRIVATE_METHODS

        private int evaluate(IGameState state)
        {
            int score = 0;
            // Evaluate score for each of the 8 lines (3 rows, 3 columns, 2 diagonals)
            score += evaluateLine(0, 0, 0, 1, 0, 2);  // row 0
            score += evaluateLine(1, 0, 1, 1, 1, 2);  // row 1
            score += evaluateLine(2, 0, 2, 1, 2, 2);  // row 2
            score += evaluateLine(0, 0, 1, 0, 2, 0);  // col 0
            score += evaluateLine(0, 1, 1, 1, 2, 1);  // col 1
            score += evaluateLine(0, 2, 1, 2, 2, 2);  // col 2
            score += evaluateLine(0, 0, 1, 1, 2, 2);  // diagonal
            score += evaluateLine(0, 2, 1, 1, 2, 0);  // alternate diagonal
            return score;
        }

        private bool checkTriplet(int r1, int c1, int r2, int c2, int r3, int c3, int _player_id)
        {
            return cells[r1, c1] == player_id && cells[r2, c2] == player_id && cells[r3, c3] == player_id;
        }

        private string[] strWinningPatterns = new string[]
        {
            "111000000", "000111000", "000000111", // rows
            "100100100", "010010010", "001001001", // cols
            "100010001", "001010100"               // diagonals
        };

        // check winning condition using strings
        private bool hasWon(int _player_id)
        {
            return  checkTriplet(0, 0, 1, 1, 2, 2, _player_id) || // first diagonal
            checkTriplet(0, 2, 1, 1, 2, 0, _player_id) || // second diagonal
            checkTriplet(0, 0, 0, 1, 0, 2, _player_id) || // row 0
            checkTriplet(1, 0, 1, 1, 1, 2, _player_id) || // row 1
            checkTriplet(2, 0, 2, 1, 2, 2, _player_id) || // row 2
            checkTriplet(0, 0, 0, 1, 0, 2, _player_id) || // col 0
            checkTriplet(1, 0, 1, 1, 1, 2, _player_id) || // col 1
            checkTriplet(2, 0, 2, 1, 2, 2, _player_id);   // col 2
        }

        private int evaluateLine(int row1, int col1, int row2, int col2, int row3, int col3)
        {

            int score = 0;

            // First cell
            if (cells[row1, col1] == player_id)
            {
                score = 1;
            }
            else if (cells[row1, col1] == opponent_id)
            {
                score = -1;
            }

            // Second cell
            if (cells[row2, col2] == player_id)
            {
                if (score == 1)
                {   // cell1 is mySeed
                    score = 10;
                }
                else if (score == -1)
                {  // cell1 is oppSeed
                    return 0;
                }
                else
                {  // cell1 is empty
                    score = 1;
                }
            }
            else if (cells[row2, col2] == opponent_id)
            {
                if (score == -1)
                { // cell1 is oppSeed
                    score = -10;
                }
                else if (score == 1)
                { // cell1 is mySeed
                    return 0;
                }
                else
                {  // cell1 is empty
                    score = -1;
                }
            }

            // Third cell
            if (cells[row3, col3] == player_id)
            {
                if (score > 0)
                {  // cell1 and/or cell2 is mySeed
                    score *= 10;
                }
                else if (score < 0)
                {  // cell1 and/or cell2 is oppSeed
                    return 0;
                }
                else
                {  // cell1 and cell2 are empty
                    score = 1;
                }
            }
            else if (cells[row3, col3] == opponent_id)
            {
                if (score < 0)
                {  // cell1 and/or cell2 is oppSeed
                    score *= 10;
                }
                else if (score > 1)
                {  // cell1 and/or cell2 is mySeed
                    return 0;
                }
                else
                {  // cell1 and cell2 are empty
                    score = -1;
                }
            }
            return score;
        }

        private List<int[]> generateMoves()
        {
            List<int[]> nextMoves = new List<int[]>(); // allocate List

            if (hasWon(player_id) || hasWon(opponent_id))
            {
                return nextMoves;   // return empty list
            }

            // Search for empty cells and add to the List
            for (int row = 0; row < size; ++row)
            {
                for (int col = 0; col < size; ++col)
                {
                    if (cells[row, col] == 0)
                    {
                        nextMoves.Add(new int[] { row, col });
                    }
                }
            }
            return nextMoves;
        }

        /** The heuristic evaluation function for the current board
           @Return +100, +10, +1 for EACH 3-, 2-, 1-in-a-line for computer.
                   -100, -10, -1 for EACH 3-, 2-, 1-in-a-line for opponent.
                   0 otherwise   */
        private int evaluate()
        {
            int score = 0;
            // Evaluate score for each of the 8 lines (3 rows, 3 columns, 2 diagonals)
            score += evaluateLine(0, 0, 0, 1, 0, 2);  // row 0
            score += evaluateLine(1, 0, 1, 1, 1, 2);  // row 1
            score += evaluateLine(2, 0, 2, 1, 2, 2);  // row 2
            score += evaluateLine(0, 0, 1, 0, 2, 0);  // col 0
            score += evaluateLine(0, 1, 1, 1, 2, 1);  // col 1
            score += evaluateLine(0, 2, 1, 2, 2, 2);  // col 2
            score += evaluateLine(0, 0, 1, 1, 2, 2);  // diagonal
            score += evaluateLine(0, 2, 1, 1, 2, 0);  // alternate diagonal
            return score;
        }

        private int[] minimax(int depth, int player)
        {

            // Generate possible next moves in a List of int[2] of {row, col}.
            List<int[]> nextMoves = generateMoves();

            // mySeed is maximizing; while oppSeed is minimizing
            int bestScore = (player == player_id) ? int.MinValue : int.MaxValue;

            int currentScore;
            int bestRow = -1;
            int bestCol = -1;

            if (nextMoves.Count == 0 || depth == 0)
            {
                // Gameover or depth reached, evaluate score
                bestScore = evaluate();
            }
            else
            {
                for (int i = 0; i < nextMoves.Count; i++)
                {
                    int[] move = new int[2];
                    move = nextMoves[i];

                    cells[move[0], move[1]] = player;
                    if (player == player_id)
                    {  // mySeed (computer) is maximizing player
                        currentScore = minimax(depth - 1, opponent_id)[0];
                        if (currentScore > bestScore)
                        {
                            bestScore = currentScore;
                            bestRow = move[0];
                            bestCol = move[1];
                        }
                    }
                    else
                    {  // oppSeed is minimizing player
                        currentScore = minimax(depth - 1, player_id)[0];
                        if (currentScore < bestScore)
                        {
                            bestScore = currentScore;
                            bestRow = move[0];
                            bestCol = move[1];
                        }
                    }
                    // Undo move
                    cells[move[0], move[1]] = 0;
                }
            }
            int[] res = new int[3];
            res[0] = bestScore;
            res[1] = bestRow;
            res[2] = bestCol;

            return res;
        }

        #endregion

        public string getTypeName()
        {
            return this.GetType().Name;
        }

        public string getFriendlyName()
        {
            return "MiniMax";
        }

    }
}

