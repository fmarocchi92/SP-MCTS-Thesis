using Common;
using Common.Abstract;

namespace BoardGames
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Forza4GameState : IGameState
    {
        public static int[,] ParseBoard(IGameState gameState, int height, int width)
        {
            int[,] _board = new int[height, width];

            for (var rowIndex = 0; rowIndex < height; rowIndex++)
            {
                for (var colIndex = 0; colIndex < width; colIndex++)
                {
                    _board[rowIndex, colIndex] = gameState.GetBoard(rowIndex, colIndex);
                }
            }

            return _board;
        }

        public int playerJustMoved { get; private set; }

        public int currentPlayer { get; private set; }

        public int numberOfPlayers { get { return 2; } }

        public int size { get; private set; }

        public int[,] board { get; private set; }

        private int _cols;
        private int _rows;
        private int? _winner = null;
        private bool _stateChanged = true;
        private List<IGameMove> _moves = new List<IGameMove>();
        private IForza4SimulationStrategy simulationStrategy;

        private static int[,] directions =
            {
                { 0, 1 },
                { 1, 0 },
                { 1, 1 },
                { 1, -1 }
            };

        public Forza4GameState(int rows = 6, int cols = 7, IForza4SimulationStrategy simulationStrategy = null)
        {
            if (simulationStrategy == null)
            {
                simulationStrategy = new Forza4RandomStrategy();
            }
            this.simulationStrategy = simulationStrategy;

            this.currentPlayer = 1;
            this._cols = cols;
            this._rows = rows;
            this.size = _rows * _cols;
            ClearBoard();
        }

        public IGameMove GetRandomMove()
        {
            List<IGameMove> moves = GetMoves();
            return moves[RNG.Next(moves.Count)];
        }

        public List<IGameMove> GetMoves()
        {
            _moves = new List<IGameMove>();
            if (_winner.Equals(null))
            {
                for (int j = 0; j < _cols; j++)
                {
                    if (board[0, j] == 0)
                    {
                        _moves.Add(new Forza4GameMove(j));
                    }
                }
            }

            return _moves;
        }

        public int[,] GetForza4Board()
        {
            return this.board;
        }

        public List<int> GetBoard()
        {
            return this.board.Cast<int>().ToList();
        }

        public int GetBoard(int rowIndex, int columnIndex)
        {
            return this.board[rowIndex, columnIndex];
        }

        public void DoMove(IGameMove move)
        {
            Forza4GameMove f4Move = new Forza4GameMove(move.move);
//            _stateChanged = true;
            for (int i = _rows - 1; i >= 0; i--)
            {
                if (board[i, f4Move.move] == 0)
                {
                    board[i, f4Move.move] = this.currentPlayer;
                    if (CheckWinDirections(this.currentPlayer, i, f4Move.move))
                    {
                        _winner = this.currentPlayer;
                    }
                    this.playerJustMoved = this.currentPlayer;
                    this.currentPlayer = 3 - this.currentPlayer;
                    break;
                }
            }
        }

        public override string ToString()
        {
            string s = "";
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _cols; j++)
                {
                    s += board[i, j] + " ";
                }
                s += "\n";
            }
            return s;
        }

        //TODO: Make it pretty
        public string PrettyPrint()
        {
            string s = "";
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _cols; j++)
                {
                    s += board[i, j] /*+ " "*/;
                }
                //s += "\n";
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

        public bool EndState()
        {
            return _winner.HasValue;
        }

        public bool isTerminal()
        {
            return GetMoves().Count == 0 || EndState();
        }

        public void Restart(int _player = 1)
        {
            ClearBoard();
            currentPlayer = _player;
        }

        public bool StateChanged()
        {
            return _stateChanged;
        }

        public int GetPositionIndex(int x, int y)
        {
            return y * this._rows + x;
        }

        private void ClearBoard()
        {
            board = new int[_rows, _cols];
            _moves.Clear();
            _winner = null;
            //_stateChanged = true;
        }

        private bool CheckWinDirections(int player, int x, int y)
        {
            for (int i = 0; i < directions.GetLength(0); i++)
            {
                if (Near(player, x, y, directions[i, 0], directions[i, 1]) + Near(player, x, y, -directions[i, 0], -directions[i, 1]) >= 3)
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
            while (0 <= i && i < _rows && 0 <= j && j < _cols && board[i, j] == player)
            {
                count++;
                i = i + dx;
                j = j + dy;
            }
            return count;
        }

        public double GetResult(int player)
        {
            if (_winner == 0)
            {
                return 0.5;
            }
            if (_winner == player)
            {
                return 1.0;
            }
            return 0.0;
        }

        //        public IGameMove GetSimulationMove()
        //        {
        //            return _simulationStrategy.selectMove(IGameMove.toIGameMoves(GetMoves()));
        //        }

        public IGameState Clone()
        {
            Forza4GameState st = new Forza4GameState(_rows, _cols);
            for (int i = 0; i < _rows; i++)
            {
                for (int j = 0; j < _cols; j++)
                {
                    st.board[i, j] = board[i, j];
                }
            }
            st.currentPlayer = this.currentPlayer;
            st._winner = _winner;
            st._rows = _rows;
            st._cols = _cols;
            st.size = size;

            return st;
        }

        public IGameMove ParseMove(string move)
        {
            return new Forza4GameMove(int.Parse(move));
        }

        public int GetScore(int player)
        {
            return -1;
        }

        public void Pass()
        {
            // no concept of pass in connect 4, a player can always play
            return;
        }

        public IGameMove GetSimulationMove()
        {
            return simulationStrategy.selectMove(this);
        }
    }
}