#define TRUESKILL

using System;
using System.Diagnostics;
using System.IO;
using Common;
using Common.Abstract;
using BoardGames;
using MCTS2016.Puzzles.Sokoban;
using MCTS2016.Puzzles.SameGame;
using System.Collections.Generic;
using MCTS2016.Common.Abstract;

#if TRUESKILL
using Moserware.Skills;
#endif


class MainClass
{
    /// quality of the match, the higher it is the more balance it is
    private static double cQualityThreshold = 0.98f;

    /// how much the upper limit is increased when MCTS is still weaker (1 means +100%)
    private static double cUpperLimitIncrease = .25f;

    /// if the difference between lower and upper limit is below this threshold, it is not worth keep looking
    private static int cMinBackUpDifference = 10;

	private static bool outputToFile = false;
	private static StreamWriter outputStreamForConsole;

    static private Tuple<string,int>[] TicTacToeStrategies = new Tuple<string,int>[]{
		new Tuple<string,int>("Random",1),
		new Tuple<string,int>("LookUp",-1),
		new Tuple<string,int>("MiniMax",8),
		new Tuple<string,int>("MCTS",1),
		new Tuple<string,int>("MCTS",100),
		new Tuple<string,int>("MCTS",1000),
	};

	static private ITicTacToeSimulationStrategy[] strategies = new ITicTacToeSimulationStrategy[]{
		new TicTacToeRandomStrategy(),
		new TicTacToeLookUpStrategy(),
		new TicTacToeMiniMaxStrategy(8),
		new TicTacToeMCTSStrategy(1),
		new TicTacToeMCTSStrategy(100),
		new TicTacToeMCTSStrategy(1000),
	};

	//public static void Main (string[] args)
	//{
 //       //RedirectOutputToFile ("output.txt");

 //       //TicTacToeMCTSEval(new Tuple<string, int>("MCTS", 100), 10, 0);

 //       //		if (outputToFile)
 //       //			CloseOutputToFile ();
 //       //		else 
 //       //Console.ReadLine();

 //       //SokobanTest();
 //       SamegameTest();

 //   }
		
    private static void SokobanTest()
    {
        
    }

    private static void SamegameTest()
    {
        ////string level = "00100\n20330\n10230\n00221\n00221";

        //int[] scores = new int[20];
        //int[][][] preset = new int[20][][];

        //preset[0] = new int[][]{
        //    new int []{3, 1, 1, 4, 1, 0, 4, 0, 4, 4, 1, 1, 0, 2, 3},
        //    new int []{3, 3, 2, 0, 4, 4, 1, 3, 1, 2, 0, 0, 4, 0, 4},
        //    new int []{0, 2, 3, 4, 3, 0, 3, 0, 0, 3, 4, 4, 1, 1, 1},
        //    new int []{2, 3, 4, 0, 2, 3, 0, 2, 4, 4, 4, 3, 0, 2, 3},
        //    new int []{1, 2, 1, 3, 1, 2, 0, 1, 2, 1, 0, 3, 4, 0, 1},
        //    new int []{0, 4, 4, 3, 0, 3, 4, 2, 2, 2, 0, 2, 3, 4, 0},
        //    new int []{2, 4, 3, 4, 2, 3, 1, 1, 1, 3, 4, 1, 0, 3, 1},
        //    new int []{1, 0, 0, 4, 0, 3, 1, 2, 1, 0, 4, 1, 3, 3, 1},
        //    new int []{1, 3, 3, 2, 0, 4, 3, 1, 3, 0, 4, 1, 0, 0, 3},
        //    new int []{0, 3, 3, 4, 2, 3, 0, 0, 2, 1, 2, 3, 4, 0, 1},
        //    new int []{0, 4, 1, 2, 0, 1, 3, 4, 3, 3, 4, 1, 4, 0, 4},
        //    new int []{2, 2, 3, 1, 0, 4, 0, 1, 2, 4, 1, 3, 3, 0, 1},
        //    new int []{3, 3, 0, 2, 3, 2, 1, 4, 3, 1, 3, 0, 2, 1, 3},
        //    new int []{1, 0, 3, 2, 1, 4, 4, 4, 4, 0, 4, 2, 1, 3, 4},
        //    new int []{1, 0, 1, 0, 1, 1, 2, 2, 1, 0, 0, 1, 4, 3, 2}
        //};

        //preset[1] = new int[][]{
        //    new int []{3, 3, 0, 1, 0, 2, 1, 2, 3, 2, 3, 1, 1, 1, 0},
        //    new int []{4, 1, 3, 4, 0, 3, 3, 2, 2, 4, 0, 2, 4, 0, 0},
        //    new int []{2, 3, 2, 2, 0, 3, 1, 0, 4, 4, 0, 2, 4, 0, 4},
        //    new int []{0, 3, 4, 4, 2, 2, 1, 3, 3, 1, 3, 0, 3, 3, 4},
        //    new int []{0, 0, 2, 1, 2, 1, 3, 4, 3, 2, 1, 2, 3, 1, 4},
        //    new int []{1, 2, 4, 2, 0, 0, 0, 1, 1, 1, 0, 0, 2, 4, 4},
        //    new int []{1, 0, 3, 3, 3, 2, 1, 0, 4, 2, 4, 1, 4, 3, 0},
        //    new int []{4, 4, 3, 3, 0, 2, 3, 3, 4, 3, 0, 3, 0, 0, 4},
        //    new int []{3, 3, 3, 1, 4, 3, 3, 3, 0, 4, 2, 0, 3, 2, 0},
        //    new int []{2, 4, 1, 1, 1, 1, 4, 0, 0, 3, 0, 4, 0, 4, 3},
        //    new int []{3, 3, 0, 1, 4, 1, 2, 1, 1, 0, 3, 4, 2, 1, 0},
        //    new int []{2, 2, 3, 3, 2, 0, 4, 3, 3, 4, 0, 4, 3, 3, 1},
        //    new int []{0, 1, 3, 2, 1, 2, 1, 1, 0, 2, 4, 1, 4, 0, 3},
        //    new int []{4, 1, 4, 0, 2, 1, 3, 1, 3, 1, 4, 0, 1, 0, 3},
        //    new int []{1, 3, 2, 3, 2, 2, 4, 2, 2, 4, 3, 0, 3, 1, 1 }
        //};

        //preset[2] = new int[][]{
        //    new int []{4, 2, 4, 3, 1, 0, 3, 3, 2, 2, 4, 3, 1, 4, 2},
        //    new int []{3, 0, 3, 4, 0, 3, 3, 3, 2, 4, 4, 3, 1, 3, 3},
        //    new int []{2, 0, 4, 4, 0, 1, 2, 2, 2, 3, 4, 0, 4, 4, 0},
        //    new int []{0, 4, 3, 0, 0, 2, 4, 2, 1, 2, 0, 3, 2, 4, 2},
        //    new int []{0, 2, 0, 2, 0, 1, 1, 3, 2, 1, 1, 2, 3, 4, 0},
        //    new int []{1, 0, 1, 0, 4, 3, 3, 3, 4, 2, 2, 2, 3, 4, 1},
        //    new int []{2, 3, 4, 3, 4, 2, 2, 4, 2, 4, 3, 4, 4, 0, 1},
        //    new int []{4, 2, 3, 2, 2, 0, 1, 2, 4, 3, 3, 0, 0, 2, 1},
        //    new int []{3, 4, 4, 3, 0, 4, 3, 4, 1, 0, 0, 2, 1, 4, 3},
        //    new int []{4, 0, 1, 3, 1, 0, 2, 3, 0, 2, 0, 2, 3, 0, 1},
        //    new int []{4, 2, 0, 0, 0, 2, 2, 1, 0, 2, 3, 1, 1, 3, 1},
        //    new int []{0, 3, 1, 1, 3, 3, 2, 1, 2, 0, 0, 4, 2, 4, 1},
        //    new int []{2, 1, 4, 4, 4, 0, 3, 3, 4, 2, 0, 0, 2, 0, 0},
        //    new int []{1, 0, 4, 4, 0, 1, 3, 2, 4, 0, 4, 2, 0, 0, 1},
        //    new int []{2, 2, 2, 2, 3, 3, 0, 4, 3, 3, 4, 0, 4, 1, 2 }
        //    };

        //preset[3] = new int [][]{
        //    new int []{4, 2, 2, 4, 1, 3, 3, 2, 4, 0, 4, 2, 3, 4, 2},
        //    new int []{2, 0, 2, 1, 2, 1, 0, 1, 2, 1, 1, 3, 0, 4, 2},
        //    new int []{0, 2, 3, 2, 0, 0, 4, 1, 0, 4, 3, 0, 0, 3, 2},
        //    new int []{2, 2, 3, 1, 1, 0, 0, 1, 0, 1, 1, 4, 3, 0, 0},
        //    new int []{4, 2, 0, 4, 2, 2, 0, 3, 0, 0, 2, 2, 1, 4, 2},
        //    new int []{1, 4, 3, 3, 2, 3, 0, 4, 4, 0, 0, 2, 2, 3, 0},
        //    new int []{2, 1, 1, 4, 1, 0, 1, 0, 4, 4, 1, 0, 4, 1, 3},
        //    new int []{3, 3, 0, 2, 1, 3, 1, 1, 4, 0, 2, 3, 3, 3, 3},
        //    new int []{2, 3, 3, 1, 3, 1, 0, 4, 1, 0, 1, 2, 3, 0, 4},
        //    new int []{3, 2, 1, 1, 3, 4, 0, 2, 4, 2, 4, 2, 0, 2, 0},
        //    new int []{0, 3, 0, 1, 4, 0, 0, 0, 4, 2, 1, 0, 2, 4, 0},
        //    new int []{2, 0, 1, 4, 2, 3, 1, 4, 2, 0, 1, 0, 3, 4, 2},
        //    new int []{0, 4, 2, 0, 3, 4, 4, 3, 1, 1, 3, 4, 2, 1, 4},
        //    new int []{4, 2, 4, 0, 4, 3, 0, 2, 2, 4, 1, 4, 3, 4, 1},
        //    new int []{4, 3, 2, 2, 2, 1, 1, 2, 3, 3, 1, 2, 0, 3, 2 }
        //};

        //preset[4] = new int [][]{
        //    new int []{3, 4, 4, 3, 2, 3, 2, 1, 3, 4, 1, 2, 3, 3, 2},
        //    new int []{2, 0, 2, 0, 3, 1, 0, 3, 1, 1, 2, 1, 4, 3, 4},
        //    new int []{1, 3, 1, 0, 3, 1, 3, 2, 3, 4, 0, 0, 1, 4, 1},
        //    new int []{0, 2, 1, 0, 2, 2, 2, 4, 1, 0, 4, 4, 3, 3, 2},
        //    new int []{2, 3, 1, 3, 0, 4, 0, 2, 3, 0, 1, 4, 4, 2, 3},
        //    new int []{3, 1, 3, 3, 2, 3, 0, 1, 0, 4, 3, 4, 0, 1, 4},
        //    new int []{4, 4, 4, 2, 2, 3, 0, 0, 0, 1, 0, 1, 2, 1, 3},
        //    new int []{2, 1, 3, 4, 4, 0, 4, 1, 0, 4, 0, 1, 2, 1, 3},
        //    new int []{3, 4, 3, 1, 2, 0, 1, 3, 3, 0, 1, 4, 2, 0, 0},
        //    new int []{2, 3, 0, 1, 2, 4, 3, 3, 0, 1, 1, 2, 2, 3, 3},
        //    new int []{4, 4, 1, 0, 3, 3, 4, 4, 2, 2, 4, 2, 0, 3, 0},
        //    new int []{3, 1, 0, 4, 3, 2, 0, 2, 3, 1, 4, 3, 1, 2, 2},
        //    new int []{2, 2, 3, 0, 2, 4, 1, 3, 0, 3, 2, 1, 3, 4, 2},
        //    new int []{2, 4, 3, 1, 3, 0, 3, 2, 0, 4, 3, 2, 2, 3, 4},
        //    new int []{0, 4, 2, 2, 2, 3, 2, 0, 1, 1, 4, 0, 1, 3, 3 }
        //};

        //preset[5] = new int [][]{
        //    new int []{2, 4, 2, 0, 4, 2, 2, 3, 1, 0, 1, 3, 4, 2, 0},
        //    new int []{2, 3, 3, 2, 3, 1, 3, 3, 0, 1, 4, 1, 0, 0, 1},
        //    new int []{0, 4, 3, 0, 3, 1, 3, 3, 3, 1, 0, 2, 4, 2, 1},
        //    new int []{3, 0, 1, 0, 1, 2, 3, 0, 0, 2, 1, 1, 1, 4, 4},
        //    new int []{0, 1, 1, 1, 2, 0, 2, 1, 3, 4, 2, 0, 3, 1, 0},
        //    new int []{1, 1, 1, 4, 1, 1, 0, 0, 1, 1, 4, 1, 1, 2, 1},
        //    new int []{3, 3, 0, 1, 1, 3, 2, 0, 0, 0, 0, 1, 2, 0, 1},
        //    new int []{0, 3, 0, 3, 4, 0, 1, 1, 2, 1, 4, 2, 1, 0, 2},
        //    new int []{1, 2, 2, 2, 2, 3, 4, 1, 3, 1, 4, 2, 4, 1, 1},
        //    new int []{2, 2, 0, 3, 3, 0, 2, 2, 3, 3, 2, 2, 1, 0, 3},
        //    new int []{2, 4, 0, 0, 4, 0, 4, 3, 4, 4, 3, 4, 1, 4, 4},
        //    new int []{2, 1, 2, 3, 1, 1, 2, 2, 1, 0, 3, 1, 4, 4, 0},
        //    new int []{2, 3, 2, 2, 1, 1, 4, 0, 1, 4, 4, 0, 4, 3, 3},
        //    new int []{1, 1, 3, 0, 3, 1, 4, 3, 4, 1, 0, 4, 1, 1, 4},
        //    new int []{0, 4, 4, 4, 2, 2, 4, 3, 1, 1, 3, 2, 4, 4, 1 }
        //};

        //preset[6] = new int [][]{
        //    new int []{3, 4, 0, 3, 1, 2, 0, 1, 3, 1, 2, 4, 1, 1, 3},
        //    new int []{3, 1, 4, 3, 0, 0, 1, 3, 0, 2, 0, 4, 4, 4, 4},
        //    new int []{0, 4, 3, 2, 1, 1, 0, 2, 2, 1, 3, 4, 0, 2, 3},
        //    new int []{2, 4, 0, 1, 3, 3, 3, 2, 2, 2, 2, 0, 2, 2, 0},
        //    new int []{0, 4, 0, 0, 2, 1, 0, 1, 4, 3, 3, 3, 1, 0, 2},
        //    new int []{1, 0, 4, 1, 2, 4, 4, 2, 2, 0, 0, 0, 3, 4, 4},
        //    new int []{4, 2, 1, 3, 1, 2, 0, 1, 3, 4, 2, 2, 1, 3, 2},
        //    new int []{1, 1, 1, 0, 3, 0, 3, 1, 3, 3, 1, 1, 2, 3, 0},
        //    new int []{1, 2, 4, 3, 1, 4, 1, 1, 1, 0, 2, 3, 0, 3, 3},
        //    new int []{0, 4, 1, 3, 4, 0, 4, 1, 4, 0, 4, 2, 3, 0, 1},
        //    new int []{0, 4, 3, 4, 2, 4, 1, 3, 1, 3, 0, 4, 3, 0, 0},
        //    new int []{3, 1, 1, 1, 0, 4, 2, 0, 3, 0, 4, 4, 2, 4, 4},
        //    new int []{4, 0, 4, 3, 1, 4, 1, 3, 2, 3, 0, 1, 0, 1, 1},
        //    new int []{3, 3, 4, 2, 4, 4, 2, 0, 3, 4, 3, 0, 1, 0, 3},
        //    new int []{0, 2, 3, 4, 4, 2, 4, 1, 0, 0, 0, 4, 2, 4, 0 }
        //};

        //preset[7] = new int [][]{
        //    new int []{3, 1, 3, 1, 4, 4, 2, 2, 0, 4, 0, 2, 2, 3, 1},
        //    new int []{1, 1, 2, 3, 3, 1, 0, 2, 2, 2, 0, 2, 4, 1, 1},
        //    new int []{4, 4, 1, 2, 4, 2, 1, 4, 1, 2, 3, 3, 2, 1, 4},
        //    new int []{1, 0, 2, 2, 3, 4, 1, 3, 2, 2, 1, 3, 4, 3, 2},
        //    new int []{3, 1, 1, 0, 0, 1, 2, 0, 3, 2, 4, 3, 4, 3, 1},
        //    new int []{1, 1, 3, 0, 4, 2, 1, 3, 0, 1, 2, 4, 4, 0, 3},
        //    new int []{0, 1, 1, 1, 0, 1, 2, 3, 3, 1, 0, 1, 0, 0, 3},
        //    new int []{2, 3, 2, 3, 1, 1, 1, 2, 4, 0, 2, 1, 2, 3, 3},
        //    new int []{0, 1, 3, 0, 4, 3, 1, 1, 4, 0, 1, 3, 0, 3, 0},
        //    new int []{1, 3, 3, 0, 3, 0, 0, 0, 3, 4, 1, 3, 0, 0, 0},
        //    new int []{4, 4, 2, 1, 3, 1, 0, 1, 1, 3, 1, 3, 2, 4, 3},
        //    new int []{0, 3, 0, 2, 3, 1, 1, 1, 3, 3, 1, 2, 3, 2, 2},
        //    new int []{3, 2, 2, 0, 3, 0, 3, 1, 0, 0, 3, 3, 2, 4, 2},
        //    new int []{0, 1, 2, 2, 0, 2, 4, 4, 1, 3, 4, 3, 1, 1, 4},
        //    new int []{4, 4, 3, 0, 4, 3, 3, 3, 4, 1, 3, 4, 4, 3, 1 }
        //};

        //preset[8] = new int [][]{
        //    new int []{1, 3, 4, 0, 2, 1, 4, 3, 0, 0, 1, 2, 3, 1, 1},
        //    new int []{0, 0, 3, 0, 3, 2, 3, 0, 1, 4, 0, 3, 3, 3, 2},
        //    new int []{2, 4, 1, 2, 0, 1, 2, 1, 0, 0, 3, 1, 0, 2, 2},
        //    new int []{0, 2, 1, 2, 1, 1, 0, 0, 0, 3, 3, 0, 1, 1, 3},
        //    new int []{1, 4, 2, 3, 1, 3, 3, 0, 4, 2, 3, 1, 0, 4, 4},
        //    new int []{2, 1, 1, 4, 1, 1, 4, 0, 4, 4, 2, 0, 0, 4, 0},
        //    new int []{3, 4, 4, 3, 0, 0, 2, 0, 4, 1, 2, 4, 0, 3, 3},
        //    new int []{1, 4, 0, 4, 0, 0, 3, 3, 4, 4, 0, 2, 2, 4, 4},
        //    new int []{0, 1, 0, 4, 2, 3, 3, 0, 0, 2, 0, 4, 3, 4, 1},
        //    new int []{3, 1, 1, 4, 2, 4, 0, 0, 2, 0, 3, 1, 2, 4, 3},
        //    new int []{0, 0, 4, 2, 4, 1, 2, 0, 0, 0, 3, 0, 3, 3, 3},
        //    new int []{0, 0, 1, 0, 1, 2, 2, 0, 3, 4, 3, 2, 4, 3, 4},
        //    new int []{1, 1, 0, 2, 0, 4, 3, 3, 1, 1, 4, 3, 2, 4, 1},
        //    new int []{0, 1, 2, 2, 3, 4, 0, 3, 1, 4, 0, 0, 3, 1, 1},
        //    new int []{0, 3, 0, 0, 1, 0, 1, 1, 1, 3, 1, 2, 0, 0, 0 }
        //};

        //preset[9] = new int [][]{
        //    new int []{0, 1, 3, 3, 4, 3, 4, 3, 2, 4, 4, 0, 3, 2, 1},
        //    new int []{4, 0, 1, 1, 0, 0, 0, 1, 2, 0, 3, 0, 0, 2, 1},
        //    new int []{1, 2, 4, 3, 0, 2, 0, 2, 3, 4, 3, 1, 2, 2, 3},
        //    new int []{3, 4, 3, 0, 1, 3, 3, 2, 3, 1, 1, 0, 3, 4, 2},
        //    new int []{2, 0, 0, 3, 2, 0, 2, 3, 3, 3, 0, 1, 1, 1, 1},
        //    new int []{2, 4, 2, 2, 1, 4, 3, 2, 1, 4, 0, 1, 4, 4, 1},
        //    new int []{0, 0, 0, 2, 2, 3, 4, 3, 2, 3, 0, 3, 4, 3, 4},
        //    new int []{1, 2, 0, 4, 1, 2, 2, 4, 0, 2, 4, 2, 4, 0, 3},
        //    new int []{3, 4, 3, 3, 1, 1, 0, 4, 4, 2, 1, 0, 0, 1, 3},
        //    new int []{1, 2, 2, 2, 4, 3, 2, 0, 2, 1, 0, 1, 0, 1, 3},
        //    new int []{2, 3, 4, 2, 1, 0, 1, 2, 3, 2, 4, 0, 2, 4, 3},
        //    new int []{1, 3, 2, 4, 3, 0, 4, 4, 1, 1, 4, 1, 2, 4, 0},
        //    new int []{3, 0, 2, 2, 1, 4, 3, 4, 1, 2, 2, 1, 1, 3, 1},
        //    new int []{2, 0, 2, 1, 0, 4, 1, 4, 0, 3, 2, 3, 0, 2, 4},
        //    new int []{0, 3, 1, 1, 0, 1, 4, 1, 4, 1, 1, 1, 0, 4, 2 }
        //};

        //preset[10] = new int [][]{
        //    new int []{4, 1, 2, 0, 2, 3, 4, 1, 4, 4, 1, 4, 3, 1, 3},
        //    new int []{1, 3, 1, 3, 4, 0, 3, 4, 2, 3, 3, 2, 3, 4, 1},
        //    new int []{1, 3, 2, 2, 3, 4, 2, 3, 4, 0, 3, 4, 1, 2, 3},
        //    new int []{1, 3, 2, 4, 0, 2, 0, 0, 1, 2, 1, 3, 4, 4, 2},
        //    new int []{4, 0, 2, 2, 0, 1, 1, 0, 0, 1, 0, 2, 3, 2, 4},
        //    new int []{2, 2, 0, 3, 4, 1, 0, 4, 3, 4, 4, 2, 3, 3, 4},
        //    new int []{4, 4, 0, 2, 0, 3, 4, 1, 1, 4, 4, 2, 0, 1, 1},
        //    new int []{3, 1, 0, 4, 1, 1, 1, 3, 2, 4, 1, 3, 2, 0, 2},
        //    new int []{0, 2, 0, 0, 1, 1, 2, 0, 4, 1, 1, 0, 2, 2, 4},
        //    new int []{3, 1, 0, 4, 3, 4, 3, 1, 1, 0, 0, 3, 2, 3, 4},
        //    new int []{4, 4, 1, 2, 4, 0, 4, 2, 0, 3, 2, 3, 4, 0, 0},
        //    new int []{2, 4, 3, 0, 1, 3, 1, 3, 1, 0, 1, 0, 0, 1, 4},
        //    new int []{1, 2, 1, 2, 0, 0, 3, 0, 1, 1, 0, 2, 3, 1, 2},
        //    new int []{3, 2, 0, 1, 3, 0, 2, 4, 3, 4, 4, 4, 0, 3, 0},
        //    new int []{2, 3, 3, 0, 2, 2, 4, 3, 0, 2, 1, 2, 3, 2, 0 }
        //};

        //preset[11] = new int [][]{
        //    new int []{1, 2, 2, 4, 2, 3, 4, 2, 4, 1, 2, 2, 3, 3, 4},
        //    new int []{3, 1, 1, 4, 1, 1, 1, 1, 1, 2, 1, 1, 4, 1, 0},
        //    new int []{1, 4, 1, 4, 4, 2, 1, 4, 0, 3, 4, 0, 2, 3, 3},
        //    new int []{3, 3, 1, 2, 0, 3, 3, 3, 2, 4, 0, 1, 2, 3, 0},
        //    new int []{4, 3, 4, 1, 3, 0, 4, 4, 3, 4, 0, 4, 0, 0, 2},
        //    new int []{2, 0, 3, 1, 2, 4, 4, 4, 0, 0, 2, 3, 0, 0, 3},
        //    new int []{0, 4, 0, 3, 4, 2, 1, 1, 0, 3, 3, 3, 2, 2, 1},
        //    new int []{0, 2, 0, 3, 1, 4, 0, 0, 1, 2, 0, 3, 4, 1, 2},
        //    new int []{3, 2, 2, 2, 1, 1, 1, 4, 3, 2, 0, 2, 4, 2, 2},
        //    new int []{4, 3, 3, 0, 3, 0, 0, 4, 0, 0, 2, 2, 3, 3, 1},
        //    new int []{4, 2, 3, 4, 1, 2, 3, 1, 3, 0, 4, 4, 4, 0, 2},
        //    new int []{0, 1, 3, 1, 2, 3, 2, 4, 3, 3, 1, 2, 4, 0, 1},
        //    new int []{4, 1, 3, 3, 1, 0, 3, 2, 0, 1, 4, 0, 2, 0, 2},
        //    new int []{4, 0, 2, 4, 1, 0, 0, 4, 2, 0, 0, 4, 4, 3, 0},
        //    new int []{1, 1, 1, 3, 4, 2, 3, 2, 1, 2, 0, 1, 4, 1, 0 }
        //};

        //preset[12] = new int [][]{
        //    new int []{4, 0, 1, 4, 3, 3, 1, 4, 1, 2, 4, 1, 0, 0, 2},
        //    new int []{0, 1, 4, 0, 3, 0, 0, 2, 4, 2, 2, 3, 3, 2, 4},
        //    new int []{0, 2, 1, 0, 3, 3, 3, 0, 0, 4, 4, 3, 1, 1, 4},
        //    new int []{4, 4, 2, 1, 0, 2, 4, 3, 3, 2, 2, 4, 2, 4, 0},
        //    new int []{3, 0, 0, 4, 4, 2, 2, 1, 3, 4, 3, 2, 4, 2, 0},
        //    new int []{0, 4, 1, 4, 4, 4, 4, 4, 1, 2, 3, 4, 2, 3, 3},
        //    new int []{0, 1, 2, 0, 0, 2, 2, 1, 3, 4, 2, 0, 0, 4, 1},
        //    new int []{4, 3, 3, 2, 0, 0, 1, 0, 1, 4, 3, 2, 3, 1, 1},
        //    new int []{3, 4, 2, 2, 0, 2, 3, 3, 3, 0, 0, 1, 2, 1, 3},
        //    new int []{1, 3, 2, 1, 2, 2, 4, 1, 1, 1, 2, 3, 1, 3, 1},
        //    new int []{0, 0, 2, 1, 2, 1, 1, 4, 1, 1, 0, 2, 1, 2, 0},
        //    new int []{4, 1, 2, 1, 0, 3, 1, 0, 3, 4, 0, 4, 3, 3, 2},
        //    new int []{4, 3, 0, 0, 3, 4, 3, 3, 3, 3, 1, 1, 3, 2, 1},
        //    new int []{0, 1, 1, 3, 0, 1, 1, 0, 4, 0, 4, 0, 2, 0, 4},
        //    new int []{2, 2, 1, 4, 4, 2, 2, 0, 3, 4, 3, 0, 2, 4, 3 }
        //};

        //preset[13] = new int [][]{
        //    new int []{2, 2, 4, 0, 2, 4, 0, 0, 1, 4, 0, 3, 4, 3, 3},
        //    new int []{0, 4, 3, 1, 0, 3, 2, 0, 1, 2, 2, 1, 4, 4, 0},
        //    new int []{2, 1, 2, 3, 3, 2, 1, 2, 3, 3, 0, 4, 2, 1, 0},
        //    new int []{4, 4, 3, 3, 2, 4, 1, 0, 1, 4, 4, 0, 4, 2, 1},
        //    new int []{3, 3, 0, 1, 2, 2, 3, 1, 3, 0, 1, 3, 2, 3, 3},
        //    new int []{1, 2, 0, 3, 4, 0, 4, 2, 2, 2, 1, 3, 3, 3, 1},
        //    new int []{4, 0, 0, 1, 1, 1, 1, 4, 3, 3, 2, 1, 3, 2, 0},
        //    new int []{4, 1, 4, 4, 1, 0, 0, 2, 0, 3, 2, 2, 0, 2, 3},
        //    new int []{2, 3, 3, 1, 4, 3, 0, 1, 0, 4, 4, 0, 0, 2, 1},
        //    new int []{0, 1, 2, 2, 4, 3, 1, 1, 4, 4, 2, 4, 4, 2, 4},
        //    new int []{2, 4, 1, 1, 0, 3, 3, 3, 0, 4, 4, 0, 0, 2, 0},
        //    new int []{3, 2, 1, 3, 0, 4, 4, 2, 3, 0, 2, 1, 1, 3, 1},
        //    new int []{0, 4, 3, 3, 1, 2, 0, 2, 2, 1, 2, 3, 0, 0, 1},
        //    new int []{4, 3, 4, 2, 1, 1, 3, 0, 4, 1, 4, 1, 4, 2, 0},
        //    new int []{2, 1, 3, 2, 0, 1, 4, 0, 1, 4, 0, 4, 0, 4, 3 }
        //};

        //preset[14] = new int [][]{
        //    new int []{0, 1, 2, 1, 3, 4, 3, 2, 1, 2, 1, 2, 2, 3, 4},
        //    new int []{4, 0, 0, 1, 3, 0, 4, 2, 0, 4, 4, 4, 2, 1, 1},
        //    new int []{3, 0, 0, 1, 2, 1, 1, 3, 0, 0, 3, 2, 4, 0, 0},
        //    new int []{4, 2, 1, 4, 4, 1, 4, 0, 0, 3, 2, 0, 2, 2, 0},
        //    new int []{3, 3, 4, 2, 1, 2, 4, 1, 3, 4, 0, 4, 2, 3, 0},
        //    new int []{0, 4, 4, 1, 2, 2, 1, 4, 4, 2, 3, 3, 4, 4, 1},
        //    new int []{3, 1, 1, 3, 2, 2, 0, 3, 2, 3, 4, 4, 3, 2, 0},
        //    new int []{2, 4, 1, 3, 2, 0, 2, 4, 4, 4, 1, 4, 4, 0, 0},
        //    new int []{1, 4, 2, 1, 2, 0, 3, 3, 0, 1, 3, 3, 2, 4, 3},
        //    new int []{2, 2, 3, 2, 1, 1, 0, 0, 1, 1, 3, 1, 2, 4, 3},
        //    new int []{2, 1, 0, 2, 2, 0, 3, 2, 2, 1, 4, 1, 1, 4, 0},
        //    new int []{0, 1, 3, 2, 1, 0, 4, 0, 0, 3, 3, 0, 3, 0, 4},
        //    new int []{1, 2, 1, 3, 4, 3, 1, 1, 3, 0, 0, 4, 3, 1, 4},
        //    new int []{0, 3, 3, 3, 1, 1, 4, 0, 0, 4, 2, 4, 1, 0, 3},
        //    new int []{3, 0, 3, 2, 1, 4, 0, 3, 3, 1, 2, 2, 0, 4, 2 }
        //};

        //preset[15] = new int [][]{
        //    new int []{2, 0, 1, 4, 4, 3, 1, 4, 2, 0, 4, 0, 4, 0, 1},
        //    new int []{3, 3, 0, 2, 1, 1, 1, 4, 2, 4, 3, 4, 2, 1, 0},
        //    new int []{4, 1, 4, 4, 1, 2, 1, 1, 1, 2, 3, 1, 0, 3, 3},
        //    new int []{4, 4, 2, 3, 3, 0, 2, 0, 3, 2, 1, 4, 4, 1, 4},
        //    new int []{1, 4, 1, 3, 3, 3, 1, 0, 2, 2, 2, 2, 2, 3, 0},
        //    new int []{0, 4, 2, 3, 0, 3, 1, 0, 1, 1, 3, 1, 3, 2, 1},
        //    new int []{2, 0, 2, 4, 1, 1, 2, 1, 3, 1, 1, 1, 2, 2, 2},
        //    new int []{1, 3, 3, 3, 1, 1, 0, 0, 3, 3, 0, 2, 1, 1, 1},
        //    new int []{0, 0, 0, 4, 4, 1, 3, 2, 4, 1, 0, 0, 3, 3, 0},
        //    new int []{4, 3, 2, 3, 1, 3, 3, 3, 4, 3, 1, 2, 2, 1, 1},
        //    new int []{1, 1, 1, 1, 2, 0, 2, 1, 4, 1, 3, 1, 1, 1, 2},
        //    new int []{0, 1, 2, 1, 0, 4, 0, 2, 3, 1, 0, 0, 0, 1, 0},
        //    new int []{0, 1, 1, 4, 3, 3, 4, 4, 0, 0, 1, 0, 1, 2, 4},
        //    new int []{3, 2, 2, 1, 1, 4, 0, 2, 1, 0, 1, 0, 4, 4, 4},
        //    new int []{4, 0, 1, 1, 2, 2, 4, 3, 4, 1, 2, 4, 4, 3, 4 }
        //};

        //preset[16] = new int [][]{
        //    new int []{0, 2, 2, 2, 4, 1, 2, 0, 4, 0, 2, 3, 0, 2, 2},
        //    new int []{4, 4, 4, 2, 2, 2, 1, 2, 3, 2, 3, 0, 0, 2, 1},
        //    new int []{3, 2, 0, 1, 2, 3, 2, 4, 3, 1, 0, 4, 2, 0, 2},
        //    new int []{2, 1, 2, 0, 0, 2, 2, 3, 4, 3, 2, 2, 2, 1, 3},
        //    new int []{0, 2, 0, 3, 2, 0, 2, 1, 2, 2, 2, 3, 3, 0, 2},
        //    new int []{3, 1, 0, 4, 3, 0, 1, 1, 0, 3, 0, 0, 2, 3, 4},
        //    new int []{0, 3, 4, 1, 3, 4, 3, 1, 1, 3, 3, 1, 2, 1, 3},
        //    new int []{4, 2, 3, 1, 1, 0, 3, 3, 4, 4, 1, 1, 4, 4, 3},
        //    new int []{3, 0, 4, 1, 1, 1, 3, 3, 1, 4, 1, 1, 4, 4, 2},
        //    new int []{2, 1, 3, 0, 2, 2, 4, 2, 4, 2, 1, 1, 2, 2, 0},
        //    new int []{0, 1, 3, 2, 4, 4, 0, 0, 0, 4, 2, 2, 4, 2, 2},
        //    new int []{3, 0, 1, 2, 4, 1, 0, 3, 3, 1, 0, 4, 0, 2, 2},
        //    new int []{4, 2, 1, 4, 2, 2, 2, 2, 0, 2, 1, 0, 4, 3, 0},
        //    new int []{0, 4, 3, 2, 0, 2, 3, 2, 4, 2, 1, 1, 1, 3, 4},
        //    new int []{4, 2, 4, 4, 0, 2, 0, 1, 3, 4, 2, 4, 2, 3, 1 }
        //};

        //preset[17] = new int [][]{
        //    new int []{0, 2, 2, 4, 4, 3, 3, 3, 3, 0, 4, 3, 0, 2, 3},
        //    new int []{4, 1, 4, 4, 4, 1, 3, 1, 4, 1, 0, 3, 0, 2, 1},
        //    new int []{0, 4, 1, 3, 0, 3, 1, 3, 3, 2, 4, 0, 4, 3, 2},
        //    new int []{1, 2, 3, 3, 4, 2, 1, 0, 2, 3, 3, 3, 2, 3, 4},
        //    new int []{0, 4, 1, 4, 1, 1, 2, 3, 0, 2, 1, 3, 1, 0, 2},
        //    new int []{3, 1, 3, 4, 1, 3, 3, 1, 4, 3, 3, 2, 4, 4, 0},
        //    new int []{0, 2, 4, 4, 1, 0, 0, 3, 2, 3, 2, 3, 3, 3, 2},
        //    new int []{0, 1, 4, 3, 3, 1, 2, 1, 3, 2, 3, 1, 2, 0, 2},
        //    new int []{0, 2, 0, 2, 3, 1, 3, 4, 1, 1, 0, 2, 1, 4, 1},
        //    new int []{0, 3, 4, 0, 0, 2, 3, 2, 4, 3, 3, 0, 0, 0, 3},
        //    new int []{2, 4, 0, 2, 2, 0, 3, 1, 0, 2, 3, 2, 3, 2, 3},
        //    new int []{4, 3, 1, 1, 4, 3, 1, 1, 3, 1, 3, 0, 4, 1, 3},
        //    new int []{4, 2, 1, 1, 3, 3, 0, 3, 0, 4, 0, 3, 4, 3, 0},
        //    new int []{1, 2, 4, 4, 2, 4, 3, 4, 1, 4, 3, 0, 0, 0, 2},
        //    new int []{4, 3, 3, 2, 3, 3, 0, 0, 1, 1, 2, 3, 3, 4, 2 }
        //};

        //preset[18] = new int [][]{
        //    new int []{4, 4, 1, 4, 1, 1, 2, 4, 0, 3, 3, 0, 1, 2, 0},
        //    new int []{4, 1, 1, 4, 3, 1, 1, 2, 2, 3, 0, 2, 4, 0, 3},
        //    new int []{3, 2, 3, 4, 0, 1, 4, 0, 2, 4, 4, 1, 2, 3, 0},
        //    new int []{0, 4, 3, 4, 0, 2, 4, 0, 4, 4, 4, 1, 4, 3, 4},
        //    new int []{4, 0, 1, 4, 2, 0, 1, 2, 4, 3, 0, 1, 4, 4, 3},
        //    new int []{3, 1, 4, 2, 2, 1, 2, 1, 2, 2, 4, 2, 1, 2, 2},
        //    new int []{2, 4, 2, 0, 4, 0, 3, 4, 3, 0, 2, 3, 3, 3, 0},
        //    new int []{3, 4, 3, 4, 0, 0, 0, 1, 1, 4, 1, 2, 1, 3, 3},
        //    new int []{4, 4, 4, 1, 2, 4, 2, 4, 0, 1, 4, 1, 4, 4, 3},
        //    new int []{0, 4, 2, 2, 4, 0, 3, 1, 3, 2, 4, 1, 4, 4, 4},
        //    new int []{0, 0, 2, 1, 4, 0, 2, 4, 3, 4, 0, 0, 4, 3, 0},
        //    new int []{4, 2, 1, 0, 2, 2, 4, 2, 2, 2, 3, 3, 1, 0, 0},
        //    new int []{4, 1, 3, 3, 4, 3, 1, 3, 2, 1, 1, 3, 1, 4, 2},
        //    new int []{1, 1, 4, 0, 4, 3, 3, 2, 0, 2, 4, 3, 1, 4, 0},
        //    new int []{3, 3, 1, 4, 1, 4, 0, 4, 0, 4, 3, 0, 4, 4, 0 }
        //};

        //preset[19] = new int [][]{
        //    new int []{3, 0, 1, 3, 3, 0, 0, 1, 0, 0, 2, 4, 0, 0, 1},
        //    new int []{1, 2, 2, 3, 2, 2, 0, 4, 0, 2, 3, 2, 2, 2, 1},
        //    new int []{3, 1, 0, 0, 0, 0, 4, 4, 1, 3, 1, 3, 2, 0, 4},
        //    new int []{0, 1, 0, 2, 0, 3, 4, 3, 2, 3, 0, 2, 0, 3, 4},
        //    new int []{2, 3, 2, 2, 0, 3, 3, 0, 0, 3, 0, 3, 4, 1, 1},
        //    new int []{0, 3, 3, 2, 0, 4, 1, 2, 4, 1, 2, 4, 4, 1, 0},
        //    new int []{3, 2, 4, 0, 4, 1, 4, 3, 2, 1, 1, 4, 0, 0, 2},
        //    new int []{1, 4, 1, 3, 0, 4, 0, 3, 2, 3, 2, 0, 0, 0, 1},
        //    new int []{0, 0, 0, 1, 4, 2, 1, 0, 4, 4, 4, 3, 1, 0, 4},
        //    new int []{3, 3, 3, 1, 0, 3, 1, 2, 0, 2, 4, 3, 4, 1, 1},
        //    new int []{1, 1, 1, 3, 0, 2, 2, 3, 0, 4, 3, 4, 4, 1, 1},
        //    new int []{0, 2, 0, 0, 2, 0, 0, 1, 3, 0, 2, 3, 0, 2, 4},
        //    new int []{4, 3, 3, 2, 4, 0, 0, 0, 4, 3, 1, 0, 4, 1, 2},
        //    new int []{2, 2, 3, 2, 0, 4, 2, 0, 0, 4, 1, 4, 4, 0, 1},
        //    new int []{3, 4, 1, 4, 4, 0, 0, 0, 0, 1, 0, 2, 1, 0, 0 }
        //};
        
        //int totalScore = 0;
        //for(int i = 0; i < preset.Length; i++)
        //{
        //    //ISimulationStrategy simulationStrategy = new SamegameRandomStrategy();
        //    ISimulationStrategy simulationStrategy = new SamegameTabuColorRandomStrategy(preset[i],null);
        //    SamegameGameState s = new SamegameGameState(preset[i], null, simulationStrategy);
        //    //Debug.WriteLine(s.PrettyPrint());

        //    IGameMove move;
        //    ISimulationStrategy player = new SamegameMCTSStrategy(null,1000,0.1);
        //    double startTime = DateTime.Now.TimeOfDay.TotalSeconds;//used to keep track of the time needed to solve each level
        //    while (!s.isTerminal())
        //    {
                
        //        move = player.selectMove(s);
        //        //Debug.WriteLine(move);
        //        s.DoMove(move);
        //        //Debug.WriteLine(s.GetScore(1));
        //        //Debug.WriteLine(s.PrettyPrint());
        //    }
        //    Debug.WriteLine("Final configuration level "+i+": \n"+ s.PrettyPrint());
        //    Debug.WriteLine("Score level "+i+": " + s.GetScore(0));
        //    scores[i] = s.GetScore(0);
        //    totalScore += scores[i];
        //    double elapsedTime = DateTime.Now.TimeOfDay.TotalSeconds - startTime;
        //    Debug.WriteLine("Time elapsed level "+i+": " + Math.Truncate(elapsedTime/60) +" minutes and "+Math.Truncate(elapsedTime%60)+" seconds\n");
        //}

        //Console.WriteLine("TotalScore: " + totalScore);
    }

	public static void TestStrategies(ITicTacToeSimulationStrategy[] strategies) {

	}

	/// <summary>
	/// Tournament among all the tic-tac-toe algorithms
	/// </summary>
	static void TicTacToeAllMatches(int no_matches=100) {
		for (int s1 = 0; s1 < TicTacToeStrategies.Length; s1++) {
			for (int s2 = s1; s2 < TicTacToeStrategies.Length; s2++) {
				TicTacToeEval (TicTacToeStrategies [s1], TicTacToeStrategies [s2], no_matches);
			}	
		}
	}


	/// <summary>
	/// Evaluate the complexity of an artificial intelligence algorithm using vanilla MCTS
	/// </summary>
	/// <param name="p">P.</param>
	public static void TicTacToeMCTSEval(Tuple<string,int> p, int min_backups, int max_backups = 100)
	{
	    bool done = false;
		int current_backups = min_backups;
	    int prev_backups = 0;

        /// if the max number of backups is not specified, double the number of backups until it is found 
	    if (max_backups == 0)
	    {
	        bool foundUpperLimit = false;
            current_backups = min_backups;

            do
            {
	            TwoPlayersGameStats stats = TicTacToeEval(new Tuple<string, int>("MCTS", current_backups), p);
	            if (stats.Quality < cQualityThreshold && stats.Wins(1) < stats.Wins(2))
	            {
	                current_backups = 2*current_backups;
	            } else {
	                foundUpperLimit = true;
	            }
	        } while (!foundUpperLimit);

	        max_backups = (int) (current_backups * (1f+cUpperLimitIncrease));
	    }

        Console.WriteLine("==\tMaxBackups\t"+max_backups);

        /// binary search for the smallest number of backups to match the target AI
	    int lowerLimit = min_backups;
	    int upperLimit = max_backups;

	    do
	    {
			string str_output = string.Empty;

	        current_backups = (min_backups + max_backups)/2;

			str_output = ">>\t" + min_backups + "\t" + max_backups + "\t" + current_backups;

            TwoPlayersGameStats stats = TicTacToeEval(new Tuple<string, int>("MCTS", current_backups), p);
            //Console.WriteLine("current backups " + current_backups + "=>" + stats.Quality);

			str_output = str_output + "\t" + stats.Wins(1,1);
			str_output = str_output + "\t" + stats.Wins(1,2);
			str_output = str_output + "\t" + stats.Wins(2,1);
			str_output = str_output + "\t" + stats.Wins(2,2);
			str_output = str_output + "\t" + string.Format("{0:F3}", stats.Quality);

			Console.WriteLine(str_output);

            if (stats.Quality > cQualityThreshold || 
                lowerLimit>=upperLimit || 
                Math.Abs(upperLimit-lowerLimit)< cMinBackUpDifference)
	        {
	            done = true;
	        }
	        else
	        {
	            if (stats.Wins(1) < stats.Wins(2))
	            {
	                lowerLimit = current_backups;
	            }
	            else
	            {
	                upperLimit = current_backups;
	            }
	        }
	    } while (!done && current_backups < max_backups);

	    if (current_backups >= max_backups)
	    {
	        Console.WriteLine("LIMITE TROPPO BASSO");
	    }
	    else
	    {
	        Console.WriteLine("Complexity = "+current_backups);
	    }

	}


	public static void Forza4MCTSEval(Tuple<string,int> p, int min_backups, int max_backups = 100)
	{
		bool done = false;
		int current_backups = min_backups;
		int prev_backups = 0;

		/// if the max number of backups is not specified, double the number of backups until it is found 
		if (max_backups == 0)
		{
			bool foundUpperLimit = false;
			current_backups = min_backups;

			do
			{
				Console.WriteLine(">> CURRENT BACKUPS = "+current_backups);
				TwoPlayersGameStats stats = Forza4Eval(new Tuple<string, int>("MCTS", current_backups), p);
				if (stats.Quality < cQualityThreshold && stats.Wins(1) < stats.Wins(2))
				{
					current_backups = 2*current_backups;
				} else {
					foundUpperLimit = true;
				}
			} while (!foundUpperLimit);

			max_backups = (int) (current_backups * (1f+cUpperLimitIncrease));
		}

		Console.WriteLine("MAX BACKUPS = "+max_backups);

		/// binary search for the smallest number of backups to match the target AI
		int lowerLimit = min_backups;
		int upperLimit = max_backups;

		do
		{
			current_backups = (min_backups + max_backups)/2;

			TwoPlayersGameStats stats = Forza4Eval(new Tuple<string, int>("MCTS", current_backups), p);
			Console.WriteLine("current backups " + current_backups + "=>" + stats.Quality);

			if (stats.Quality > cQualityThreshold || 
				lowerLimit>=upperLimit || 
				Math.Abs(upperLimit-lowerLimit)< cMinBackUpDifference)
			{
				done = true;
			}
			else
			{
				if (stats.Wins(1) < stats.Wins(2))
				{
					lowerLimit = current_backups;
				}
				else
				{
					upperLimit = current_backups;
				}

				Console.Write("BACKUPS => ["+prev_backups+ " "+current_backups+"]");
			}
		} while (!done && current_backups < max_backups);

		if (current_backups >= max_backups)
		{
			Console.WriteLine("LIMITE TROPPO BASSO");
		}
		else
		{
			Console.WriteLine("Complexity = "+current_backups);
		}

	}

	static TwoPlayersGameStats TicTacToeEval(Tuple<string,int> p1, Tuple<string,int> p2, int no_matches=100) {
		
		TicTacToeGameState state = new TicTacToeGameState ();
		TwoPlayersGameStats stats = new TwoPlayersGameStats ();

		ITicTacToeSimulationStrategy player1;
		ITicTacToeSimulationStrategy player2;


		/// set up experiment parameters and labels
		string label1 = p1.Item1;
		string label2 = p2.Item1;

		if (p1.Item2 < 0)
			player1 = TicTacToePlayer (p1.Item1);
		else {
			player1 = TicTacToePlayer (p1.Item1, p1.Item2);
			label1 = label1 + "(" + p1.Item2 + ")";
		}

		if (p2.Item2 < 0)
			player2 = TicTacToePlayer (p2.Item1);
		else {
			player2 = TicTacToePlayer (p2.Item1, p2.Item2);
			label2 = label2 + "(" + p2.Item2 + ")";
		}

		//Console.WriteLine (label1 + " vs " + label2 + "\n");

		int move_counter = 0;

		IGameMove current_move; 

		stats.Reset();

		for (int p = 0; p < no_matches; p++) {

			int start_player = ((p % 2 == 0) ? 1 : 2);
			state.Restart (start_player);

			move_counter = 0;

			//// ERRORE NELLA SELEZIONE DEL GIOCATORE! :) 

			while (!state.EndState ()) {

				// when player1 starts it plays even move (0 -> the first, 2 -> the third, etc.)
				if (start_player == 1) {
					if (move_counter % 2 == 0) {
						current_move = player1.selectMove (state);
					} else {
						current_move = player2.selectMove (state);
					}

					// when player2 starts it plays even move (0 -> the first, 2 -> the third, etc.)
				} else {
					if (move_counter % 2 == 0) {
						current_move = player2.selectMove (state);
					} else {
						current_move = player1.selectMove (state);
					}
				}

				move_counter = move_counter + 1;

				state.DoMove (current_move);
			}
			//Console.WriteLine (p + "\tWINNER\t" + state.Winner() + "\t" + stats.PrettyPrintRates());
			if (state.Winner() == 1 || state.Winner() == 2)
				stats.PlayerWon (state.Winner(), start_player);
			else
				stats.PlayersTied ();
		}

//		Console.WriteLine (player1.getFriendlyName() + " vs " + player2.getFriendlyName()+"\n"); 
//		Console.WriteLine ("P1WINS\t" + stats.Wins(1) + "\tP2WINS\t" +  + stats.Wins(2) + "\tTIES\t" + stats.Ties()+"\n\n");
//		Console.WriteLine ("\tP1-1ST\tP2-1S\nP1\t" + stats.Wins (1, 1) + "\t" + stats.Wins (1, 2) + "\nP2\t" + stats.Wins (2, 1) + "\t" + stats.Wins (2, 2) + "\n");

		return stats;
	}



	static TwoPlayersGameStats Forza4Eval(Tuple<string,int> p1, Tuple<string,int> p2, int no_matches=100) {

		Forza4GameState state = new Forza4GameState ();
		TwoPlayersGameStats stats = new TwoPlayersGameStats ();

		IForza4SimulationStrategy player1;
		IForza4SimulationStrategy player2;


		/// set up experiment parameters and labels
		string label1 = p1.Item1;
		string label2 = p2.Item1;

		if (p1.Item2 < 0)
			player1 = Forza4Player (p1.Item1);
		else {
			player1 = Forza4Player (p1.Item1, p1.Item2);
			label1 = label1 + "(" + p1.Item2 + ")";
		}

		if (p2.Item2 < 0)
			player2 = Forza4Player (p2.Item1);
		else {
			player2 = Forza4Player (p2.Item1, p2.Item2);
			label2 = label2 + "(" + p2.Item2 + ")";
		}


		//Console.WriteLine (label1 + " vs " + label2 + "\n");

		int move_counter = 0;

		IGameMove current_move; 

		stats.Reset();

		for (int p = 0; p < no_matches; p++) {

			int start_player = ((p % 2 == 0) ? 1 : 2);
			state.Restart (start_player);

			move_counter = 0;

			//// ERRORE NELLA SELEZIONE DEL GIOCATORE! :) 

			while (!state.EndState ()) {

				// when player1 starts it plays even move (0 -> the first, 2 -> the third, etc.)
				if (start_player == 1) {
					if (move_counter % 2 == 0) {
						current_move = player1.selectMove (state);
					} else {
						current_move = player2.selectMove (state);
					}

					// when player2 starts it plays even move (0 -> the first, 2 -> the third, etc.)
				} else {
					if (move_counter % 2 == 0) {
						current_move = player2.selectMove (state);
					} else {
						current_move = player1.selectMove (state);
					}
				}

				move_counter = move_counter + 1;

				state.DoMove (current_move);
			}
			//Console.WriteLine (p + "\tWINNER\t" + state.Winner() + "\t" + stats.PrettyPrintRates());
			if (state.Winner() == 1 || state.Winner() == 2)
				stats.PlayerWon (state.Winner(), start_player);
			else
				stats.PlayersTied ();
		}

		Console.WriteLine (player1.getFriendlyName() + " vs " + player2.getFriendlyName()+"\n"); 
		Console.WriteLine ("P1WINS\t" + stats.Wins(1) + "\tP2WINS\t" +  + stats.Wins(2) + "\tTIES\t" + stats.Ties()+"\n\n");
		Console.WriteLine ("\tP1-1ST\tP2-1S\nP1\t" + stats.Wins (1, 1) + "\t" + stats.Wins (1, 2) + "\nP2\t" + stats.Wins (2, 1) + "\t" + stats.Wins (2, 2) + "\n");

		return stats;
	}

	/// <summary>
	/// OLD VERSIONS AND FUNCTIONS TO BE TRASHED
	/// </summary>

	#region OldVersionsToBeTrashed
//	static void Match() {
//		// quando lo crea dovrebbe resettarlo
//		TicTacToeGameState state = new TicTacToeGameState ();
//
//		ITicTacToeSimulationStrategy player1 = TicTacToePlayer ("Random");
//		ITicTacToeSimulationStrategy player2 = TicTacToePlayer ("MCTS",1500);
//
//		int move_counter = 0;
//
//		IGameMove current_move; 
//
//		TwoPlayersGameStats.Reset();
//
//		for (int p = 0; p < 100; p++) {
//
//			int start_player = ((p % 2 == 0) ? 1 : 2);
//			state.Restart (start_player);
//
//			move_counter = 0;
//
//			//// ERRORE NELLA SELEZIONE DEL GIOCATORE! :) 
//
//			while (!state.EndState ()) {
//
//				// when player1 starts it plays even move (0 -> the first, 2 -> the third, etc.)
//				if (start_player == 1) {
//					if (move_counter % 2 == 0) {
//						current_move = player1.selectMove (state);
//					} else {
//						current_move = player2.selectMove (state);
//					}
//
//					// when player2 starts it plays even move (0 -> the first, 2 -> the third, etc.)
//				} else {
//					if (move_counter % 2 == 0) {
//						current_move = player2.selectMove (state);
//					} else {
//						current_move = player1.selectMove (state);
//					}
//				}
//
//				move_counter = move_counter + 1;
//
//				state.DoMove (current_move);
//			}
//			Console.WriteLine (p + "\tWINNER\t" + state.Winner());
//			if (state.Winner() == 1 || state.Winner() == 2)
//				TwoPlayersGameStats.PlayerWon (state.Winner(), start_player);
//			else
//				TwoPlayersGameStats.PlayersTied ();
//		}
//
//		Console.WriteLine (player1.getFriendlyName() + " vs " + player2.getFriendlyName()+"\n"); 
//		Console.WriteLine ("P1WINS\t" + TwoPlayersGameStats.Wins(1) + "\tP2WINS\t" +  + TwoPlayersGameStats.Wins(2) + "\tTIES\t" + TwoPlayersGameStats.Ties()+"\n\n");
//		Console.WriteLine ("\tP1-1ST\tP2-1S\nP1\t" + TwoPlayersGameStats.Wins (1, 1) + "\t" + TwoPlayersGameStats.Wins (1, 2) + "\nP2\t" + TwoPlayersGameStats.Wins (2, 1) + "\t" + TwoPlayersGameStats.Wins (2, 2) + "\n");
//	}
	#endregion

	static ISimulationStrategy GetPlayer(string game, string name, int n=0) {
		switch (game) {
		case "TicTacToe":
			return TicTacToePlayer (name, n);
		case "Forza4":
			return Forza4Player (name, n);
		default:
			return Forza4Player (name, n);
		}
	}

	#region PLAYERS

	static IForza4SimulationStrategy Forza4Player(string name, int n=0) {
		
		switch (name) {

		case "Random":
			return new Forza4RandomStrategy ();

		case "MiniMax":
			if (n>0)
				return new Forza4MiniMaxStrategy (n);
			else 
				return new Forza4MiniMaxStrategy ();
			
		case "Schmidt":
			return new Forza4SchmidtStrategy ();

		case "NFriendsFloyd":
			return new Forza4NFriendsFloydStrategy ();

		case "MCTS":
			if (n>0)
				return new Forza4MCTSStrategy (n);
			else 
				return new Forza4MCTSStrategy (1000);
		default:
			return new Forza4RandomStrategy ();
		}
	}

	static ITicTacToeSimulationStrategy TicTacToePlayer(string name, int n=0) {
		switch (name) {
		case "Random":
			return new TicTacToeRandomStrategy ();
		case "MiniMax":
			return new TicTacToeMiniMaxStrategy (n);
		case "LookUp":
			return new TicTacToeLookUpStrategy ();
		case "MCTS":
			if (n>0)
				return new TicTacToeMCTSStrategy (n);
			else 
				return new TicTacToeMCTSStrategy (1000);
		default:
			return new TicTacToeRandomStrategy ();
		}
	}
	#endregion

	#region REDIRECT_OUTPUT
	public static void RedirectOutputToFile(string fn) {
		FileStream fs = new FileStream(fn, FileMode.Create);
		TextWriter tmp = Console.Out;
		outputStreamForConsole = new StreamWriter(fs);
		Console.SetOut(outputStreamForConsole);
		outputToFile = true;
	}

	public static void CloseOutputToFile() {
		outputStreamForConsole.Close ();
		outputToFile = false;
	}
	#endregion



}
