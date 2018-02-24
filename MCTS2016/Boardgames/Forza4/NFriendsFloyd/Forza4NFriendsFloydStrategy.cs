using System;
using System.Collections.Generic;
using System.Threading;
using System.Linq;
using Common.Abstract;

namespace BoardGames
{
    public class Forza4NFriendsFloydStrategy : IForza4SimulationStrategy
    {
        private const int THE_NEED = 4;

        public IGameMove selectMove(IGameState gameState)
        {
            int[,] board = Forza4GameState.ParseBoard(gameState, 6, 7);

            return MakeMove(board, gameState.currentPlayer);
        }

        public IGameMove MakeMove(int[,] felder, int me)
        {
            MyField f = new MyField(felder, me);
            
            List<FieldInfo> bestFields = new List<FieldInfo>();
            foreach (FieldInfo fi in f.FieldInfos)
            {
                if (bestFields.Count == 0)
                {
                    bestFields.Add(fi);
                    continue;
                }
                
                if (bestFields.Count > 0 && fi.Weight >= bestFields[0].Weight)
                {
                    if (fi.Weight > bestFields[0].Weight)
                        bestFields.Clear();
                    
                    bestFields.Add(fi);
                }
            }
            
            int oneBest = 0;
            if (bestFields.Count > 1)
            {
                oneBest = (new Random()).Next(0, bestFields.Count);
            }
            
            return (IGameMove)(bestFields[oneBest].Y);
        }

        public string getTypeName()
        {
            return this.GetType().Name;
        }

        public string getFriendlyName()
        {
            return "NFriendsFloyd";
        }
    }
}

