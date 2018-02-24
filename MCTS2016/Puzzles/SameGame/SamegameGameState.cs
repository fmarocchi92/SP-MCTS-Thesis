using Common;
using Common.Abstract;
using MCTS2016.Common.Abstract;
using MCTS2016.SP_MCTS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Puzzles.SameGame
{
    class SamegameGameState : IPuzzleState
    {

        public int size {get;set;}

        private List<List<int>> board;

        private int score;

        private bool stateChanged = false;

        private ISPSimulationStrategy simulationStrategy;

        private MersenneTwister rnd;

        private SamegameGameState()
        {

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="levelBoardToTranspose">An array containing the rows of the level</param>
        /// <param name="sim"></param>
        public SamegameGameState(int[][] levelBoardToTranspose, MersenneTwister rng,ISPSimulationStrategy sim = null)
        {
            //Transform arrays into lists 
            List<List<int>> levelBoard = new List<List<int>>();
            for (int i = 0; i < levelBoardToTranspose.Length; i++)
            {
                List<int> newList = new List<int>();
                for (int j = 0; j < levelBoardToTranspose[i].Length; j++)
                {
                    newList.Add(levelBoardToTranspose[j][i]);
                }
                levelBoard.Add(newList);
            }

            for (int i= 0;i<levelBoard.Count;i++)
            {
                levelBoard[i].Reverse();
            }
            InitState(levelBoard, sim, rng);
            
        }
        public SamegameGameState(List<List<int>> levelBoard, MersenneTwister rng, ISPSimulationStrategy sim = null)
        {
            InitState(levelBoard, sim, rng);
        }

        private void InitState(List<List<int>> levelBoard, ISPSimulationStrategy sim,MersenneTwister rng)
        {
            rnd = rng;
            board = levelBoard;
            size = board.Count;
            if (sim == null)
            {
                simulationStrategy = new SamegameRandomStrategy();
            }
            else
            {
                simulationStrategy = sim;
            }
        }
        
        
        public SamegameGameState(string level, MersenneTwister rng, ISPSimulationStrategy sim = null)
        {
            rnd = rng;
            String[] levelRows = level.Split(new string[] { "\r\n", "\r", "\n" }, StringSplitOptions.RemoveEmptyEntries);
            board = new List<List<int>>();
            foreach (string row in levelRows)
            {
                board.Add(new List<int>());
            }
            int x;
            int val;
            foreach (string row in levelRows)
            {
                x = 0;
                foreach (char c in row)
                {
                    int.TryParse(c.ToString(), out val);
                    board[x].Add(val);
                    x++;
                }
            }
            foreach(List<int> column in board)
            {
                column.Reverse();
            }
            size = levelRows.Length;

            if (sim == null)
            {
                simulationStrategy = new SamegameRandomStrategy();
            }
            else
            {
                simulationStrategy = sim;
            }
        }

        public IPuzzleState Clone()
        {
            List<List<int>> boardCopy = new List<List<int>>(); ;
            foreach(List<int> column in board)
            {
                List<int> newColumn = new List<int>();
                foreach(int value in column)
                {
                    newColumn.Add(value);
                }
                boardCopy.Add(newColumn);
            }
            return new SamegameGameState()
            {
                board = boardCopy,
                simulationStrategy = this.simulationStrategy,
                score = this.score,
                size = this.size,
                rnd = this.rnd
            };
        }

        public void DoMove(IPuzzleMove move)
        {
            stateChanged = false;
            SamegameGameMove sgmove = move as SamegameGameMove;
            int value = board[sgmove.x][sgmove.y];
            HashSet<Position> toRemove = new HashSet<Position>();
            CheckAdjacentBlocks(sgmove.x, sgmove.y, value, toRemove); //remove adjacent blocks
            if(toRemove.Count > 0)
            {
                board[sgmove.x][sgmove.y] = 1000;
                score += (int) Math.Pow((toRemove.Count() - 2) ,2);
                stateChanged = true;
            }
            foreach (Position position in toRemove) { 
                board[position.X][position.Y] = 1000;
            }
            for(int i = 0; i < board.Count; i++)
            {
                board[i].RemoveAll(v => v == 1000);
            }
            board.RemoveAll(column => column.Count == 0); //remove empty columns
        }
        /// <summary>
        /// Recursively find adjacent blocks with the same value and put them into toRemove
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="value"></param>
        /// <param name="toRemove"></param>
        private void CheckAdjacentBlocks(int x, int y, int value, HashSet<Position> toRemove) //should I use "out" for toRemove?
        {
            if(x > 0 && board[x - 1].Count > y)
            {
                if(board[x-1][y] == value && !toRemove.Contains(new Position(x - 1, y)))
                {
                    toRemove.Add(new Position(x - 1, y));
                    CheckAdjacentBlocks(x - 1, y, value, toRemove);
                }
            }
            if(x < board.Count -1 && board[x + 1].Count > y)
            {
                if (board[x + 1][y] == value && !toRemove.Contains(new Position(x + 1, y)))
                {
                    toRemove.Add(new Position(x + 1, y));
                    CheckAdjacentBlocks(x + 1, y, value, toRemove);
                }
            }
            if (y > 0)
            {
                if (board[x][y-1] == value && !toRemove.Contains(new Position(x, y - 1)))
                {
                    toRemove.Add(new Position(x, y - 1));
                    CheckAdjacentBlocks(x, y - 1, value, toRemove);
                }
            }
            if (y < board[x].Count -1)
            {
                if (board[x][y + 1] == value && !toRemove.Contains(new Position(x, y + 1)))
                {
                    toRemove.Add(new Position(x, y + 1));
                    CheckAdjacentBlocks(x, y + 1, value, toRemove);
                }
            }
        }

        public bool EndState()
        {
            return (board.Count == 0);
        }

        public List<int> GetBoard()
        {
            List<int> boardList = new List<int>();
            for(int i = 0; i < board.Count; i++)
            {
                boardList.AddRange(board[i]);
            }
            return boardList;
        }

        public int GetBoard(int x, int y)
        {
            return board[x][y];
        }

        public List<IPuzzleMove> GetMoves()
        {
            List<IPuzzleMove> moves  = new List<IPuzzleMove>();
            HashSet<Position> alreadyChecked = new HashSet<Position>();
            for (int x = 0; x < board.Count; x++)
            {
                for(int y = 0; y < board[x].Count; y++)
                {
                    if(!alreadyChecked.Contains(new Position(x, y))) //only check for unchecked blocks
                    {
                        int previousCheckedCount = alreadyChecked.Count;
                        //HashSet<Position> group = new HashSet<Position>();
                        CheckAdjacentBlocks(x, y, board[x][y], alreadyChecked); //group adjacent blocks together to have a single action for all of them
                        if (alreadyChecked.Count> previousCheckedCount)
                        {
                            moves.Add(new SamegameGameMove(x, y));
                            //alreadyChecked.UnionWith(group);
                        }
                    }
                }
            }
            return moves;
        }

        private bool HasMoves()
        {
            for (int x = 0; x < board.Count; x++)
            {
                for (int y = 0; y < board[x].Count; y++)
                {
                    int value = board[x][y];
                    if (x > 0 && board[x - 1].Count > y)
                    {
                        if (board[x - 1][y] == value)
                        {
                            return true;
                        }
                    }
                    if (x < board.Count - 1 && board[x + 1].Count > y)
                    {
                        if (board[x + 1][y] == value )
                        {
                            return true;
                        }
                    }
                    if (y > 0)
                    {
                        if (board[x][y - 1] == value )
                        {
                            return true;
                        }
                    }
                    if (y < board[x].Count - 1)
                    {
                        if (board[x][y + 1] == value)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        //it's slower than the recursive version
        //void CheckAdjacentBlocksIterative(int x, int y, int value, HashSet<Position> toRemove)
        //{
        //    List<Position> frontier = new List<Position>();
        //    frontier.Add(new Position(x, y));
        //    while (frontier.Count > 0)
        //    {
        //        Position current = frontier.First<Position>();
        //        if (current.X > 0 && board[current.X - 1].Count > current.Y)
        //        {
        //            if (board[current.X - 1][current.Y] == value && !toRemove.Contains(new Position(current.X - 1, current.Y)))
        //            {
        //                toRemove.Add(new Position(current.X - 1, current.Y));
        //                frontier.Add(new Position(current.X - 1, current.Y));
        //            }
        //        }
        //        if (current.X < board.Count - 1 && board[current.X + 1].Count > current.Y)
        //        {
        //            if (board[current.X + 1][current.Y] == value && !toRemove.Contains(new Position(current.X + 1, current.Y)))
        //            {
        //                toRemove.Add(new Position(current.X + 1, current.Y));
        //                frontier.Add(new Position(current.X + 1, current.Y));
        //            }
        //        }
        //        if (current.Y > 0)
        //        {
        //            if (board[current.X][current.Y - 1] == value && !toRemove.Contains(new Position(current.X, current.Y - 1)))
        //            {
        //                toRemove.Add(new Position(current.X, current.Y - 1));
        //                frontier.Add(new Position(current.X, current.Y - 1));
        //            }
        //        }
        //        if (current.Y < board[current.X].Count - 1)
        //        {
        //            if (board[current.X][current.Y + 1] == value && !toRemove.Contains(new Position(current.X, current.Y + 1)))
        //            {
        //                toRemove.Add(new Position(current.X, current.Y + 1));
        //                frontier.Add(new Position(current.X, current.Y + 1));
        //            }
        //        }
        //        frontier.Remove(current);
        //    }
        //}

        public int GetPositionIndex(int x, int y)
        {
            int i = 0;
            for(int j = 0; j < x; j++)
            {
                i += board[j].Count;
            }
            i += y;
            return i;
        }

        public IPuzzleMove GetRandomMove()
        {
            List<IPuzzleMove> moves = GetMoves();
            int rndIndex = rnd.Next(moves.Count);
            return moves[rndIndex];
        }

        public double GetResult()
        {
            return GetScore();
        }

        public int GetScore()
        {
            int finalScore = score;
            if(board.Count == 0) //bonus of 1000 if the board is empty 
            {
                finalScore += 1000;
                //Debug.WriteLine("Emptied board: score = "+finalScore);
            }
            if (board.Count > 0 && isTerminal()) //penalty of (number of blocks left -2)^2 if at the end the board is not empty
            {
                int remainingBlocks = 0;
                for(int i = 0; i < board.Count; i++)
                {
                    remainingBlocks += board[i].Count;
                }
                if (remainingBlocks > 2)
                {
                    finalScore -= (remainingBlocks - 2) * (remainingBlocks - 2);
                }
            }
            return finalScore;
        }

        public IPuzzleMove GetSimulationMove()
        {
            return simulationStrategy.selectMove(this);
        }

        public bool isTerminal()
        {
            return EndState() || !HasMoves();
        }

        public void Pass()
        {
            throw new NotImplementedException();
        }

        public string PrettyPrint()//prints the board rotated 90° clockwise
        {
            string s = "";
            foreach(List<int> column in board)
            {
                foreach(int value in column)
                {
                    s += value.ToString();
                }
                if (board.Last<List<int>>() != column)
                {
                    s += "\n";
                }
            }
            return s;
        }

        public void Restart()
        {
            throw new NotImplementedException();
        }

        public bool StateChanged()
        {
            return stateChanged;
        }

        public int Winner()
        {
            throw new NotImplementedException();
        }
    }

    class Position
    {
        public int X { get; set; }
        public int Y { get; set; }

        public static int Id(int x,int y)
        {
            return x * 1000 + y;
        }
        
        public Position(int x,int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            var position = obj as Position;
            return position != null &&
                   X == position.X &&
                   Y == position.Y;
        }

        public override int GetHashCode()
        {
            var hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();
            return hashCode;
        }

        public override string ToString()
        {
            return "["+X+","+Y+"]";
        }
    }
}
