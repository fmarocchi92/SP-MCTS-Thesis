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


        public static void Main(string[] args)
        {
            string game;
            int iterations;
            int maxCost;
            RewardType rewardType;
            double const_C;
            double epsilon;
            double const_D;
            int restarts;
            uint seed;
            bool stopOnResult;
            string level;

            if (args.Length != 3)
            {
                throw new ArgumentException("Need three arguments: game method level");
            }

            textWriter = File.AppendText("Log.txt");

            game = args[0];

            string[] commands = args[1].Split(':');

            level = args[2];

            switch (game) {
                case "sokoban":
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
                            if (!double.TryParse(commands[5], out epsilon))
                            {
                                PrintInputError("epsilon requires a double value");
                                return;
                            }
                            if(!bool.TryParse(commands[6], out stopOnResult))
                            {
                                PrintInputError("stop on result requires a boolean value");
                                return;
                            }
                            if(!uint.TryParse(commands[7], out seed))
                            {
                                PrintInputError("seed requires an unsigned integer value");
                                return;
                            }
                            Log("\n********************\nBEGIN TASK:\nGame:" + game + "\nMethod: MCTS \niterations: " + iterations + "\nUCT constant: " + const_C+ "\nSP_UCT constant: "+ const_D +  "\nReward Type: "+rewardType+"\nepsilon: "+epsilon+ "level: "+ level +"\n********************");
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
                            Log("\n********************\nBEGIN TASK:\nGame:" + game + "\nMethod: IDA* \nMaximum cost: " + maxCost + "\nReward Type: " + rewardType + "\nlevel: "+level+"\n********************");
                            IDAStarTest(level, maxCost, rewardType, maxTableSize);
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
                            if (!bool.TryParse(commands[8], out bool nodeRecycling))
                            {
                                PrintInputError("seed requires an unsigned integer value");
                                return;
                            }
                            if (!int.TryParse(commands[9], out int memoryBudget))
                            {
                                PrintInputError("restarts requires an integer value");
                                return;
                            }
                            if(nodeRecycling && (memoryBudget <= 0 || memoryBudget >= iterations)){
                                PrintInputError("Memory budget value not compatible with node recycling");
                                return;
                            }
                            Log("\n********************\nBEGIN TASK:\nGame:" + game + "\nMethod: MCTS \niterations: " + iterations + "\nUCT constant: " + const_C + "\nSP_UCT constant: " + const_D + "\nrestarts: " + restarts + "level: " + level + "\n********************");
                            MultiThreadSamegameTest(const_C, const_D, iterations, restarts, level, 1, seed, ucb1Tuned, rave, nodeRecycling, memoryBudget);
                            break;
                        case "ida":
                            throw new NotImplementedException("IDA* for samegame not implemented");
                            break;
                    }
                    break;

            }
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
                IDAStarSearch idaStar = new IDAStarSearch();
                Log("Level" + (i + 1) + ":\n" + states[i].PrettyPrint());
                List<IPuzzleMove> solution = idaStar.Solve(states[i],maxCost,200000,100);
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

        private static void IDAStarTest(string levelPath, int maxCost, RewardType rewardType, int maxTableSize)
        {
            string[] levels = ReadSokobanLevels(levelPath);
            IPuzzleState[] states = new IPuzzleState[levels.Length];
            int solvedLevels = 0;
            //GoalMacroWrapper.BuildMacroTree(null);
            for (int i = 0; i < states.Length; i++)
            {
                states[i] = new AbstractSokobanState(levels[i], rewardType,true,true,true,true, null);
                IDAStarSearch idaStar = new IDAStarSearch();
                Log("Level" + (i + 1) + ":\n" + states[i].PrettyPrint());
                List<IPuzzleMove> solution = idaStar.Solve(states[i],maxCost, maxTableSize, 700);
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
                    states[i] = new AbstractSokobanState(levels[i], rewardType,false, true,true,true,simulationStrategy,rng);
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
            while (currentLevelIndex >= 0)
            {
                ISPSimulationStrategy simulationStrategy = new SamegameTabuColorRandomStrategy(levels[currentLevelIndex],rnd);
                //Console.Write("\rRun " + (restartN + 1) + " of " + restarts + "  ");
                SamegameGameState s = new SamegameGameState(levels[currentLevelIndex], rnd, simulationStrategy);
                IPuzzleMove move;
                ISPSimulationStrategy player = new SamegameMCTSStrategy(rnd,ucb1Tuned, rave, nodeRecycling, memoryBudget, iterations, null, const_C, const_D);
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
