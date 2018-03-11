using Common;
using MCTS2016.BFS;
using MCTS2016.Common.Abstract;
using MCTS2016.Puzzles.SameGame;
using MCTS2016.SP_MCTS;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MCTS2016.Puzzles.Sokoban
{
    class AbstractSokobanState : IPuzzleState
    {
        private SokobanGameState state;
        private List<IPuzzleMove> availableMoves;
        private Position normalizedPlayerPosition;
        private ISPSimulationStrategy simulationStrategy;
        private RewardType rewardType;
        private bool useNormalizedPosition;
        private MersenneTwister rng;
        private bool useTunnelMacro;
        private bool useGoalMacro;
        private bool useGoalCut;
        private GoalMacroTree goalMacroTree;
        private GoalMacroNode currentGoalMacroNode;

        public AbstractSokobanState(SokobanGameState state, RewardType rewardType, bool useNormalizedPosition, bool useGoalMacro, bool useTunnelMacro, bool useGoalCut, ISPSimulationStrategy simulationStrategy = null, MersenneTwister rng = null)
        {
            Init(state, rewardType, useNormalizedPosition, useGoalMacro, useTunnelMacro, useGoalCut, simulationStrategy, rng);
        }

        public AbstractSokobanState(String level, RewardType rewardType, bool useNormalizedPosition, bool useGoalMacro, bool useTunnelMacro, bool useGoalCut, ISPSimulationStrategy simulationStrategy = null, MersenneTwister rng = null)
        {
            Init(new SokobanGameState(level, rewardType, simulationStrategy), rewardType, useNormalizedPosition, useGoalMacro, useTunnelMacro, useGoalCut, simulationStrategy, rng);
        }

        private AbstractSokobanState() { }

        private void Init(SokobanGameState state, RewardType rewardType, bool useNormalizedPosition, bool useGoalMacro, bool useTunnelMacro, bool useGoalCut, ISPSimulationStrategy simulationStrategy, MersenneTwister rng = null)
        {
            this.useNormalizedPosition = useNormalizedPosition;
            this.state = (SokobanGameState)state;
            if (simulationStrategy == null)
            {
                simulationStrategy = new SokobanRandomStrategy();
            }
            if (rng == null)
            {
                rng = new MersenneTwister();
            }
            this.rng = rng;
            this.rewardType = rewardType;
            this.simulationStrategy = simulationStrategy;
            normalizedPlayerPosition = new Position(int.MaxValue, int.MaxValue);
            availableMoves = null;
            this.useGoalMacro = useGoalMacro;
            this.useTunnelMacro = useTunnelMacro;
            this.useGoalCut = useGoalCut;
            if (useGoalMacro)
            {
                goalMacroTree = GoalMacroWrapper.BuildMacroTree(this);
                if (goalMacroTree.Roots.Length > 0)
                { 
                    currentGoalMacroNode = GetInitialMacroNode(goalMacroTree.Roots[0], state.GetGoalMacroHash(goalMacroTree.GoalsInRoom));
                    if(currentGoalMacroNode == null)
                    {
                        this.useGoalMacro = false;
                    }
                }
                else
                {
                    this.useGoalMacro = false;
                }
            }

        }

        private GoalMacroNode GetInitialMacroNode(GoalMacroNode node, int hash)
        {
            if (node.Hashkey == hash)
                return node;
            GoalMacroNode newNode = null;
            foreach (GoalMacroEntry entry in node.Entries)
            {
                newNode = GetInitialMacroNode(entry.Next, hash);
                if (newNode != null)
                {
                    return node;
                }
            }
            return null;
        }

        public int size => ((IPuzzleState)state).size;

        internal SokobanGameState State { get => state; set => state = value; }
        public bool UseGoalCut { get => useGoalCut; set => useGoalCut = value; }
        public bool UseGoalMacro { get => useGoalMacro; set => useGoalMacro = value; }
        public bool UseTunnelMacro { get => useTunnelMacro; set => useTunnelMacro = value; }
        internal RewardType RewardType { get => rewardType; set => rewardType = value; }
        public ISPSimulationStrategy SimulationStrategy { get => simulationStrategy; set => simulationStrategy = value; }
        public MersenneTwister Rng { get => rng; set => rng = value; }

        public IPuzzleState Clone()
        {
            IPuzzleState clone = new AbstractSokobanState()
            {
                state = (SokobanGameState)state.Clone(),
                rewardType = rewardType,
                useNormalizedPosition = useNormalizedPosition,
                useGoalMacro = useGoalMacro,
                useTunnelMacro = useTunnelMacro,
                useGoalCut = useGoalCut,
                simulationStrategy = simulationStrategy,
                rng = rng,
                normalizedPlayerPosition = new Position(int.MaxValue, int.MaxValue),
                availableMoves = null,
                goalMacroTree = goalMacroTree,
                currentGoalMacroNode = currentGoalMacroNode
            };//(SokobanGameState)state.Clone(), rewardType, useNormalizedPosition,useGoalMacro,useTunnelMacro, simulationStrategy, rng);
            return clone;
        }

        public void DoMove(IPuzzleMove move)
        {
            DoAbstractMove(move);
            availableMoves = null;
        }

        public bool EndState()
        {
            return ((IPuzzleState)state).EndState();
        }

        public override bool Equals(object obj)
        {

            return obj != null && obj.GetType().Equals(GetType()) && this.GetHashCode() == obj.GetHashCode();
        }

        public List<int> GetBoard()
        {
            return ((IPuzzleState)state).GetBoard();
        }

        public int GetBoard(int x, int y)
        {
            return ((IPuzzleState)state).GetBoard(x, y);
        }

        public override int GetHashCode()
        {
            if (useNormalizedPosition)
            {
                int hc = state.GetEmptyBoardHash();
                GetMoves();//this is necessary because normalizedPlayer position is updated during the move search (to ensure that the value is assigned)
                hc = (13 * hc) + normalizedPlayerPosition.GetHashCode();
                return hc;
            }
            else
            {
                return state.GetHashCode();
            }
        }

        public List<IPuzzleMove> GetMoves()
        {
            if (availableMoves != null)
            {
                return availableMoves;
            }
            return GetAvailablePushes();
        }

        public int GetPositionIndex(int x, int y)
        {
            return ((IPuzzleState)state).GetPositionIndex(x, y);
        }

        public IPuzzleMove GetRandomMove()
        {
            List<IPuzzleMove> moves = GetMoves();
            int rndIndex = rng.Next(moves.Count);
            return moves[rndIndex];
        }

        public double GetResult()
        {
            return ((IPuzzleState)state).GetResult();
        }

        public int GetScore()
        {
            return ((IPuzzleState)state).GetScore();
        }

        public IPuzzleMove GetSimulationMove()
        {
            return simulationStrategy.selectMove(this);
        }

        public bool isTerminal()
        {
            if (GetMoves().Count() == 0)
            {
                return true;
            }
            return ((IPuzzleState)state).isTerminal();
        }

        public void Pass()
        {
            ((IPuzzleState)state).Pass();
        }

        public string PrettyPrint()
        {
            string s = ((IPuzzleState)state).PrettyPrint();
            List<IPuzzleMove> moves = GetMoves();
            s += normalizedPlayerPosition;
            s += "\n";
            foreach (IPuzzleMove m in moves)
            {
                s += m;
            }
            return s;
        }

        public void Restart()
        {
            ((IPuzzleState)state).Restart();
        }

        public bool StateChanged()
        {
            return ((IPuzzleState)state).StateChanged();
        }

        public override string ToString()
        {
            return PrettyPrint();
        }

        public int Winner()
        {
            return ((IPuzzleState)state).Winner();
        }


        private void DoAbstractMove(IPuzzleMove move)
        {
            SokobanPushMove pushMove = (SokobanPushMove)move;
            foreach (SokobanGameMove m in pushMove.MoveList)
            {
                state.DoMove(m);
            }
            if (pushMove.IsGoalMacro)
            {
                SokobanGameMove lastMove = pushMove.MoveList[pushMove.MoveList.Count - 1];
                state.LockBox(state.BoxPositions[lastMove.BoxIndex]);
            }

        }

        private List<IPuzzleMove> GetAvailablePushes()
        {
            Position normalizedPosition = new Position(int.MaxValue, int.MaxValue);
            HashSet<SokobanGameState> visitedStates = new HashSet<SokobanGameState>();
            List<IPuzzleMove> pushes = new List<IPuzzleMove>();
            List<BFSNodeState> frontier = new List<BFSNodeState>();
            frontier.Add(new BFSNodeState(state, null, null));
            while (frontier.Count() > 0)
            {
                BFSNodeState currentNode = frontier[0];
                SokobanGameState s = (SokobanGameState)currentNode.state;
                if (s.PlayerY < normalizedPosition.Y || s.PlayerY == normalizedPosition.Y && s.PlayerX < normalizedPosition.X)
                {
                    normalizedPosition.X = s.PlayerX;
                    normalizedPosition.Y = s.PlayerY;
                }
                frontier.RemoveAt(0);
                visitedStates.Add(s);

                foreach (SokobanGameMove move in s.GetMoves())
                {
                    SokobanGameState sCopy = (SokobanGameState)s.Clone();
                    if (move.move < 4)//Movement move
                    {
                        sCopy.DoMove(move);
                        if (!visitedStates.Contains(sCopy))//only expand on unvisited states
                        {
                            visitedStates.Add(sCopy);
                            frontier.Add(new BFSNodeState(sCopy, move, currentNode));
                        }
                    }
                    else//Push move
                    {
                        sCopy.DoMove(move);
                        List<SokobanGameMove> movesToPush = new List<SokobanGameMove>() { };
                        BFSNodeState node = currentNode;
                        while (node.parent != null)//build move sequence that lead to push move
                        {
                            movesToPush.Add((SokobanGameMove)node.move);
                            node = node.parent;
                        }
                        movesToPush.Reverse();

                        if (useGoalMacro)
                        {
                            SokobanPushMove goalMacro = GetGoalMacro((SokobanGameState)sCopy, move);
                            if (goalMacro != null)
                            {
                                movesToPush.Add(move);
                                movesToPush.AddRange(goalMacro.MoveList);
                                SokobanPushMove pushMove = new SokobanPushMove(movesToPush, goalMacro.PlayerPosition, movesToPush[movesToPush.Count - 1].BoxIndex);
                                if (useGoalCut)
                                {
                                    return new List<IPuzzleMove>() { pushMove };
                                }
                                else
                                {
                                    pushes.Add(pushMove);
                                }
                            }
                        }
                        if (useTunnelMacro)
                        {
                            List<SokobanGameMove> tunnel = GetTunnelMacro((SokobanGameState)s, move);
                            if (tunnel.Count > 0)
                            {
                                movesToPush.AddRange(tunnel);
                            }
                            else
                            {
                                movesToPush.Add(move);
                            }
                        }
                        else
                        {
                            movesToPush.Add(move);
                        }

                        pushes.Add(new SokobanPushMove(movesToPush, new Position(sCopy.PlayerX, sCopy.PlayerY), movesToPush.Last<SokobanGameMove>().BoxIndex)); //add push move to available pushes
                    }
                }
            }
            normalizedPlayerPosition = normalizedPosition;
            return pushes;
        }

        List<SokobanGameMove> GetTunnelMacro(SokobanGameState state, IPuzzleMove push)
        {
            List<SokobanGameMove> macro = new List<SokobanGameMove>();
            Position boxToPush = null;
            Position pushTarget = null;

            switch (push.ToString())
            {
                case "U":
                    boxToPush = new Position(state.PlayerX, state.PlayerY - 1);
                    pushTarget = new Position(state.PlayerX, state.PlayerY - 2);
                    break;
                case "D":
                    boxToPush = new Position(state.PlayerX, state.PlayerY + 1);
                    pushTarget = new Position(state.PlayerX, state.PlayerY + 2);
                    break;
                case "R":
                    boxToPush = new Position(state.PlayerX + 1, state.PlayerY);
                    pushTarget = new Position(state.PlayerX + 2, state.PlayerY);
                    break;
                case "L":
                    boxToPush = new Position(state.PlayerX - 1, state.PlayerY);
                    pushTarget = new Position(state.PlayerX - 2, state.PlayerY);
                    break;
            }
            if (boxToPush.X == pushTarget.X &&
                state.Board[boxToPush.X + 1, boxToPush.Y] == SokobanGameState.WALL && state.Board[boxToPush.X - 1, boxToPush.Y] == SokobanGameState.WALL) //Vertical push
            {
                while ((state.Board[pushTarget.X, pushTarget.Y] == SokobanGameState.EMPTY || state.Board[pushTarget.X, pushTarget.Y] == SokobanGameState.GOAL) && //keep pushing if possible
                    state.Board[boxToPush.X + 1, boxToPush.Y] == SokobanGameState.WALL && state.Board[boxToPush.X - 1, boxToPush.Y] == SokobanGameState.WALL && //check if still inside tunnel walls
                        !(state.Board[boxToPush.X, boxToPush.Y] == SokobanGameState.GOAL && state.Board[pushTarget.X, pushTarget.Y] == SokobanGameState.EMPTY)) //Stop tunnel macro if goal followed by empty
                {
                    SokobanGameMove move;
                    if (pushTarget.Y > boxToPush.Y)
                    {
                        move = new SokobanGameMove("D");
                        pushTarget.Y++;
                        boxToPush.Y++;
                    }
                    else
                    {
                        move = new SokobanGameMove("U");
                        pushTarget.Y--;
                        boxToPush.Y--;
                    }
                    macro.Add(move);
                }
            }
            else if (state.Board[boxToPush.X, boxToPush.Y + 1] == SokobanGameState.WALL && state.Board[boxToPush.X, boxToPush.Y - 1] == SokobanGameState.WALL)//horizontal push
            {
                while ((state.Board[pushTarget.X, pushTarget.Y] == SokobanGameState.EMPTY || state.Board[pushTarget.X, pushTarget.Y] == SokobanGameState.GOAL) && //keep pushing if possible
                    state.Board[boxToPush.X, boxToPush.Y + 1] == SokobanGameState.WALL && state.Board[boxToPush.X, boxToPush.Y - 1] == SokobanGameState.WALL && //check if still inside tunnel walls
                        !(state.Board[boxToPush.X, boxToPush.Y] == SokobanGameState.GOAL && state.Board[pushTarget.X, pushTarget.Y] == SokobanGameState.EMPTY))//Stop tunnel macro if goal followed by empty
                {
                    SokobanGameMove move;
                    if (pushTarget.X > boxToPush.X)
                    {
                        move = new SokobanGameMove("R");
                        pushTarget.X++;
                        boxToPush.X++;
                    }
                    else
                    {
                        move = new SokobanGameMove("L");
                        pushTarget.X--;
                        boxToPush.X--;
                    }
                    macro.Add(move);
                }
            }
            return macro;
        }


        private SokobanPushMove GetGoalMacro(SokobanGameState state, IPuzzleMove push)
        {
            Position boxToPush = null;

            switch (push.ToString())
            {
                case "U":
                    boxToPush = new Position(state.PlayerX, state.PlayerY - 1);
                    break;
                case "D":
                    boxToPush = new Position(state.PlayerX, state.PlayerY + 1);
                    break;
                case "R":
                    boxToPush = new Position(state.PlayerX + 1, state.PlayerY);
                    break;
                case "L":
                    boxToPush = new Position(state.PlayerX - 1, state.PlayerY);
                    break;
            }
            int stateHash = state.GetGoalMacroHash(goalMacroTree.GoalsInRoom);
            if (stateHash == currentGoalMacroNode.Hashkey)
            {
                foreach (GoalMacroEntry entry in currentGoalMacroNode.Entries)
                {
                    if (boxToPush.Equals(entry.GetEntrancePosition()))
                    {
                        foreach(GoalMacro goalMacro in entry.GoalMacros)
                        {
                            if(state.PlayerX == goalMacro.PlayerPosition.X && state.PlayerY == goalMacro.PlayerPosition.Y)
                            {
                                foreach (IPuzzleMove move in goalMacro.MacroMove.MoveList)
                                {
                                    if (state.GetMoves().Contains(move))
                                    {
                                        state.DoMove(move);
                                    }
                                    else
                                    {
                                        return null;
                                    }
                                }
                                goalMacro.MacroMove.IsGoalMacro = true;
                                return goalMacro.MacroMove;
                            }
                        }
                    }
                }
            }
            return null;
        }

        public void ClearBoardForGoalMacro(List<Position> boxesInGoal, Position goal, Position entrance)
        {
            for (int x = 0; x < state.Board.GetLength(0); x++)
            {
                for (int y = 0; y < state.Board.GetLength(1); y++)
                {
                    if (state.Board[x, y] != SokobanGameState.WALL)
                    {
                        state.Board[x, y] = SokobanGameState.EMPTY;
                    }
                }
            }
            
            state.Board[entrance.X, entrance.Y] = SokobanGameState.BOX;
            state.Board[goal.X,goal.Y] = SokobanGameState.GOAL;
            foreach(Position p in boxesInGoal)
            {
                state.Board[p.X, p.Y] = SokobanGameState.WALL;
            }
        }

        public void SetPlayerPosition(Position player)
        {
            for (int x = 0; x < state.Board.GetLength(0); x++)
            {
                for (int y = 0; y < state.Board.GetLength(1); y++)
                {
                    if (x == player.X && y == player.Y)
                    {
                        if (state.Board[x, y] == SokobanGameState.GOAL)
                            state.Board[x, y] = SokobanGameState.PLAYER_ON_GOAL;
                        else if (state.Board[x, y] == SokobanGameState.EMPTY)
                            state.Board[x, y] = SokobanGameState.PLAYER;
                    }
                }
            }
            state.PlayerX = player.X;
            state.PlayerY = player.Y;
        }
    }
}
