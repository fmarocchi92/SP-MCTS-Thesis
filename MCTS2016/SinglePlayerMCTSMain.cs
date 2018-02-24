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
        /// <summary>
        /// - game
        /// - constC
        /// - constD
        /// - iterations per search
        /// - number of randomized restarts
        /// - maximum number of threads
        /// - seed
        /// - log path
        /// - level path
        /// </summary>
        /// <param name="args">
        /// </param>
        public static void Main(string[] args)
        {
            if (args.Length < 9)
            {
                PrintInputError("Missing arguments");
                return;
            }
            string game = args[0];
            double const_C;
            if (!double.TryParse(args[1], out const_C))
            {
                PrintInputError("Const_C requires a double value");
                return;
            }
            double const_D;
            if (!double.TryParse(args[2], out const_D))
            {
                PrintInputError("Const_D requires a double value");
                return;
            }
            int iterations;
            if (!int.TryParse(args[3], out iterations))
            {
                PrintInputError("iterations requires an integer value");
                return;
            }
            int restarts;
            if (!int.TryParse(args[4], out restarts))
            {
                PrintInputError("number of restarts requires an integer value");
                return;
            }
            int maxThread;
            if (!int.TryParse(args[5], out maxThread))
            {
                PrintInputError("maximum number of threads requires an integer value");
                return;
            }
            uint seed;
            if (!uint.TryParse(args[6], out seed))
            {
                PrintInputError("seed requires an unsigned integer value");
                return;
            }

            bool abstractSokoban;
            if (!bool.TryParse(args[7], out abstractSokoban))
            {
                PrintInputError("abstract sokoban requires a boolean value");
                return;
            }
            RewardType rewardType;
            if (!RewardType.TryParse(args[8], out rewardType))
            {
                PrintInputError("reward type requires an integer value");
                return;
            }
            bool stopOnResult;
            if (!bool.TryParse(args[9], out stopOnResult))
            {
                PrintInputError("stop on result requires a boolean value");
                return;
            }
            //RNG.Seed(seed);
            string logPath = args[10];
            textWriter = File.AppendText(logPath);
            string levelPath = args[11];
            Log("\n");
            Log("BEGIN TASK: " + game + " - const_C: " + const_C + " - const_D: " + const_D + " - iterations per move: " + iterations + " - restarts: " + restarts + " - max threads: "+maxThread+" - abstract: "+abstractSokoban);

            switch (game)
            {
                case "sokoban":
                    MultiThreadSokobanTest(const_C, const_D, iterations, restarts, levelPath, seed, abstractSokoban, rewardType, stopOnResult, 0.1, true, maxThread);
                    break;
                case "samegame":
                    MultiThreadSamegameTest(const_C, const_D, iterations, restarts, levelPath, maxThread, seed);
                    break;
                case "idastar":
                    IDAStarTest(levelPath, iterations);
                    break;
                case "sokobanTuning":
                    string c_valuesPath = args[12];
                    string e_valuesPath = args[13];
                    SokobanTuning(levelPath,c_valuesPath, e_valuesPath, iterations, restarts, seed, abstractSokoban, stopOnResult, maxThread);
                    break;
                default:
                    PrintInputError("Invalid game value");
                    break;
            }
            if (game.Equals("samegameidastar"))
            {
                SamegameIDAStarTest(levelPath, iterations);
            }
            //textWriter.Close();
        }

        

        private static void PrintInputError(string errorMessage)
        {
            Console.WriteLine(errorMessage + ".\nArguments list:\n - game\n -const_C\n -const_D\n -iterations per search\n -number of randomized restarts\n -maximum number of threads\n -seed\n -abstractSokoban\n -rewardType\n -log path\n -level path");
            if (textWriter != null)
            {
                textWriter.Close();
            }
        }

        private static void SamegameIDAStarTest(string levelPath, int maxCost)//TODO Need a good heuristic for samegame
        {
            string[] levels = ReadSamegameLevels(levelPath);
            IPuzzleState[] states = new IPuzzleState[levels.Length];
            int solvedLevels = 0;
            for (int i = 0; i < states.Length; i++)
            {
                states[i] = new SamegameGameState(levels[i], null, null);
                IDAStarSearch idaStar = new IDAStarSearch(states[i]);
                Log("Level" + (i + 1) + ":\n" + states[i].PrettyPrint());
                List<IPuzzleMove> solution = idaStar.Solve(maxCost);
                string moves = "";
                if (solution != null)
                {
                    foreach (IPuzzleMove m in solution)
                    {
                        moves += m;
                        states[i].DoMove(m);
                    }
                    if (states[i].EndState())
                    {
                        solvedLevels++;
                    }
                    Log("Level " + (i + 1) + " solved: " + (states[i].EndState()) + " solution length:" + moves.Count());
                    Log("Moves: " + moves);
                    Log("Solved " + solvedLevels + "/" + (i + 1));
                    Console.Write("\rSolved " + solvedLevels + "/" + (i + 1));
                }
            }
        }

        private static void IDAStarTest(string levelPath, int maxCost)
        {
            string[] levels = ReadSokobanLevels(levelPath);
            IPuzzleState[] states = new IPuzzleState[levels.Length];
            int solvedLevels = 0;
            for(int i = 0; i < states.Length; i++)
            {
                states[i] = new AbstractSokobanState(levels[i], RewardType.PositiveBM, false, null);
                IDAStarSearch idaStar = new IDAStarSearch(states[i]);
                Log("Level" + (i + 1) + ":\n" + states[i].PrettyPrint());
                List<IPuzzleMove> solution = idaStar.Solve(maxCost);
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
                    Log("Level " + (i + 1) + " solved: " + (states[i].EndState()) + " solution length:" + moves.Count() +"/"+pushCount);
                    Log("Moves: " + moves);
                    Log("Solved " + solvedLevels + "/" + (i + 1));
                    Console.Write("\rSolved " + solvedLevels + "/" + (i + 1));
                }
            }
            
            
        }

        private static int[] MultiThreadSokobanTest(double const_C, double const_D, int iterations, int restarts, string levelPath, uint seed, bool abstractSokoban, RewardType rewardType, bool stopOnResult, double epsilonValue, bool log = true, int threadNumber = 8)
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
                int solvedCount = 0;
                for (int i = 0; i < levels.Length; i++)
                {
                    if (solved[i])
                        solvedCount++;
                    Log("Level " + (i + 1) + " solved: " + (solved[i]) + " in " + scores[i]);
                }
                Log("Solved " + solvedCount + "/" + levels.Length);
            }
            return scores;
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
            int solvedLevels = 0;
            int[] rolloutsCount = new int[states.Length];
            for (int i = 0; i < states.Length; i++)
            {
                if(i%SinglePlayerMCTSMain.threadIndex != threadIndex)
                {
                    continue;
                }
                if (abstractSokoban)
                {
                    states[i] = new AbstractSokobanState(levels[i], rewardType,false, simulationStrategy,rng);
                }
                else
                {
                    states[i] = new SokobanGameState(levels[i], rewardType, simulationStrategy);
                }
                List<IPuzzleMove> moveList = new List<IPuzzleMove>();
                //player = new SokobanMCTSStrategy(rng, iterations, 600, null, const_C, const_D, stopOnResult);
                SP_MCTSAlgorithm mcts = new SP_MCTSAlgorithm(new SP_UCTTreeNodeCreator(const_C, const_D, rng), stopOnResult);                

                string moves = "";
                moveList = mcts.Solve(states[i], iterations);
                //moveList = player.GetSolution(states[i]);
                int pushCount = 0;
                foreach (IPuzzleMove m in moveList)
                {
                    if (abstractSokoban)
                    {
                        Debug.WriteLine("Move: " + m);
                        Debug.WriteLine(states[i]);
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
                }
                rolloutsCount[i] = mcts.IterationsExecuted;
                scores[i] = rolloutsCount[i];
                solved[i] = states[i].EndState();
                if (log)
                {
                    Log("Level " + (i + 1) + " solved: " + (states[i].EndState()) + " in " + mcts.IterationsExecuted + " rollouts - solution length (moves/pushes): " + moves.Count() + "/" + pushCount);
                    Log("Moves: " + moves);
                }
                Console.Write("\r                              ");
                Console.Write("\rSolved " + solvedLevels + "/" + (i + 1));
            }
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

        private static void MultiThreadSamegameTest(double const_C, double const_D, int iterations, int restarts, string levelPath, int threadNumber, uint seed)
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
                threads[i] = new Thread(() => SamegameTest(const_C, const_D, iterations, restarts, levels, seed));
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

        private static void SamegameTest(double const_C, double const_D, int iterations, int restarts, string[] levels, uint seed)
        {
            uint threadIndex = GetThreadIndex();
            Console.WriteLine("Thread "+ threadIndex +" started");
            MersenneTwister rnd = new MersenneTwister(seed+threadIndex);
            int currentLevelIndex = GetTaskIndex(threadIndex);
            while (currentLevelIndex >= 0)
            {
                ISPSimulationStrategy simulationStrategy = new SamegameTabuColorRandomStrategy(levels[currentLevelIndex],rnd);
                //Console.Write("\rRun " + (restartN + 1) + " of " + restarts + "  ");
                SamegameGameState s = new SamegameGameState(levels[currentLevelIndex], rnd, simulationStrategy);
                IPuzzleMove move;
                ISPSimulationStrategy player = new SamegameMCTSStrategy(rnd,iterations, 600, null, const_C, const_D);
                string moveString = string.Empty;
                List<IPuzzleMove> moveList = new List<IPuzzleMove>();
                while (!s.isTerminal())
                {
                    move = player.selectMove(s);
                    moveList.Add(move);
                    s.DoMove(move);
                }
                lock (taskLock)
                {
                    if (s.GetScore() > scores[currentLevelIndex])
                    {
                        scores[currentLevelIndex] = s.GetScore();
                        bestMoves[currentLevelIndex] = moveList;
                        Log("Completed run " + taskTaken[currentLevelIndex] + "/" + restarts + " of level " + (currentLevelIndex + 1) + ". New top score found: " + scores[currentLevelIndex]);
                        PrintMoveList(currentLevelIndex, moveList);
                        PrintBestScore();
                    }
                    else
                    {
                        Log("Completed run " + taskTaken[currentLevelIndex] + "/" + restarts + " of level " + (currentLevelIndex + 1) + " with score: " + s.GetScore());
                    }
                }
                currentLevelIndex = GetTaskIndex(threadIndex);
            }
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
                if (autoFlush)
                {
                    textWriter.Flush();
                }
            }
        }
    }
}
