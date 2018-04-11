::executable game method:iterations:const_C:const_D:rewardType:epsilon:terminateOnSolution:seed levelfile
::FABIO STOP
SET ucb1=false
SET rave=false
SET rewardType=NegativeBM
SET tunnelMacro=true
SET normalizedPositions=true
SET epsilon=0.2
SET iterations=1000
SET avoidCycles=true
SET nodeElimination=true
SET seed=1
SET level=./Levels/microban.txt

.\UCB1Tuned.exe sokoban:true:true:false:false mcts:5000:6:100:NegativeBM:true:1:false:false:5:false:1:true:true:EpsilonGreedy:-1 ./microban.txt