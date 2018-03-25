::executable game method:iterations:const_C:const_D:rewardType:epsilon:terminateOnSolution:seed levelfile
::FABIO STOP
SET ucb1=false
SET rave=false
SET rewardType=InverseBM
SET tunnelMacro=true
SET normalizedPositions=true
SET epsilon=1
SET iterations=1000
SET avoidCycles=true
SET nodeElimination=true
SET seed=1
SET level=./Levels/test.txt

.\MCTS2016.exe sokoban:%normalizedPositions%:%tunnelMacro%:false:false mcts:%iterations%:0.001:100:%rewardType%:%epsilon%:true:1:%ucb1%:%rave%:10:false:5000:%avoidCycles%:%nodeElimination% %level%
.\MCTS2016.exe sokoban:%normalizedPositions%:%tunnelMacro%:false:false mcts:%iterations%:0.005:100:%rewardType%:%epsilon%:true:1:%ucb1%:%rave%:10:false:5000:%avoidCycles%:%nodeElimination% %level%
.\MCTS2016.exe sokoban:%normalizedPositions%:%tunnelMacro%:false:false mcts:%iterations%:0.02:100:%rewardType%:%epsilon%:true:1:%ucb1%:%rave%:10:false:5000:%avoidCycles%:%nodeElimination% %level%