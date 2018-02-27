# SP-MCTS

**EXECUTION**  
**parameters**: game method levelsPath  

**game values**:  
-sokoban  
-samegame  

**method format**  
-for **sokoban MCTS**: mcts:iterations:const_C:const_D:rewardType:epsilon:terminateOnSolution:seed  
example: *`sokoban mcts:10000:1\:100\:InverseBM:0.2:true:1 ./Levels/sokoban.txt`*  

-for **sokoban IDA\***: ida:maxDepth:rewardType  
example: *`sokoban ida:40:PositiveBM ./Levels/sokoban.txt`*  

-for **samegame MCTS**: mcts:iterations:const_C:const_D:restarts:seed  
example: *`samegame mcts:10000:1:100\:50:1 ./Levels/samegame.txt`*  

-for **samegame IDA\***: to be implemented  

**Parameters**:  

**const_C**: UCT constant used for sokoban  

**const_D**: SP-MCTS constant  

**iterations**: maximum number of iterations when using MCTS  

**maxDepth**: maximum cost depth when using IDA*  

**restarts**: number of randomized restarts  

**seed**: seed used for rng  

**rewardType**: sokoban reward type ( R0 = 1 if solution found, 0 otherwise;  InverseBM = 1/BM;  NegativeBM = -BM;  LogBM = -1/Log(BM) )  

**terminateOnSolution**: boolan used to specify whether MCTS stops as soon as it finds a solution or if it continues execution to find a better solution  

**levelfile**: path of the file containing the levels  