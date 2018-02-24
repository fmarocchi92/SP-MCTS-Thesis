# SP-MCTS
To execute download the Release folder and run the sokoban.bat file

bat file content in order of appearance:

**executable**

**type of test**: sokobanTuning for testing different parameters in sokoban, sokoban for solving a set of sokoban levels, samegame for solving a set of samegame levels, idastar for solving a set of sokoban levels using IDA*

**const_C**: UCT constant used for sokoban

**const_D**: SP-MCTS constant (used only in samegame)

**maxIterations**: maximum number of iterations when using MCTS, maximum cost depth when using IDA*

**restarts**: number of restarts in samegame, number of experiment repetitions in sokobanTuning

**maxThreads**: maximum number of threads used (limited to those available in the machine)

**seed**: seed used for rng

**abstractSokoban**: boolean used to specify whether the sokoban representation is at the push level (true) or at the move level (false)

**rewardType**: sokoban reward type (R0 = 1 if solution found, 0 otherwise; InverseBM = 1/BM; NegativeBM = -BM; LogBM = -1/Log(BM)

**terminateOnSolution**: boolan used to specify whether MCTS stops as soon as it finds a solution

**logfile**: path of the logfile; the file will be created if it doesn't exist

**levelfile**: path of the file containing the levels

**c_values file**: path of the csv file containing UCT constant values for sokobanTuning

**epsilon_values file**: path of the csv file containing epsilon values for sokobanTuning
