using Common;
using Common.Abstract;
using GraphAlgorithms;
using MCTS2016.Common.Abstract;
using MCTS2016.Puzzles.SameGame;
using MCTS2016.Puzzles.Sokoban;
using MCTS2016.SP_MCTS;
using MCTS2016.IDAStar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MCTS2016.SP_MCTS.SP_UCT;
using MCTS2016.SP_MCTS.Optimizations;
using MCTS2016.SP_MCTS.Optimizations.UCT;

namespace MCTS2016
{
    class SinglePlayerMCTSMain
    {
        private static Object taskLock = new object();
        private static int[] taskTaken;
        private static int[] scores;
        private static bool[] solved;
        private static List<IPuzzleMove>[] bestMoves;
        private static int currentTaskIndex=0;
        private static uint threadIndex;
        private static int restarts;
        static TextWriter textWriter;

        static bool useNormalizedPosition;
        static bool useTunnelMacro;
        static bool useGoalMacro;
        static bool useGoalCut;
        static bool ucb1Tuned;
        static bool rave;
        static bool nodeRecycling;
        static int memoryBudget;
        static bool avoidCycles;
        static bool useNodeElimination;
        static int raveThreshold;

        static SimulationType simulationType;
        static int maxNodes;
        static int maxDepth;
        static int tableSize;


        public static void Main(string[] args)
        {
            string game;
            int iterations;
            int maxCost;
            RewardType rewardType;
            double const_C;
            double epsilon = 1;
            double const_D;
            int restarts;
            uint seed;
            bool stopOnResult;
            string level;

            if (args.Length != 3)
            {
                throw new ArgumentException("Need three arguments: game method level");
            }
            //textWriter = File.WriteAllText("Log.txt");
            //File.Delete("Log.txt");
            textWriter = File.AppendText("Log.txt");


            string[] gameSettings = args[0].Split(':');
            string[] commands = args[1].Split(':');

            level = args[2];
            string configurationString = "\n\n\n********************************\n********************************\nBEGIN TASK:\nGame: " + gameSettings[0];
            switch (gameSettings[0]) {
                case "sokoban":
                    if (!bool.TryParse(gameSettings[1], out useNormalizedPosition))
                    {
                        PrintInputError("useNormalizedPosition requires a bool value");
                        return;
                    }
                    if (!bool.TryParse(gameSettings[2], out useTunnelMacro))
                    {
                        PrintInputError("useTunnelMacro requires a bool value");
                        return;
                    }
                    if (!bool.TryParse(gameSettings[3], out useGoalMacro))
                    {
                        PrintInputError("useGoalMacro requires a bool value");
                        return;
                    }
                    if (!bool.TryParse(gameSettings[4], out useGoalCut))
                    {
                        PrintInputError("useGoalCut requires a bool value");
                        return;
                    }
                    configurationString += "\nUse Normalized Position: " + useNormalizedPosition + "\nUse Tunnel Macro: " + useTunnelMacro + "\nUse Goal Macro: " + useGoalMacro + (useGoalMacro ? "\nUse Goal Cut: " + useGoalCut : "\nGoal Cut ignored with Goal Macro disabled") +"\n";
                    switch (commands[0])
                    {
                        case "mcts":
                            if (!int.TryParse(commands[1], out iterations))
                            {
                                PrintInputError("iterations requires an integer value");
                                return;
                            }
                            if (!double.TryParse(commands[2], out const_C))
                            {
                                PrintInputError("Const_C requires a double value");
                                return;
                            }
                            if (!double.TryParse(commands[3], out const_D))
                            {
                                PrintInputError("Const_D requires a double value");
                                return;
                            }
                            if (!RewardType.TryParse(commands[4], out rewardType))
                            {
                                PrintInputError("reward type requires an valid RewardType");
                                return;
                            }
                            if(!bool.TryParse(commands[5], out stopOnResult))
                            {
                                PrintInputError("stop on result requires a boolean value");
                                return;
                            }
                            if(!uint.TryParse(commands[6], out seed))
                            {
                                PrintInputError("seed requires an unsigned integer value");
                                return;
                            }
                            if (!bool.TryParse(commands[7], out ucb1Tuned))
                            {
                                PrintInputError("UCB1 tuned requires a boolean value");
                                return;
                            }
                            if (!bool.TryParse(commands[8], out rave))
                            {
                                PrintInputError("rave requires a boolean value");
                                return;
                            }
                            if (!int.TryParse(commands[9], out raveThreshold))
                            {
                                PrintInputError("Rave threshold requires an integer value");
                                return;
                            }
                            if (!bool.TryParse(commands[10], out nodeRecycling))
                            {
                                PrintInputError("node recycling requires a boolean value");
                                return;
                            }
                            if (!int.TryParse(commands[11], out memoryBudget))
                            {
                                PrintInputError("Memory budget requires an integer value");
                                return;
                            }
                            if (!bool.TryParse(commands[12], out avoidCycles))
                            {
                                PrintInputError("Avoid Cycles requires a boolean value");
                                return;
                            }
                            if (!bool.TryParse(commands[13], out useNodeElimination))
                            {
                                PrintInputError("Use Node Elimination requires a boolean value");
                                return;
                            }
                            if(!SimulationType.TryParse(commands[14], out simulationType))
                            {
                                PrintInputError("Use Node Elimination requires a boolean value");
                                return;
                            }
                            string simulationString = "";
                            switch (simulationType)
                            {
                                case SimulationType.EpsilonGreedy:
                                    if (!double.TryParse(commands[15], out epsilon))
                                    {
                                        PrintInputError("epsilon requires a double value");
                                        return;
                                    }
                                    simulationString += "\nSimulation:  EpsilonGreedy - epsilon: " + epsilon;
                                    break;
                                case SimulationType.IDAstar:
                                    if (!int.TryParse(commands[15], out maxNodes))
                                    {
                                        PrintInputError("maxCost requires an integer value");
                                        return;
                                    }
                                    if (!int.TryParse(commands[16], out tableSize))
                                    {
                                        PrintInputError("maxTableSize requires an integer value");
                                        return;
                                    }
                                    simulationString += "\nSimulation: IDA* - maxNodes: " + maxNodes + "; tableSize: " + tableSize;
                                    break;
                            }
                            
                            if (nodeRecycling && (memoryBudget <= 0 || memoryBudget >= iterations))
                            {
                                PrintInputError("Memory budget value not compatible with node recycling");
                                return;
                            }
                            configurationString += "\nSP-MCTS \nMethod: MCTS \niterations: " + iterations + "\nUCT constant: " + const_C+ "\nSP_UCT constant: "+ const_D +  "\nReward Type: "+rewardType+
                                "\nUCB1Tuned: "+ucb1Tuned+"\nRAVE: "+rave+"\nRAVE Threshold: "+raveThreshold+"\nNode Recycling: "+nodeRecycling+"\nMemory Budget: "+(nodeRecycling?""+memoryBudget:"Ignored with node recycling disabled")+"\nAvoid Cycles: "+avoidCycles+"\nNode Elimination:"+useNodeElimination+"\nStop on Result: "+stopOnResult+"\nlevel path: "+ level + "\nseed: "+seed+
                                simulationString+"\n********************************\n********************************\n";
                            Log(configurationString);
                            MultiThreadSokobanTest(const_C, const_D, iterations, 1, level, seed, true, rewardType, stopOnResult, epsilon, true, 1);
                            break;
                        case "ida":
                            if (!int.TryParse(commands[1], out maxCost))
                            {
                                PrintInputError("maxCost requires an integer value");
                                return;
                            }
                            if (!RewardType.TryParse(commands[2], out rewardType))
                            {
                                PrintInputError("reward type requires an valid RewardType");
                                return;
                            }
                            if (!int.TryParse(commands[3], out int maxTableSize))
                            {
                                PrintInputError("maxTableSize requires an integer value");
                                return;
                            }
                            configurationString+= "\nMethod: IDA* \nMaximum cost: " + maxCost + "\nReward Type: " + rewardType + "\nlevel: "+level+ "\n********************************\n********************************\n";
                            Log(configurationString);
                            SokobanIDAStarTest(level, maxCost, rewardType, maxTableSize, useNormalizedPosition, useTunnelMacro, useGoalMacro, useGoalCut);
                            break;
                        case "random":
                            if (!int.TryParse(commands[1], out iterations))
                            {
                                PrintInputError("seed requires an unsigned integer value");
                                return;
                            }
                            if (!int.TryParse(commands[2], out int maxDepth))
                            {
                                PrintInputError("seed requires an unsigned integer value");
                                return;
                            }
                            if (!uint.TryParse(commands[3], out seed))
                            {
                                PrintInputError("seed requires an unsigned integer value");
                                return;
                            }
                            configurationString += "\nMethod: random \niterations: " + iterations + "\nMax Iteration Depth: " + maxDepth + "\nseed: " + seed+ "\nlevel: " + level + "\n********************************\n********************************\n";
                            Log(configurationString);
                            SokobanRandom(level,maxDepth,iterations, seed);
                            break;
                    }
                    break;
                case "samegame":
                    switch (commands[0])
                    {
                        case "mcts":
                            if (!int.TryParse(commands[1], out iterations))
                            {
                                PrintInputError("iterations requires an integer value");
                                return;
                            }
                            if (!double.TryParse(commands[2], out const_C))
                            {
                                PrintInputError("const_C requires a double value");
                                return; 
                            }
                            if (!double.TryParse(commands[3], out const_D))
                            {
                                PrintInputError("const_d requires adouble value");
                                return;
                            }
                            if (!int.TryParse(commands[4], out restarts))
                            {
                                PrintInputError("restarts requires an integer value");
                                return;
                            }
                            if (!uint.TryParse(commands[5], out seed))
                            {
                                PrintInputError("seed requires an unsigned integer value");
                                return;
                            }
                            if(!bool.TryParse(commands[6] ,out bool ucb1Tuned))
                            {
                                PrintInputError("seed requires an unsigned integer value");
                                return;
                            }
                            if (!bool.TryParse(commands[7], out bool rave))
                            {
                                PrintInputError("seed requires an unsigned integer value");
                                return;
                            }
                            if (!int.TryParse(commands[8], out raveThreshold))
                            {
                                PrintInputError("Rave threshold requires an integer value");
                                return;
                            }
                            if (!bool.TryParse(commands[9], out bool nodeRecycling))
                            {
                                PrintInputError("seed requires an unsigned integer value");
                                return;
                            }
                            if (!int.TryParse(commands[10], out int memoryBudget))
                            {
                                PrintInputError("restarts requires an integer value");
                                return;
                            }
                            if (!bool.TryParse(commands[11], out useNodeElimination))
                            {
                                PrintInputError("Use Node Elimination requires a boolean value");
                                return;
                            }
                            if (nodeRecycling && (memoryBudget <= 0 || memoryBudget >= iterations)){
                                PrintInputError("Memory budget value not compatible with node recycling");
                                return;
                            }
                            Log("\n********************\nBEGIN TASK:\nGame:" + gameSettings[0] + "\nMethod: MCTS \niterations: " + iterations + "\nUCT constant: " + const_C + "\nSP_UCT constant: " + const_D + "\nrestarts: " + restarts + 
                                "\nSeed: "+seed+"\nUCB1Tuned: "+ucb1Tuned+"\nRAVE: "+rave+"\nRAVE threshold: "+raveThreshold+"\nNode Recycling: "+nodeRecycling + "\nMemory Budget: " + (nodeRecycling ? "" + memoryBudget : "Ignored with node recycling disabled")+"\nNode Elimination: "+useNodeElimination+"\nlevel: " + level + "\n********************");
                            MultiThreadSamegameTest(const_C, const_D, iterations, restarts, level, 1, seed, ucb1Tuned, rave, nodeRecycling, memoryBudget);
                            break;
                        case "ida":
                            if (!int.TryParse(commands[1], out maxCost))
                            {
                                PrintInputError("maxCost requires an integer value");
                                return;
                            }
                            if (!int.TryParse(commands[2], out int tableSize))
                            {
                                PrintInputError("maxTableSize requires an integer value");
                                return;
                            }
                            SamegameIDAStarTest(level, maxCost, tableSize);
                            break;
                    }
                    break;
                default:
                    PrintInputError("Wrong game name: Accepted names are 'sokoban' and 'samegame'");
                    break;

            }
        }

        

        private static void PrintInputError(string errorMessage)
        {
            Console.WriteLine(errorMessage + ".\nArguments list:\n - game\n -const_C\n -const_D\n -iterations per search\n -number of randomized restarts\n -maximum number of threads\n -seed\n -abstractSokoban\n -rewardType\n -log path\n -level path");
            Log(errorMessage);
            if (textWriter != null)
            {
                textWriter.Close();
            }
        }

        private static void SamegameIDAStarTest(string levelPath, int maxCost, int tableSize)//TODO Need a good heuristic for samegame
        {
            string[] levels = ReadSamegameLevels(levelPath);
            IPuzzleState[] states = new IPuzzleState[levels.Length];
            int solvedLevels = 0;
            double totalScore = 0;
            for (int i = 0; i < states.Length; i++)
            {
                states[i] = new SamegameGameState(levels[i], null, null);
                IDAStarSearch idaStar = new IDAStarSearch();
                Log("Level" + (i + 1) + ":\n" + states[i].PrettyPrint());
                List<IPuzzleMove> solution = null;
                string moves = "";
                while (!states[i].isTerminal())
                {
                    solution = idaStar.Solve(states[i], maxCost, tableSize, 100);
                    if(solution.Count > 0)
                    {
                        moves += solution[0];
                        states[i].DoMove(solution[0]);
                    }
                    else
                    {
                        break;
                    }
                }
                if (states[i].EndState())
                {
                    solvedLevels++;
                }
                totalScore += states[i].GetResult();
                Log("Level " + (i + 1) + " solved: " + (states[i].EndState()) + " solution length:" + moves.Count()+ " Score: "+states[i].GetResult());
                Log("Moves: " + moves);
                Log("Solved " + solvedLevels + "/" + (i + 1));
                Console.Write("\rSolved " + solvedLevels + "/" + (i + 1));
                
            }
            Log("Total score: " + totalScore);
        }

        private static void SokobanIDAStarTest(string levelPath, int maxCost, RewardType rewardType, int maxTableSize, bool useNormalizedPosition, bool useTunnelMacro, bool useGoalMacro, bool useGoalCut)
        {
            string[] levels = ReadSokobanLevels(levelPath);
            IPuzzleState[] states = new IPuzzleState[levels.Length];
            int solvedLevels = 0;
            Stopwatch stopwatch = new Stopwatch();
            long stateInitTime;
            long solvingTime;
            //GoalMacroWrapper.BuildMacroTree(null);
            for (int i = 0; i < states.Length; i++)
            {
                stopwatch.Restart();
                states[i] = new AbstractSokobanState(levels[i], rewardType,useNormalizedPosition,useGoalMacro,useTunnelMacro,useGoalCut, null);
                stopwatch.Stop();
                stateInitTime = stopwatch.ElapsedMilliseconds;
                IDAStarSearch idaStar = new IDAStarSearch();
                //Log("Level" + (i + 1) + ":\n" + states[i].PrettyPrint());
                stopwatch.Restart();
                List<IPuzzleMove> solution = idaStar.Solve(states[i],maxCost, maxTableSize, 700);
                stopwatch.Stop();
                solvingTime = stopwatch.ElapsedMilliseconds;
                string moves = "";
                int pushCount = 0;
                
                if (solution != null)
                {
                    foreach (IPuzzleMove m in solution)
                    {
                        //Debug.WriteLine(states[i]);
                        //Debug.WriteLine(m);
                        SokobanPushMove push = (SokobanPushMove)m;
                        foreach (IPuzzleMove basicMove in push.MoveList)
                        {
                            moves += basicMove;
                            if (basicMove.move > 3)//the move is a push move
                            {
                                pushCount++;
                            }
                        }
                        
                        states[i].DoMove(m);
                    }
                    if (states[i].EndState())
                    {
                        solvedLevels++;
                    }
                    Log("Level " + (i + 1) + " solved: " + (states[i].EndState()) + " with "+idaStar.NodeCount+ " nodes; solution length:" + moves.Count() +"/"+pushCount + " - Init Time: " + TimeFormat(stateInitTime) + " - Solving Time: " + TimeFormat(solvingTime));
                    Log("Moves: " + moves);
                    Log("Solved " + solvedLevels + "/" + (i + 1));
                    Console.Write("\rSolved " + solvedLevels + "/" + (i + 1));
                }
            }
        }

        private static int[] MultiThreadSokobanTest(double const_C, double const_D, int iterations, int restarts, string levelPath, uint seed, bool abstractSokoban, RewardType rewardType, bool stopOnResult, double epsilonValue, bool log, int threadNumber)
        {
            string[] levels = ReadSokobanLevels(levelPath);
            int threadCount = Math.Min(Environment.ProcessorCount, threadNumber);
            taskTaken = new int[levels.Length];
            scores = new int[levels.Length];
            solved = new bool[levels.Length];
            SinglePlayerMCTSMain.restarts = restarts;

            Thread[] threads = new Thread[threadCount];
            threadIndex = 0;
            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(() => SokobanTest(const_C, const_D, iterations, restarts, levels, seed, abstractSokoban, rewardType, stopOnResult, epsilonValue, log));
                threads[i].Start();
            }
            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Join();
            }
            if (log)
            {
                //int totalRollouts = 0;
                //int solvedCount = 0;
                //for (int i = 0; i < levels.Length; i++)
                //{
                //    if (solved[i])
                //        solvedCount++;
                //    Log("Level " + (i + 1) + " solved: " + (solved[i]) + " in " + scores[i]);
                //    totalRollouts += scores[i];
                //}
                //Log("Solved " + solvedCount + "/" + levels.Length);
                //Log("Total Rollouts: " + totalRollouts);

            }
            return scores;
        }

        private static void SokobanRandom(string levelPath, int maxDepth, int iterations, uint seed)
        {
            string[] levels = ReadSokobanLevels(levelPath);
            int randomSolved = 0;
            List<IPuzzleMove> moves=null;
            RNG.Seed(seed + threadIndex);
            MersenneTwister rng;


            for (int i = 0; i < levels.Length; i++)
            {
                RNG.Seed(seed + threadIndex);
                rng = new MersenneTwister(seed + threadIndex);
                IPuzzleState state = new AbstractSokobanState(levels[i], RewardType.R0, useNormalizedPosition, useGoalMacro, useTunnelMacro, useGoalCut, null, rng);
                IPuzzleMove move = null;
                int restart = 0;
                IPuzzleState clone = state.Clone();
                string solution;
                while (!state.EndState() && restart < iterations)
                {
                    moves = new List<IPuzzleMove>();
                    state = clone.Clone();
                    int count = 0;
                    
                    while (!state.isTerminal() && count < maxDepth)
                    {
                        move = state.GetRandomMove();
                        moves.Add(move);
                        state.DoMove(move);
                        count++;
                    }

                    restart++;
                }
                if (state.EndState())
                {
                    solution = "";
                    int pushCount = 0;
                    foreach (SokobanPushMove push in moves)
                    {
                        foreach (IPuzzleMove m in push.MoveList)
                        {
                            if (m > 3)
                                pushCount++;
                            solution += m;
                        }
                    }
                    Log("Level " + (i + 1) + "\titerations: " + restart + "\tsolution length:  (moves/pushes)"+ solution.Length+"/"+pushCount +"\tsolution: "+ solution);
                    randomSolved++;
                }
                else
                {
                    Log("Level " + (i + 1) + "\tNo solution found");
                }
            }
            Log("Solved: " + randomSolved + "/" + levels.Length);
        }

        private static int[] SokobanTest(double const_C, double const_D, int iterations, int restarts, string[] levels, uint seed, bool abstractSokoban, RewardType rewardType, bool stopOnResult, double epsilonValue, bool log)
        {
            uint threadIndex = GetThreadIndex();
            RNG.Seed(seed+threadIndex);
            MersenneTwister rng = new MersenneTwister(seed+threadIndex);
            int currentLevelIndex = GetTaskIndex(threadIndex);
            ISPSimulationStrategy simulationStrategy;

            simulationStrategy = new SokobanEGreedyStrategy(epsilonValue, rng);
            IPuzzleState[] states = new IPuzzleState[levels.Length];

            //SokobanMCTSStrategy player;
            
            //Debug.WriteLine("Solved random: "+randomSolved+"/"+levels.Length);
                
            //    Debug.WriteLine(restart);
            

            int solvedLevels = 0;
            int[] rolloutsCount = new int[states.Length];
            Stopwatch stopwatch = new Stopwatch();
            long stateInitializationTime;
            long solvingTime;
            int totalRollouts=0;
            int totalNodes = 0;
            List<int> visitsList = new List<int>();
            List<int> raveVisitsList = new List<int>();
            double totalDepth = 0;
            int totalNodesEliminated = 0;
            int totalNodesNotExpanded = 0;
            for (int i = 0; i < states.Length; i++)
            {
                RNG.Seed(seed + threadIndex);
                rng = new MersenneTwister(seed + threadIndex);
                if (simulationType == SimulationType.EpsilonGreedy)
                {
                    simulationStrategy = new SokobanEGreedyStrategy(epsilonValue, rng);
                }
                else
                {
                    simulationStrategy = new SokobanIDAstarStrategy(maxNodes, tableSize, 200, 0.2);
                }
                if (i%SinglePlayerMCTSMain.threadIndex != threadIndex)
                {
                    continue;
                }
                stopwatch.Restart();
                if (abstractSokoban)
                {
                    states[i] = new AbstractSokobanState(levels[i], rewardType,useNormalizedPosition, useGoalMacro, useTunnelMacro, useGoalCut,simulationStrategy,rng);
                }
                else
                {
                    states[i] = new SokobanGameState(levels[i], rewardType, simulationStrategy);
                }
                stopwatch.Stop();
                stateInitializationTime = stopwatch.ElapsedMilliseconds;
                List<IPuzzleMove> moveList = new List<IPuzzleMove>();
                //player = new SokobanMCTSStrategy(rng, iterations, 600, null, const_C, const_D, stopOnResult);

                //SP_MCTSAlgorithm mcts = new SP_MCTSAlgorithm(new SP_UCTTreeNodeCreator(const_C, const_D, rng), stopOnResult);
                
                OptMCTSAlgorithm mcts = new OptMCTSAlgorithm(new Opt_SP_UCTTreeNodeCreator(const_C, const_D, rng, ucb1Tuned, rave, raveThreshold, nodeRecycling), iterations, memoryBudget, stopOnResult, avoidCycles, useNodeElimination);
                string moves = "";
                stopwatch.Restart();
                moveList = mcts.Solve(states[i], iterations);
                stopwatch.Stop();
                solvingTime = stopwatch.ElapsedMilliseconds;

                //moveList = player.GetSolution(states[i]);
                int pushCount = 0;
                
                foreach (IPuzzleMove m in moveList)
                {
                    if (abstractSokoban)
                    {
                        //Debug.WriteLine("Move: " + m);
                        //Debug.WriteLine(states[i]);
                        SokobanPushMove push = (SokobanPushMove)m;
                        foreach (IPuzzleMove basicMove in push.MoveList)
                        {
                            moves += basicMove;
                            if(basicMove.move > 3)//the move is a push move
                            {
                                pushCount++;
                            }
                        }
                    }
                    else
                    {
                        moves += m;
                        if (m.move > 3)//the move is a push move
                        {
                            pushCount++;
                        }
                    }
                    states[i].DoMove(m);
                }
                if (states[i].EndState())
                {
                    solvedLevels++;
                    totalRollouts += mcts.IterationsForFirstSolution;
                }
                else
                {
                    totalRollouts += mcts.IterationsExecuted;
                }
                totalDepth += mcts.maxDepth;
                totalNodesEliminated += mcts.nodesEliminated;
                totalNodesNotExpanded += mcts.nodesNotExpanded;
                rolloutsCount[i] = mcts.IterationsExecuted;
                scores[i] = rolloutsCount[i];
                solved[i] = states[i].EndState();
                if (log)
                {
                    Log("Level " + (i + 1) + "\titerations: " + mcts.IterationsExecuted + "\titerations for first solution: " + mcts.IterationsForFirstSolution + "\ttotal solutions: " + mcts.SolutionCount + "\tbest solution length (moves/pushes): " + moves.Count() + "/" + pushCount + "\tInit Time: " + TimeFormat(stateInitializationTime) + " - Solving Time: " + TimeFormat(solvingTime) + "\tTree depth: " + mcts.maxDepth +"\tNodes: "+mcts.NodeCount+"\tNodes Eliminated: "+mcts.nodesEliminated+ "\tNodes Not Expanded: "+mcts.nodesNotExpanded +"\tBest solution: " + moves);
                }
                totalNodes += mcts.NodeCount;
                visitsList.AddRange(mcts.visits);
                raveVisitsList.AddRange(mcts.raveVisits);
                Console.Write("\r                              ");
                Console.Write("\rSolved " + solvedLevels + "/" + (i + 1));
            }
            visitsList.Sort((x, y) => (x.CompareTo(y)));
            raveVisitsList.Sort((x, y) => (x.CompareTo(y)));
            double avgVisits = 0;
            foreach(int v in visitsList)
            {
                avgVisits += v;
            }
            double avgRaveVisits = 0;
            foreach (int v in raveVisitsList)
            {
                avgRaveVisits += v;
            }
            avgVisits /= visitsList.Count;
            avgRaveVisits /= raveVisitsList.Count;

            Log("Solved " + solvedLevels + "/" + levels.Length);
            Log("Total iterations: " + totalRollouts);
            Log("Total nodes: " + totalNodes);
            Log("Nodes eliminated: " + totalNodesEliminated);
            Log("Nodes Not Expanded: " + totalNodesNotExpanded);
            Log("avg nodes:" + ((double)totalNodes)/states.Length);
            Log("avg visits: " + avgVisits);
            Log("avg raveVisits: " + avgRaveVisits);
            Log("median visits: " + (visitsList.Count % 2 == 0 ? visitsList[visitsList.Count / 2] : (visitsList[visitsList.Count / 2] + visitsList[1 + visitsList.Count / 2]) / 2));
            Log("median raveVisits: " + (raveVisitsList.Count % 2 == 0 ? raveVisitsList[raveVisitsList.Count / 2] : (raveVisitsList[raveVisitsList.Count / 2] + raveVisitsList[1 + raveVisitsList.Count / 2]) / 2));
            Log("avg depth: " + totalDepth / states.Length);
            return rolloutsCount;
        }

        private static void SokobanTuning(string levelPath, string c_valuesPath, string e_valuesPath, int iterations, int restarts, uint seed, bool abstractSokoban, bool stopOnResult, int maxThread)
        {
            RewardType[] rewards = new RewardType[] { RewardType.R0, RewardType.InverseBM, RewardType.NegativeBM, RewardType.LogBM };
            double[] constantValues = ReadDoubleValues(c_valuesPath);
            double[] epsilonValues = ReadDoubleValues(e_valuesPath);
            RewardType bestReward = RewardType.R0;
            double bestC_value = -1;
            int minTotalRollout = int.MaxValue;
            double bestEpsilon = -1;
            int totalRollouts = 0;
            
            foreach (RewardType reward in rewards)
            {
                foreach(double c_value in constantValues)
                {
                    foreach (double epsilon in epsilonValues)
                    {
                        totalRollouts = 0;
                        List<int> rolloutsCount = new List<int>();
                        Log("Testing Reward: " + reward + " UCT constant: " + c_value + " epsilon: "+epsilon);
                        for (int i = 0; i < restarts; i++)
                        {
                            MultiThreadSokobanTest(c_value, 0, iterations, restarts, levelPath, (uint)(seed+i), abstractSokoban, reward, stopOnResult, epsilon, false, maxThread);
                            for (int j = 0; j < scores.Length; j++)
                            {
                                if (rolloutsCount.Count() <= j)
                                {
                                    rolloutsCount.Add(scores[j]);
                                }
                                else
                                {
                                    rolloutsCount[j] += scores[j];
                                }
                            }
                        }
                        for (int j = 0; j < scores.Length; j++)
                        {
                            rolloutsCount[j] = rolloutsCount[j] / restarts;
                            Log((j + 1) + ": " + rolloutsCount[j]);
                            totalRollouts += rolloutsCount[j];
                        }
                        Log("Total Rollouts: " + totalRollouts);
                        if (totalRollouts < minTotalRollout)
                        {
                            bestReward = reward;
                            bestC_value = c_value;
                            minTotalRollout = totalRollouts;
                        }
                        
                    }
                }
            }
            Log("Best reward :" + bestReward.ToString() + "; Best C value:" + bestC_value+ "; Best epsilon value: " + bestEpsilon);
        }

        private static void ManualSokoban()
        {
            string level = " #####\n #   ####\n #   #  #\n ##    .#\n### ###.#\n# $ # #.#\n# $$# ###\n#@  #\n#####";
            //string level = "####\n# .#\n#  ###\n#*@  #\n#  $ #\n#  ###\n####";
            Log("Level:\n" + level);
            MersenneTwister rng = new MersenneTwister(1+threadIndex);
            ISPSimulationStrategy simulationStrategy = new SokobanRandomStrategy();
            SokobanGameState s = new SokobanGameState(level, RewardType.NegativeBM, simulationStrategy);
            SokobanGameState backupState = (SokobanGameState) s.Clone();
            bool quit = false;
            IPuzzleMove move=null;
            Console.WriteLine(s.PrettyPrint());
            while (!quit)
            {
                ConsoleKeyInfo input = Console.ReadKey();
                List<IPuzzleMove> moves = s.GetMoves();
                switch (input.Key)
                {
                    case ConsoleKey.UpArrow:
                        if(moves.Contains(new SokobanGameMove("u"))){
                            move = new SokobanGameMove("u");
                        }
                        else
                        {
                            move = new SokobanGameMove("U");
                        }
                        break;
                    case ConsoleKey.DownArrow:
                        if (moves.Contains(new SokobanGameMove("d")))
                        {
                            move = new SokobanGameMove("d");
                        }
                        else
                        {
                            move = new SokobanGameMove("D");
                        }
                        break;
                    case ConsoleKey.LeftArrow:
                        if (moves.Contains(new SokobanGameMove("l")))
                        {
                            move = new SokobanGameMove("l");
                        }
                        else
                        {
                            move = new SokobanGameMove("L");
                        }
                        break;
                    case ConsoleKey.RightArrow:
                        if (moves.Contains(new SokobanGameMove("r")))
                        {
                            move = new SokobanGameMove("r");
                        }
                        else
                        {
                            move = new SokobanGameMove("R");
                        }
                        break;
                    case ConsoleKey.R:
                        s = (SokobanGameState) backupState.Clone();
                        move = null;
                        break;
                    case ConsoleKey.Q:
                        move = null;
                        quit = true;
                        break;
                }
                if (move != null)
                {
                    Console.WriteLine("Move: " + move);
                    s.DoMove(move);
                }
                    Console.WriteLine(s.PrettyPrint());
                    Console.WriteLine("Score: " + s.GetScore() + "  |  isTerminal: "+s.isTerminal());
                
                
            }
        }

        private static void MultiThreadSamegameTest(double const_C, double const_D, int iterations, int restarts, string levelPath, int threadNumber, uint seed, bool ucb1Tuned, bool rave, bool nodeRecycling, int memoryBudget)
        {
            string[] levels = ReadSamegameLevels(levelPath);
            taskTaken = new int[levels.Length];
            scores = new int[levels.Length];
            SinglePlayerMCTSMain.restarts = restarts;
            bestMoves = new List<IPuzzleMove>[levels.Length];
            for (int i = 0; i < scores.Length; i++)
            {
                scores[i] = int.MinValue;
            }
            int threadCount = Math.Min(Environment.ProcessorCount, threadNumber);
            Thread[] threads = new Thread[threadCount];
            for (int i = 0; i < threadCount; i++)
            {
                threads[i] = new Thread(() => SamegameTest(const_C, const_D, iterations, restarts, levels, seed, ucb1Tuned,rave,nodeRecycling,memoryBudget));
                threads[i].Start();
            }
            for (int i = 0; i < threadCount; i++)
            {
                threads[i].Join();
            }
            int totalScore = 0;
            Log("*** FINAL RESULT ***");
            for(int i = 0; i < scores.Length; i++)
            {
                totalScore += scores[i];
                Log("Level "+(i+1)+" score: "+scores[i]);
                PrintMoveList(i, bestMoves[i]);
            }
            Log("Total score:" + totalScore);
            textWriter.Close();
        }

        private static void SamegameTest(double const_C, double const_D, int iterations, int restarts, string[] levels, uint seed, bool ucb1Tuned, bool rave, bool nodeRecycling, int memoryBudget)
        {
            uint threadIndex = GetThreadIndex();
            Console.WriteLine("Thread "+ threadIndex +" started");
            MersenneTwister rnd = new MersenneTwister(seed+threadIndex);
            int currentLevelIndex = GetTaskIndex(threadIndex);
            Stopwatch stopwatch = new Stopwatch();
            long stateInitTime;
            long solvingTime;
            int totalRollouts = 0;
            int totalNodes = 0;
            List<int> visitsList = new List<int>();
            List<int> raveVisitsList = new List<int>();
            double totalDepth = 0;
            int totalNodesEliminated = 0;
            int totalNodesNotExpanded = 0;

            while (currentLevelIndex >= 0)
            {
                ISPSimulationStrategy simulationStrategy = new SamegameTabuColorRandomStrategy(levels[currentLevelIndex],rnd);
                //Console.Write("\rRun " + (restartN + 1) + " of " + restarts + "  ");
                stopwatch.Restart();
                SamegameGameState s = new SamegameGameState(levels[currentLevelIndex], rnd, simulationStrategy);
                stopwatch.Stop();
                stateInitTime = stopwatch.ElapsedMilliseconds;
                IPuzzleMove move;
                SamegameMCTSStrategy player = new SamegameMCTSStrategy(rnd,ucb1Tuned, rave, raveThreshold, nodeRecycling, memoryBudget, useNodeElimination, iterations, null, const_C, const_D);
                string moveString = string.Empty;
                List<IPuzzleMove> moveList = new List<IPuzzleMove>();
                stopwatch.Restart();
                while (!s.isTerminal())
                {
                    move = player.selectMove(s);
                    moveList.Add(move);
                    s.DoMove(move);
                    totalRollouts += player.Mcts.IterationsExecuted;
                    totalNodes += player.Mcts.NodeCount;
                    totalNodesEliminated += player.Mcts.nodesEliminated;
                    totalNodesNotExpanded += player.Mcts.nodesNotExpanded;
                    visitsList.AddRange(player.Mcts.visits);
                    raveVisitsList.AddRange(player.Mcts.raveVisits);
                    totalDepth += player.Mcts.maxDepth;
                }
                stopwatch.Stop();

                solvingTime = stopwatch.ElapsedMilliseconds;
                lock (taskLock)
                {
                    if (s.GetScore() > scores[currentLevelIndex])
                    {
                        scores[currentLevelIndex] = s.GetScore();
                        bestMoves[currentLevelIndex] = moveList;
                        Log("Completed run " + taskTaken[currentLevelIndex] + "/" + restarts + " of level " + (currentLevelIndex + 1) + ". New top score found: " + scores[currentLevelIndex] + " - Init Time: " + TimeFormat(stateInitTime) + " - Solving Time: " + TimeFormat(solvingTime));
                        //PrintMoveList(currentLevelIndex, moveList);
                        //PrintBestScore();
                    }
                    else
                    {
                        Log("Completed run " + taskTaken[currentLevelIndex] + "/" + restarts + " of level " + (currentLevelIndex + 1) + " with score: " + s.GetScore() + " - Init Time: " + TimeFormat(stateInitTime) + " - Solving Time: " + TimeFormat(solvingTime));
                    }
                }
                currentLevelIndex = GetTaskIndex(threadIndex);
            }
            visitsList.Sort((x, y) => (x.CompareTo(y)));
            raveVisitsList.Sort((x, y) => (x.CompareTo(y)));
            double avgVisits = 0;
            foreach (int v in visitsList)
            {
                avgVisits += v;
            }
            double avgRaveVisits = 0;
            foreach (int v in raveVisitsList)
            {
                avgRaveVisits += v;
            }
            avgVisits /= visitsList.Count;
            avgRaveVisits /= raveVisitsList.Count;

            //Log("Solved " + solvedLevels + "/" + levels.Length);
            Log("Total iterations: " + totalRollouts);
            Log("Total nodes: " + totalNodes);
            Log("Nodes eliminated: " + totalNodesEliminated);
            Log("Nodes Not Expanded: " + totalNodesNotExpanded);
            Log("avg nodes:" + ((double)totalNodes) / levels.Length);
            Log("avg visits: " + avgVisits);
            Log("avg raveVisits: " + avgRaveVisits);
            Log("median visits: " + (visitsList.Count % 2 == 0 ? visitsList[visitsList.Count / 2] : (visitsList[visitsList.Count / 2] + visitsList[1 + visitsList.Count / 2]) / 2));
            Log("median raveVisits: " + (raveVisitsList.Count % 2 == 0 ? raveVisitsList[raveVisitsList.Count / 2] : (raveVisitsList[raveVisitsList.Count / 2] + raveVisitsList[1 + raveVisitsList.Count / 2]) / 2));
            Log("avg depth: " + totalDepth / levels.Length);
        }

        private static string[] ReadSamegameLevels(string levelPath)
        {
            StreamReader reader = File.OpenText(levelPath);
            string fullString = reader.ReadToEnd();
            reader.Close();
            string[] levels = fullString.Split(new string[] { "#" }, StringSplitOptions.RemoveEmptyEntries);
            return levels;
        }

        private static string[] ReadSokobanLevels(string levelPath)
        {
            StreamReader reader = File.OpenText(levelPath);
            string fullString = reader.ReadToEnd();
            fullString = Regex.Replace(fullString, @"[\d]", string.Empty);
            reader.Close();
            string[] levels = fullString.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
            return levels;
        }

        private static double[] ReadDoubleValues(string filePath)
        {
            StreamReader reader = File.OpenText(filePath);
            string fullString = reader.ReadToEnd();
            reader.Close();
            string[] stringValues = fullString.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
            double[] values = Array.ConvertAll(stringValues, double.Parse);
            return values;
        }
        
        private static int GetTaskIndex( uint threadIndex)
        {
            lock (taskLock)
            {
                for (int i = 0; i < taskTaken.Length; i++)
                {
                    if (taskTaken[i] < restarts && i%SinglePlayerMCTSMain.threadIndex == threadIndex)
                    {
                        taskTaken[i]++;
                        return i;
                    }
                    if (taskTaken[i] == restarts )
                    {
                        taskTaken[i]++;
                        //Console.WriteLine("Level " + (i+1) + " completed");
                    }
                }
                return -1;
            }
        }

        private static uint GetThreadIndex()
        {
            lock (taskLock)
            {
                return threadIndex++;
            }
        }

        public static void PrintMoveList(int level, List<IPuzzleMove> moves)
        {
            for(int i = 0; i < moves.Count; i++)
            {
                Log("Level " + (level + 1) + " - move " + i + ": " + moves[i]);
            }
        }

        public static void PrintBestScore()
        {
            int partialScore = 0;
            int scoresCount = 0;
            for (int i = 0; i < scores.Length; i++)
            {
                if (scores[i] > int.MinValue)
                {
                    partialScore += scores[i];
                    scoresCount++;
                }
            }
            Log("Partial score : " + partialScore + " on " + scoresCount + " levels");
        }

        public static void Log(string logMessage, bool autoFlush = true)
        {
            lock (taskLock)
            {
                textWriter.WriteLine("{0} - {1}  :  {2}", DateTime.Now.ToShortDateString(), DateTime.Now.ToLongTimeString(), logMessage);
                Console.WriteLine(logMessage);
                if (autoFlush)
                {
                    textWriter.Flush();
                }
            }
        }

        public static string TimeFormat(long timeInMilliseconds)
        {
            TimeSpan timeSpan = TimeSpan.FromMilliseconds(timeInMilliseconds);
            return String.Format("{0:00}h:{1:00}m:{2:00}s.{3:000}ms", timeSpan.Hours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }
    }
}
