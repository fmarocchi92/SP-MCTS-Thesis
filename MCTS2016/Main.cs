//using System;
//using Common.Abstract;
//using System.Xml;
//using System.IO;
//using System.Collections.Generic;
//using System.Text.RegularExpressions;
//using CommandLine;
//using CommandLine.Text;
//
//namespace BoardGames
//{
//    //    class Options
//    //    {
//    //        [Option('g', "game", DefaultValue = true, HelpText = "Wites out the result of every single game")]
//    //        public bool OutGameLog { get; set; }
//    //        //        [Option('r', "read", Required = true,
//    //        //            HelpText = "Input file to be processed.")]
//    //        //        public string InputFile { get; set; }
//    //        //
//    //        //        [Option('v', "verbose", DefaultValue = true,
//    //        //            HelpText = "Prints all messages to standard output.")]
//    //        //        public bool Verbose { get; set; }
//    //        //
//    //        //        [ParserState]
//    //        //        public IParserState LastParserState { get; set; }
//    //        //
//    //        [HelpOption]
//    //        public string GetUsage()
//    //        {
//    //            return HelpText.AutoBuild(this,
//    //                (HelpText current) => HelpText.DefaultParsingErrorsHandler(this, current));
//    //        }
//    //    }
//
//    class TestConfiguration
//    {
//        public Tuple<string, Dictionary<string, object>>[] algorithmsParameters = new Tuple<string, Dictionary<string, object>>[2];
//        public string gameName;
//        public float precision;
//        public int numberOfGamesToRun;
//        public int numberOfRuns;
//        public int boardSize;
//
//        public TestConfiguration(string gameName, float precision, int numberOfGamesToRun, int numberOfRuns, Tuple<string, Dictionary<string, object>>[] algorithmsParameters, int boardSize)
//        {
//            this.gameName = gameName;
//            this.precision = precision;
//            this.numberOfGamesToRun = numberOfGamesToRun;
//            this.numberOfRuns = numberOfRuns;
//            this.algorithmsParameters = algorithmsParameters;
//            this.boardSize = boardSize;
//        }
//    }
//
//    public class MainClass
//    {
//        private const string MCTS_ALG_NAME = "mcts";
//        private const string RANDOM_ALG_NAME = "random";
//        private const string LOOKUP_ALG_NAME = "lookup";
//        private const string SCHMIDT_ALG_NAME = "schmidt";
//        private const string NFRIENDSFLOYD_ALG_NAME = "nfriendsfloyd";
//        private const string MINIMAX_ALG_NAME = "minimax";
//        private const string MINIMAXVARIANTS_ALG_NAME = "minimaxvariants";
//        private const string WINOTHELLO_ALG_NAME = "winothello";
//        private const string SIMPLEWITHHEURISTIC_ALG_NAME = "simplewithheuristic";
//        private const string DTHOAI_ALG_NAME = "dthoai";
//        private const string NOVICE_ALG_NAME = "novice";
//        private const string QUARTOLIB_ALG_NAME = "quartolib";
//
//        public const string TICTACTOE_GAME_NAME = "tictactoe";
//        public const string FORZA4_GAME_NAME = "forza4";
//        public const string OTHELLO_GAME_NAME = "othello";
//        public const string GOMOKU_GAME_NAME = "gomoku";
//        public const string QUARTO_GAME_NAME = "quarto";
//
//        private const string USEHEURISTIC_PROP_NAME = "useheuristic";
//        private const string DEPTH_PROP_NAME = "depth";
//        private const string VARIATION_PROP_NAME = "variation";
//        private const string SPEEDUP_PROP_NAME = "speedup";
//        private const string NUMBEROFBACKUPS_PROP_NAME = "numberofbackups";
//        private const string MINIMIZATIONBREAKSTRATEGY_PROP_NAME = "minimizationbreakstrategy";
//        private const string STARTINGNUMBEROFBACKUPS_PROP_NAME = "startingnumberofbackups";
//        private const string SEARCHALGORITHM_PROP_NAME = "searchalgorithm";
//
//        private static List<string> supportedGames = new List<string>{ TICTACTOE_GAME_NAME, FORZA4_GAME_NAME, OTHELLO_GAME_NAME, GOMOKU_GAME_NAME, QUARTO_GAME_NAME };
//
//        public static void Main(string[] args)
//        {
////            var options = new Options();
////            if (CommandLine.Parser.Default.ParseArguments(args, options))
////            {
////                // Values are available here
////                var s = options.OutGameLog;
////
////            }
//
//            if (args.Length != 2)
//            {
//                #if DEBUG
//                Console.WriteLine("Missing args from stdin trying to find local cfg in {0}.", Directory.GetCurrentDirectory());
//                #endif
//
//                foreach (FileInfo fileInfo in (new DirectoryInfo(Directory.GetCurrentDirectory())).GetFiles())
//                {
//                    if (fileInfo.Extension.Equals(".cfg"))
//                    {
//                        var fileName = fileInfo.Name;
//
//                        Regex rgx = new Regex(@"_(.*)\.cfg", RegexOptions.IgnoreCase);
//                        if (rgx.IsMatch(fileName))
//                        {
//                            var gameName = rgx.Match(fileName).Groups[1].ToString();
//                            args = new string[2]{ gameName, fileInfo.FullName };
//                            break;
//                        }
//                    }
//                }
//            }
//
//            if (args.Length == 2)
//            {
//                var gameName = args[0];
//                var fullPathToConfigFile = args[1];
//                if (File.Exists(fullPathToConfigFile))
//                {
//
//                    if (!string.IsNullOrEmpty(gameName) && supportedGames.Contains(gameName.ToLower()))
//                    {
//                        var config = LoadTestConfiguration(gameName, fullPathToConfigFile);
//
//                        string logFilePath = String.Format("{0}/{1}/{2}_{3}Vs{4}_{5:dd-MM-yyyy_hh-mm-ss-tt}/log.txt", Directory.GetCurrentDirectory(), "TestResults", gameName, config.algorithmsParameters[0].Item1, config.algorithmsParameters[1].Item1, DateTime.Now);
//                        (new FileInfo(logFilePath)).Directory.Create();
//
//                        #if DEBUG
//                        Console.WriteLine("{0} directory created to store the info about the running games. When a move is performed it will be logged in log.txt.", new DirectoryInfo(logFilePath).Parent);
//                        #endif
//
//                        File.Copy(fullPathToConfigFile, string.Format("{0}/test.cfg", Path.GetDirectoryName(logFilePath)), true);
//
//                        Logger logger = new Logger(logFilePath);
//
//                        if (config != null)
//                        {
//                            try
//                            {
//                                RunTest(config, logger);
//                            }
//                            catch (Exception e)
//                            {
//                                logger.LogErrorMessage(e.Message);
//                                logger.LogErrorMessage(e.StackTrace);
//                                Console.Error.WriteLine("Something happend while running the test. Error: {0}", e);
//                                throw e;
//                            }
//                        }
//                        else
//                        {
//                            return;
//                        }
//                    }
//                    else
//                    {
//                        Console.Error.WriteLine("{0} is not a supported game.", gameName);
//                    }
//                }
//                else
//                {
//                    Console.Error.WriteLine("{0} does not exist.", fullPathToConfigFile);
//                }
//            }
//            else
//            {
//                Console.Error.WriteLine("Missing parameters from stdin. Needed [gameName] [fullPathToConfigFile].");
//            }
//        }
//
//        private static TestConfiguration LoadTestConfiguration(string gameName, string uriToConfigFile)
//        {
//
//            if (!File.Exists(uriToConfigFile))
//            {
//                Console.Error.WriteLine("Config file {0} does not exist.", uriToConfigFile);
//                return null;
//            }
//
//            var doc = new XmlDocument();
//
//            Tuple<string, Dictionary<string, object>>[] algorithmsParameters = new Tuple<string, Dictionary<string, object>>[2];
//
//            //< <algorithmName1, { {propertyName1, propertyValue1}, {propertyName2, propertyValue2}}>
//            // <algorithmName2, { {propertyName1, propertyValue1}, {propertyName2, propertyValue2}}> >
//
//            int numberOfGamesToRun = 100;
//            float precision = 0.1f;
//            int numberOfRuns = 10;
//            int boardSize = -1;
//
//            using (FileStream fs = File.Open(uriToConfigFile, FileMode.Open, FileAccess.Read, FileShare.Write))
//            {
//                doc.Load(fs);
//
//                #if DEBUG
//                Console.WriteLine("Configuration file at {0} loaded", uriToConfigFile);
//                #endif
//
//                var algorithmNode = doc.SelectNodes("test/algorithm");
//
//                if (algorithmNode != null)
//                {
//                    if (algorithmNode.Count == 2)
//                    {
//                        #if DEBUG
//                        Console.WriteLine("Reading <algorithm> nodes");
//                        #endif
//
//                        algorithmsParameters[0] = new Tuple<string, Dictionary<string, object>>(algorithmNode[0].InnerText.ToLower(), new Dictionary<string, object>());
//                        algorithmsParameters[1] = new Tuple<string, Dictionary<string, object>>(algorithmNode[1].InnerText.ToLower(), new Dictionary<string, object>());
//
//                        #if DEBUG
//                        Console.WriteLine("{0} and {1} found. Now reading algorithm parameters.", algorithmsParameters[0].Item1, algorithmsParameters[1].Item1);
//                        #endif
//
//                        for (int algorithmNodeIndex = 0; algorithmNodeIndex < algorithmNode.Count; algorithmNodeIndex++)
//                        {
//                            if (algorithmNode[algorithmNodeIndex].Attributes != null && algorithmNode[algorithmNodeIndex].Attributes.Count > 0)
//                            {
//                                for (int attributeIndex = 0; attributeIndex < algorithmNode[algorithmNodeIndex].Attributes.Count; attributeIndex++)
//                                {
//                                    var attrName = algorithmNode[algorithmNodeIndex].Attributes[attributeIndex].Name.ToLower();
//                                    var attrVal = algorithmNode[algorithmNodeIndex].Attributes[attributeIndex].Value;
//
//                                    algorithmsParameters[algorithmNodeIndex].Item2.Add(attrName, attrVal);
//
//                                    #if DEBUG
//                                    Console.WriteLine("{0} for {1} added", attrName, algorithmsParameters[algorithmNodeIndex].Item1);
//                                    #endif
//                                }
//                            }
//                        }
//                    }
//                }
//                else
//                {
//                    Console.Error.WriteLine("Algorithm node missing from {0} file", uriToConfigFile);
//                }
//
//                var numberOfGamesToRunNode = doc.SelectNodes("test/numberOfGamesToRun");
//
//                if (numberOfGamesToRunNode != null && numberOfGamesToRunNode.Count > 0)
//                {
//                    int.TryParse(numberOfGamesToRunNode[0].InnerText, out numberOfGamesToRun);
//                    #if DEBUG
//                    Console.WriteLine("<numberOfGamesToRun> node with value {0} found", numberOfGamesToRun);
//                    #endif
//                }
//
//                var precisionNode = doc.SelectNodes("test/precision");
//
//                if (precisionNode != null && precisionNode.Count > 0)
//                {
//                    float.TryParse(precisionNode[0].InnerText, out precision);
//                    #if DEBUG
//                    Console.WriteLine("<precision> node with value {0} found. Note this parameter is used only when one of the algorithms used is MCTS and the minimization process is used.", precision);
//                    #endif
//                }
//
//                var numberOfRunsNode = doc.SelectNodes("test/numberOfRuns");
//
//                if (numberOfRunsNode != null && numberOfRunsNode.Count > 0)
//                {
//                    int.TryParse(numberOfRunsNode[0].InnerText, out numberOfRuns);
//                    #if DEBUG
//                    Console.WriteLine("<numberOfRuns> node with value {0} found. Note this parameter is used only when one of the algorithms used is MCTS and the minimization process is used.", numberOfRuns);
//                    #endif
//                }
//
//                var boardSizeNode = doc.SelectNodes("test/boardSize");
//
//                if (boardSizeNode != null && boardSizeNode.Count > 0)
//                {
//                    int.TryParse(boardSizeNode[0].InnerText, out boardSize);
//                    #if DEBUG
//                    Console.WriteLine("<boardSize> node with value {0} found. Note this parameter works only with a given combination of board games and algorithms. For more info contact developer.", boardSize);
//                    #endif
//                }
//            }
//
//            TestConfiguration testConfig = new TestConfiguration(gameName, precision, numberOfGamesToRun, numberOfRuns, algorithmsParameters, boardSize);
//
//            #if DEBUG
//            Console.WriteLine("Success. Configuration file loaded correctly!");
//            #endif
//
//            return testConfig;
//        }
//
//        private static int GetIntAlgorithmParameter(TestConfiguration config, int algIndex, string paramName, int? def = null)
//        {
//            if (config.algorithmsParameters[algIndex].Item2.ContainsKey(paramName))
//            {
//                return int.Parse((string)config.algorithmsParameters[algIndex].Item2[paramName]);
//            }
//
//            if (def != null)
//            {
//                return def.Value;
//            }
//
//            throw new ArgumentException(string.Format("Not in algorithm {0} parameters list", algIndex), paramName);
//        }
//
//        private static bool GetBoolAlgorithmParameter(TestConfiguration config, int algIndex, string paramName, bool? def = null)
//        {
//            if (config.algorithmsParameters[algIndex].Item2.ContainsKey(paramName))
//            {
//                return bool.Parse((string)config.algorithmsParameters[algIndex].Item2[paramName]);
//            }
//
//            if (def != null)
//            {
//                return def.Value;
//            }
//
//            throw new ArgumentException(string.Format("Not in algorithm {0} parameters list", algIndex), paramName);
//        }
//
//        private static string GetStringAlgorithmParameter(TestConfiguration config, int algIndex, string paramName, string def = null)
//        {
//            if (config.algorithmsParameters[algIndex].Item2.ContainsKey(paramName))
//            {
//                return (string)config.algorithmsParameters[algIndex].Item2[paramName];
//            }
//
//            if (def != null)
//            {
//                return def;
//            }
//
//            throw new ArgumentException(string.Format("Not in algorithm {0} parameters list", algIndex), paramName);
//        }
//
//        private static void RunTest(TestConfiguration config, Logger logger)
//        {
//            #if DEBUG
//            Console.WriteLine("Running the test. Creating algorithm objects first.");
//            #endif
//
//            string algorithm1 = config.algorithmsParameters[0].Item1;
//            string algorithm2 = config.algorithmsParameters[1].Item1;
//            string boardGameName = config.gameName.ToLower();
//
//            ISimulationStrategy[] strategies = new ISimulationStrategy[2];
//            IGameState gameState = null;
//
//            for (int algCount = 0; algCount < 2; algCount++)
//            {
//                string algName = algCount == 0 ? algorithm1 : algorithm2;
//
//                switch (boardGameName)
//                {
//                    case TICTACTOE_GAME_NAME:
//                        gameState = new TicTacToeGameState();
//
//                        switch (algName)
//                        {
//                            case MCTS_ALG_NAME:
//                                strategies[algCount] = new TicTacToeMCTSStrategy(GetIntAlgorithmParameter(config, algCount, NUMBEROFBACKUPS_PROP_NAME, 1));
//                                break;
//                            case RANDOM_ALG_NAME:
//                                strategies[algCount] = new TicTacToeRandomStrategy();
//                                break;
//                            case LOOKUP_ALG_NAME:
//                                strategies[algCount] = new TicTacToeLookUpStrategy();
//                                break;
//                            case MINIMAX_ALG_NAME:
//                                strategies[algCount] = new TicTacToeMiniMaxStrategy(algCount + 1);
//                                break;
//                            default:
//                                strategies[algCount] = null;
//                                Console.Error.WriteLine("Algorithm {0} not supported for {1}, this combination of algorithms can not run.", algName, boardGameName);
//                                break;
//                        }
//                        break;
//
//                    case FORZA4_GAME_NAME:
//                        gameState = new Forza4GameState(6, 7);
//
//                        switch (algName)
//                        {
//                            case MCTS_ALG_NAME:
//                                strategies[algCount] = new Forza4MCTSStrategy(GetIntAlgorithmParameter(config, algCount, NUMBEROFBACKUPS_PROP_NAME, 1));
//                                break;
//                            case RANDOM_ALG_NAME:
//                                strategies[algCount] = new Forza4RandomStrategy();
//                                break;
//                            case SCHMIDT_ALG_NAME:
//                                strategies[algCount] = new Forza4SchmidtStrategy();
//                                break;
//                            case MINIMAX_ALG_NAME:
//                                strategies[algCount] = new Forza4MiniMaxStrategy();
//                                break;
//                            case NFRIENDSFLOYD_ALG_NAME:
//                                strategies[algCount] = new Forza4NFriendsFloydStrategy();
//                                break;
//                            default:
//                                strategies[algCount] = null;
//                                Console.Error.WriteLine("Algorithm {0} not supported for {1}, this combination of algorithms can not run.", algName, boardGameName);
//                                break;
//                        }
//                        break;
//
////                    case OTHELLO_GAME_NAME:
////                        gameState = new OthelloGameState(config.boardSize);
////
////                        switch (algName)
////                        {
////                            case MCTS_ALG_NAME:
////                                var iterations = GetIntAlgorithmParameter(config, algCount, NUMBEROFBACKUPS_PROP_NAME, -1);
////                                if (iterations == -1)
////                                {
////                                    iterations = GetIntAlgorithmParameter(config, algCount, STARTINGNUMBEROFBACKUPS_PROP_NAME, 1) * 2;
////                                }
////                                strategies[algCount] = new OthelloMCTSStrategy(iterations);
////                                break;
////                            case RANDOM_ALG_NAME:
////                                strategies[algCount] = new OthelloRandomStrategy();
////                                break;
////                            case WINOTHELLO_ALG_NAME:
////                                strategies[algCount] = new OthelloWinOthelloStrategy(
////                                    GetBoolAlgorithmParameter(config, algCount, USEHEURISTIC_PROP_NAME, false),
////                                    GetIntAlgorithmParameter(config, algCount, DEPTH_PROP_NAME, 3));
////                                break;
////                            case MINIMAXVARIANTS_ALG_NAME:
////                                strategies[algCount] = new OthelloMiniMaxVariantsStrategy(GetIntAlgorithmParameter(config, algCount, SEARCHALGORITHM_PROP_NAME, 1));
////                                break;
////                            case SIMPLEWITHHEURISTIC_ALG_NAME:
////                                strategies[algCount] = new OthelloSimpleWithHeuristicFunctionStrategy();
////                                break;
////                            default:
////                                strategies[algCount] = null;
////                                Console.Error.WriteLine("Algorithm {0} not supported for {1}, this combination of algorithms can not run.", algName, boardGameName);
////                                break;
////                        }
////                        break;
////
////                    case GOMOKU_GAME_NAME:
////                        gameState = new GomokuGameState(config.boardSize);
////
////                        switch (algName)
////                        {
////                            case MCTS_ALG_NAME:
////                                strategies[algCount] = new GomokuMCTSStrategy(GetIntAlgorithmParameter(config, algCount, NUMBEROFBACKUPS_PROP_NAME, 1));
////                                break;
////                            case RANDOM_ALG_NAME:
////                                strategies[algCount] = new GomokuRandomStrategy();
////                                break;
////                            case DTHOAI_ALG_NAME:
////                                strategies[algCount] = new GomokuDthoaiStrategy(
////                                    gameState,
////                                    GetStringAlgorithmParameter(config, algCount, VARIATION_PROP_NAME),
////                                    GetIntAlgorithmParameter(config, algCount, DEPTH_PROP_NAME, 3));
////                                break;
////                            default:
////                                strategies[algCount] = null;
////                                Console.Error.WriteLine("Algorithm {0} not supported for {1}, this combination of algorithms can not run.", algName, boardGameName);
////                                break;
////                        }
////                        break;
////
////                    case QUARTO_GAME_NAME:
////                        gameState = new QuartoGameState(config.boardSize);
////
////                        switch (algName)
////                        {
////                            case MCTS_ALG_NAME:
////                                strategies[algCount] = new QuartoMCTSStrategy(GetIntAlgorithmParameter(config, algCount, NUMBEROFBACKUPS_PROP_NAME, 1));
////                                break;
////                            case RANDOM_ALG_NAME:
////                                strategies[algCount] = new QuartoRandomStrategy();
////                                break;
////                            case MINIMAX_ALG_NAME:
////                                strategies[algCount] = new QuartoMiniMaxStrategy(
////                                    GetBoolAlgorithmParameter(config, algCount, USEHEURISTIC_PROP_NAME, false),
////                                    GetIntAlgorithmParameter(config, algCount, DEPTH_PROP_NAME, 3),
////                                    GetIntAlgorithmParameter(config, algCount, SPEEDUP_PROP_NAME, 0));
////                                break;
////                            case NOVICE_ALG_NAME:
////                                strategies[algCount] = new QuartoNoviceStrategy();
////                                break;
////                            case QUARTOLIB_ALG_NAME:
////                                strategies[algCount] = new QuartoLibSimulationStrategy();
////                                break;
////                            default:
////                                strategies[algCount] = null;
////                                Console.Error.WriteLine("Algorithm {0} not supported for {1}, this combination of algorithms can not run.", algName, boardGameName);
////                                break; 
////                        }
////
////                        break;
//                    default:
//                        throw new Exception(string.Format("{0} game not recognized", boardGameName));
//                }
//            }
//
////            if (strategies[0] == null || strategies[1] == null)
////            {
////                //it means we have received in input the combination of two algorithms that are not defined for the selected game
////                throw new Exception(string.Format("{0} and {1} are not both defined game for game {2}.", algorithm1, algorithm2, boardGameName));
////            }
////
////            #if DEBUG
////            Console.WriteLine("No issues found in creating algorithm objects. Attempting to run test.");
////            #endif
////
////            foreach (KeyValuePair<string,object> kvp in config.algorithmsParameters[0].Item2)
////            {
////                logger.player1params = string.Format("{0}{1}={2}|", logger.player1params, kvp.Key, kvp.Value);
////            }
////            if (string.IsNullOrEmpty(logger.player1params))
////            {
////                logger.player1params = "none";
////            }
////            logger.player1params = logger.player1params.TrimEnd('|');
////
////            foreach (KeyValuePair<string,object> kvp in config.algorithmsParameters[1].Item2)
////            {
////                logger.player2params = string.Format("{0}{1}={2}|", logger.player2params, kvp.Key, kvp.Value);
////            }
////            if (string.IsNullOrEmpty(logger.player2params))
////            {
////                logger.player2params = "none";
////            }
////            logger.player2params = logger.player2params.TrimEnd('|');
////
////            bool[] isMCTS = new bool[2];
////            isMCTS[0] = strategies[0] is IMCTSSimulationStrategy;
////            isMCTS[1] = strategies[1] is IMCTSSimulationStrategy;
////
////            if ((isMCTS[0] && !isMCTS[1]) || (!isMCTS[0] && isMCTS[1]))
////            {
////                //minimize
////                int mctsStrategyIndex = strategies[0] is IMCTSSimulationStrategy ? 0 : 1;
////
////                // If the numberOfBackups is explicitely set we don't want to minimize
////                if (GetIntAlgorithmParameter(config, mctsStrategyIndex, NUMBEROFBACKUPS_PROP_NAME, -1) == -1)
////                {
////                    //var minimizationBreakStrategy = GetStringAlgorithmParameter(config, mctsStrategyIndex, MINIMIZATIONBREAKSTRATEGY_PROP_NAME, BoardGames.Tests.TestsUtils.MINIMIZATION_BREAK_STRATEGY_WINNING_RATIO);
////                    var startingNumberOfBackups = GetIntAlgorithmParameter(config, mctsStrategyIndex, STARTINGNUMBEROFBACKUPS_PROP_NAME, 1);
////
////                    #if DEBUG
////                    Console.WriteLine("Minimization step detected. Attempting to minimize MCTS number of iterations by using {0} as break strategy.", minimizationBreakStrategy);
////                    #endif
////
////                    int minNumberOfIterations =
////                        BoardGames.Tests.TestsUtils.MinimizeMCTSNumberOfIterations(gameState, strategies[0], strategies[1], logger, startingNumberOfBackups, config.precision, config.numberOfRuns, config.numberOfGamesToRun, minimizationBreakStrategy); 
////
////                    #if DEBUG
////                    Console.WriteLine("Minimization step terminated. {0} number of iterations found.", minNumberOfIterations);
////                    #endif
////
////                    if (strategies[0] is IMCTSSimulationStrategy)
////                    {
////                        (strategies[0] as IMCTSSimulationStrategy).iterations = minNumberOfIterations;
////                        logger.player1params = string.Format("iterations={0}", minNumberOfIterations);
////                    }
////                    else
////                    {
////                        (strategies[1] as IMCTSSimulationStrategy).iterations = minNumberOfIterations;
////                        logger.player2params = string.Format("iterations={0}", minNumberOfIterations);
////                    }
////                }
////            }
////
////            #if DEBUG
////            Console.WriteLine("Playing a match of {0} games. {1} (with parameters {2}) Vs {3} (with parameters {4}).", config.numberOfGamesToRun, algorithm1, logger.player1params, algorithm2, logger.player2params);
////            #endif
////
////            //just play
////            //BoardGames.Tests.TestsUtils.RunGames(gameState, strategies[0], strategies[1], config.numberOfGamesToRun, logger);
//        }
//    }
//}