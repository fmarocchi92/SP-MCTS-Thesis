using System;
using Common.Abstract;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace BoardGames
{
    public class Logger
    {
        private string filePath;

        public string player1Name;
        public string player2Name;

        public int player1Wins;
        public int player2Wins;

        public int winner;

        public int numberOfGamesToRun;
        public int runningGameIndex;

        public int numberOfMatchesToRun;
        public int runningMatchIndex;

        public string player1params;
        public string player2params;

        public int player1Score;
        public int player2Score;

        public List<double> p1MovesTimes;
        public List<double> p2MovesTimes;

        public int lastMove;
        public string board;
        public int currentRunningPlayer;
        public double matchQuality;
        public int MCTSMinimiazationLower;
        public int MCTSMinimiazationUpper;
        public int MCTSMinimizationFoundNumberOfIterations;

        private double p1MovesTotalTime;
        private double p1AverageTimePerMove;
        private double p1MovesVarianceTime;
        private double p1MovesMedianTime;

        private double p2MovesTotalTime;
        private double p2AverageTimePerMove;
        private double p2MovesVarianceTime;
        private double p2MovesMedianTime;

        public bool isMinimizing;

        public Logger(string logFilePath)
        {
            filePath = logFilePath;
            p1MovesTimes = new List<double>();
            p2MovesTimes = new List<double>();
            numberOfMatchesToRun = 1;
            runningMatchIndex = 0;
        }

        private void LogCompact(string logMessage)
        {
            using (StreamWriter w = File.AppendText(this.filePath))
            {
                w.WriteLine(logMessage);
            }
        }

        private void ComputeTimes()
        {
            p1MovesTotalTime = p1MovesTimes.Sum();
            p1AverageTimePerMove = p1MovesTimes.Average();
            p1MovesVarianceTime = (from time in p1MovesTimes
                                            select Math.Pow((time - p1AverageTimePerMove), 2)).Average();
            p1MovesMedianTime = p1MovesTimes.OrderBy(time => time).ElementAt((int)Math.Floor((double)p1MovesTimes.Count / 2));

            p2MovesTotalTime = p2MovesTimes.Sum();
            p2AverageTimePerMove = p2MovesTimes.Average();
            p2MovesVarianceTime = (from time in p2MovesTimes
                                            select Math.Pow((time - p2AverageTimePerMove), 2)).Average();
            p2MovesMedianTime = p2MovesTimes.OrderBy(time => time).ElementAt((int)Math.Floor((double)p2MovesTimes.Count / 2));
        }

        public string FlushSingleGameStepLog(bool gameIsOver = false)
        {
            string compactLogString = string.Empty;

            if (isMinimizing)
            {
                if (gameIsOver)
                {
                    compactLogString = string.Format("<==>\t{0}/{1}\t{2}/{3}\tminimizing[{4};{5}]\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15:F}\t{16}",
                        (runningGameIndex + 1).ToString().PadLeft(numberOfGamesToRun.ToString().Length, '0'),
                        numberOfGamesToRun,
                        (runningMatchIndex + 1).ToString().PadLeft(numberOfMatchesToRun.ToString().Length, '0'),
                        numberOfMatchesToRun,
                        MCTSMinimiazationLower,
                        MCTSMinimiazationUpper,
                        player1Name,
                        player2Name,
                        !string.IsNullOrEmpty(player1params) ? player1params : "none",
                        !string.IsNullOrEmpty(player2params) ? player2params : "none",
                        winner,
                        player1Score,
                        player2Score,
                        player1Wins,
                        player2Wins,
                        matchQuality,
                        board);
                }
                else
                {
                    compactLogString = string.Format("==>\t{0}/{1}\t{2}/{3}\tminimizing[{4};{5}]\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13:F}\t{14:F}\t{15}\t{16}",
                        (runningGameIndex + 1).ToString().PadLeft(numberOfGamesToRun.ToString().Length, '0'),
                        numberOfGamesToRun,
                        (runningMatchIndex + 1).ToString().PadLeft(numberOfMatchesToRun.ToString().Length, '0'),
                        numberOfMatchesToRun,
                        MCTSMinimiazationLower,
                        MCTSMinimiazationUpper,
                        player1Name,
                        player2Name,
                        !string.IsNullOrEmpty(player1params) ? player1params : "none",
                        !string.IsNullOrEmpty(player2params) ? player2params : "none",
                        currentRunningPlayer,
                        player1Score,
                        player2Score,
                        currentRunningPlayer == 1 ? p1MovesTimes.Last() : 0,
                        currentRunningPlayer == 2 ? p2MovesTimes.Last() : 0,
                        lastMove,
                        board);
                }
            }
            else
            {
                if (gameIsOver)
                {
                    compactLogString = string.Format("<==>\t{0}/{1}\t{2}/{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11}\t{12}\t{13}\t{14}\t{15}",
                        (runningGameIndex + 1).ToString().PadLeft(numberOfGamesToRun.ToString().Length, '0'),
                        numberOfGamesToRun,
                        1,
                        1,
                        numberOfMatchesToRun,
                        player1Name,
                        player2Name,
                        !string.IsNullOrEmpty(player1params) ? player1params : "none",
                        !string.IsNullOrEmpty(player2params) ? player2params : "none",
                        winner,
                        player1Score,
                        player2Score,
                        player1Wins,
                        player2Wins,
                        matchQuality,
                        board);
                }
                else
                {
                    compactLogString = string.Format("==>\t{0}/{1}\t{2}/{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11:F}\t{12:F}\t{13}\t{14}",
                        (runningGameIndex + 1).ToString().PadLeft(numberOfGamesToRun.ToString().Length, '0'),
                        numberOfGamesToRun,
                        1,
                        1,
                        player1Name,
                        player2Name,
                        !string.IsNullOrEmpty(player1params) ? player1params : "none",
                        !string.IsNullOrEmpty(player2params) ? player2params : "none",
                        currentRunningPlayer,
                        player1Score,
                        player2Score,
                        currentRunningPlayer == 1 ? p1MovesTimes.Last() : 0,
                        currentRunningPlayer == 2 ? p2MovesTimes.Last() : 0,
                        lastMove,
                        board);
                }
            }

            LogCompact(compactLogString);

            return compactLogString;
        }

        public void FlushMatchLog()
        {
            ComputeTimes();

            string compactLogString;

            if (isMinimizing)
            {
                compactLogString = string.Format(">>\t{0}/{1}\tminimizing[{2};{3}]\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}\t{11:F}\t{12:F}\t{13:F}\t{14:F}\t{15:F}\t{16:F}\t{17:F}\t{18:F}\t{19:F}", 
                    (runningMatchIndex + 1).ToString().PadLeft(numberOfMatchesToRun.ToString().Length, '0'),
                    numberOfMatchesToRun,
                    MCTSMinimiazationLower,
                    MCTSMinimiazationUpper,
                    player1Name,
                    player2Name,
                    !string.IsNullOrEmpty(player1params) ? player1params : "none",
                    !string.IsNullOrEmpty(player2params) ? player2params : "none",
                    player1Wins,
                    player2Wins,
                    numberOfGamesToRun - player1Wins - player2Wins,
                    matchQuality,
                    p1MovesTotalTime,
                    p2MovesTotalTime,
                    p1AverageTimePerMove,
                    p2AverageTimePerMove,
                    p1MovesVarianceTime,
                    p2MovesVarianceTime,
                    p1MovesMedianTime,
                    p2MovesMedianTime);
            }
            else
            {
                compactLogString = string.Format(">>\t{0}/{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9:F}\t{10:F}\t{11:F}\t{12:F}\t{13:F}\t{14:F}\t{15:F}\t{16:F}\t{17:F}", 
                    1,
                    1,
                    player1Name,
                    player2Name,
                    !string.IsNullOrEmpty(player1params) ? player1params : "none",
                    !string.IsNullOrEmpty(player2params) ? player2params : "none",
                    player1Wins,
                    player2Wins,
                    numberOfGamesToRun - player1Wins - player2Wins,
                    matchQuality,
                    p1MovesTotalTime,
                    p2MovesTotalTime,
                    p1AverageTimePerMove,
                    p2AverageTimePerMove,
                    p1MovesVarianceTime,
                    p2MovesVarianceTime,
                    p1MovesMedianTime,
                    p2MovesMedianTime);
            }

            LogCompact(compactLogString);
        }

        public string FlushMinimizationEndLog()
        {   
            string compactLogString;

            compactLogString = string.Format(">=>\t{0}/{1}\t{2}\t{3}\t{4}\t{5}\t{6}\t{7}\t{8}\t{9}\t{10}", 
                (runningMatchIndex + 1).ToString().PadLeft(numberOfMatchesToRun.ToString().Length, '0'),
                numberOfMatchesToRun,
                player1Name,
                player2Name,
                !string.IsNullOrEmpty(player1params) ? player1params : "none",
                !string.IsNullOrEmpty(player2params) ? player2params : "none",
                player1Wins,
                player2Wins,
                numberOfGamesToRun - player1Wins - player2Wins,
                MCTSMinimizationFoundNumberOfIterations,
                matchQuality);

            LogCompact(compactLogString);

            return compactLogString;
        }

        public void LogErrorMessage(string logMessage)
        {
            LogCompact(string.Format("=x=\t{0}", logMessage));
        }
    }
}

