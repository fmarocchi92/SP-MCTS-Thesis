using Common;
using Common.Abstract;
//using System.Linq;
using System.Collections.Generic;
using System;

namespace BoardGames
{
    public class TicTacToeGameState : IGameState
    {
        public int playerJustMoved { get; private set; }

        public int currentPlayer { get; private set; }

        private bool _stateChanged = true;

        private int[] board;

        private string symbols = " ox";


        public int numberOfPlayers { get { return 2; } }

        public int size { get; private set; }

        private int? _winner = null;

		private int no_moves = 9;

        private static int[,] directions =
            {
                { 0, 1 },
                { 1, 0 },
                { 1, 1 },
                { 1, -1 }
            };
        
        private ITicTacToeSimulationStrategy _simulationStrategy;

        public TicTacToeGameState(int size = 3, ITicTacToeSimulationStrategy simulationStrategy = null)
        {
            if (simulationStrategy == null)
            {
                simulationStrategy = new TicTacToeRandomStrategy();
            }

            this._simulationStrategy = simulationStrategy;
            this.size = size;
            board = new int[size * size];

            ClearBoard();
        }

        // get available moves
        public List<IGameMove> GetMoves()
        {
            // returns a copy of the available moves list
            var m = new List<IGameMove>();

            if (_winner.Equals(null))
            {
                for (var i = 0; i < this.size; i++)
                {
                    for (var j = 0; j < this.size; j++)
                    {
                        if (board[i * this.size + j] == 0)
                        {
                            m.Add(new TicTacToeGameMove(i, j, this));
                        }
                    }
                }
            }
            return m;
        }

        // get available moves
        public List<int> GetBoard()
        {
            // returns a copy of the available moves list
            return new List<int>(board);
        }
                        
        // get available moves
        public int GetBoard(int x, int y)
        {
            // returns a copy of the available moves list
            return board[x + y * size];
        }

        //
        // generate a random move for the current player
        // returns -1 if no move is available
        //
        public IGameMove GetRandomMove()
        {
            List<IGameMove> moves = GetMoves();
            return moves[RNG.Next(moves.Count)];
        }

        // clone the current state
        public IGameState Clone()
        { 

            TicTacToeGameState st = new TicTacToeGameState(size);

			// copy the board
			st.board = (int[]) board.Clone ();

			// copy the other fields
            st.currentPlayer = currentPlayer;
            st.playerJustMoved = playerJustMoved;
            st.size = size;
            st._stateChanged = _stateChanged;

            return st;
        }

        public int GetPositionIndex(int x, int y)
        {
            return y * size + x;
        }

        // current player performs a move
        public void DoMove(IGameMove move)
        {
            TicTacToeGameMove tttMove = new TicTacToeGameMove(move, this);

            if (move < 0 || move > board.Length || board[move] != 0)
            {
                throw new ArgumentException("Wrong Move!");
            }

            _stateChanged = false;

            board[move] = currentPlayer;
			no_moves--;

            _stateChanged = true;

			if (CheckWinDirections (currentPlayer, tttMove.GetX, tttMove.GetY)) {
				// there is a winner
				_winner = currentPlayer;
			} else if (no_moves == 0) {
				// otherwise if there are no more moves there is a tie
				_winner = 0;
			}

            this.playerJustMoved = this.currentPlayer;
            this.currentPlayer = 3 - this.currentPlayer;
        }

        // represent the game state as a string
        public string PrettyPrint()
        {
            return this.ToString();

            string s = string.Empty;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    s += symbols[board[x + y * size]];
                    if (x != (size - 1))
                        s += "|";
                }
                if (y != (size - 1))
                    s += "\n------\n";
            }

            s += "\n\n=========\n";
            for (int i = 0; i < size * size; i++)
                s += board[i];
            s += "\n=========\n";
            return s;
        }

        // represent the game state as a string
        public override string ToString()
        {
            string s = string.Empty;
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    s += board[x + y * size];
                }
            }
            return s;
        }

        public int Winner()
        {
            if (EndState())
            {
                return _winner.Value;
            }
            return -1;
        }

        // true if the game is finished
        public bool EndState()
        {
            return _winner.HasValue;
        }

        public bool isTerminal()
        {
            return GetMoves().Count == 0 || EndState();
        }

        // reset the game state and set the current player
        public void Restart(int _player = 1)
        {
			// clear the board
            ClearBoard();

			// set the starting player
            this.currentPlayer = _player;

			// makes it like the opponent had just moved (needed for minmax and mcts to alternate players)
			this.playerJustMoved = 3 - _player;
        }

        // the state has changed due to the move
        public bool StateChanged()
        {
            return _stateChanged;
        }


        private bool CheckWinDirections(int player, int x, int y)
        {
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                if (Near(player, x, y, directions[i, 0], directions[i, 1]) + Near(player, x, y, -directions[i, 0], -directions[i, 1]) >= 2)
                {
                    return true;
                }
            }

            return false;
        }

        private int Near(int player, int x, int y, int dx, int dy)
        {
            int count = 0;
            int i = x + dx;
            int j = y + dy;
            while (0 <= i && i < size && 0 <= j && j < size && board[GetPositionIndex(i, j)] == player)
            {
                count++;
                i = i + dx;
                j = j + dy;
            }
            return count;
        }

        // init the board to the initial state
        private void ClearBoard()
        {
            for (int x = 0; x < size; x++)
            {
                for (int y = 0; y < size; y++)
                {
                    board[x + y * size] = 0;
                }
            }

            _winner = null;
			no_moves = 9; 
        }

        public double GetResult(int player)
        {
            if (Winner() == 0)
            {
                return 0.5;
            }
            if (Winner() == player)
            {
                return 1.0;
            }
            return 0.0;
        }

        public IGameMove GetSimulationMove()
        {
            return this._simulationStrategy.selectMove(this);
        }

        public int GetScore(int player)
        {
            return -1;
        }

        // There's no concept of pass in Tic Tac Toe
        public void Pass()
        {
            return;
        }
    }
}
