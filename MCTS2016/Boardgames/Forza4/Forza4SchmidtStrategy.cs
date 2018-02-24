using System;
using System.Collections.Generic;
using Common.Abstract;

namespace BoardGames
{

    public class Forza4SchmidtStrategy : IForza4SimulationStrategy
    {
        public IGameMove selectMove(IGameState gameState)
        {
            int[,] board = Forza4GameState.ParseBoard(gameState, 6, 7);

            return MakeMove(board, gameState.currentPlayer);
        }

        public IGameMove MakeMove(int[,] sockets, int currentPlayer)
        {
            int opponentPlayer = 3 - currentPlayer;
            int slot = 0;
            bool flag = false;
            for (int colIndex = 0; colIndex < sockets.GetLength(1); colIndex++)
            {
                for (int rowIndex = sockets.GetLength(0) - 1; rowIndex >= 0; rowIndex--)
                {
                    if (sockets[rowIndex, colIndex] == 0)
                    {
                        slot = colIndex;
                        flag = true;
                    }
                    if (flag)
                    {
                        break;
                    }
                }
                if (flag)
                {
                    break;
                }
            }
            for (int colIndex = 0; colIndex < sockets.GetLength(1); colIndex++)
            {
                for (int rowIndex = sockets.GetLength(0) - 1; rowIndex >= 0; rowIndex--)
                {
                    slot = Horizontal(sockets, opponentPlayer, currentPlayer, slot, rowIndex, colIndex);
                }
            }
            for (int colIndex = 0; colIndex < sockets.GetLength(1); colIndex++)
            {
                for (int rowIndex = sockets.GetLength(0) - 1; rowIndex >= 0; rowIndex--)
                {
                    slot = Diagonal(sockets, opponentPlayer, slot, rowIndex, colIndex);
                }
            }
            for (int rowIndex = sockets.GetLength(0) - 1; rowIndex >= 0; rowIndex--)
            {
                for (int colIndex = 0; colIndex < sockets.GetLength(1); colIndex++)
                {
                    slot = Vertikal(sockets, opponentPlayer, currentPlayer, slot, rowIndex, colIndex);
                }
            }
            return (IGameMove)slot;
        }

        private static int Diagonal(int[,] sockets, int opponentPlayer, int slot, int rowIndex, int colIndex)
        {
            if ((((rowIndex - 3) < sockets.GetLength(0) && (rowIndex - 3) > 0) && ((colIndex + 3) < sockets.GetLength(1))) && (sockets[rowIndex, colIndex] == opponentPlayer))
            {
                if (sockets[rowIndex - 1, colIndex + 1] == opponentPlayer)
                {
                    if (sockets[rowIndex - 2, colIndex + 2] == opponentPlayer)
                    {
                        if ((sockets[rowIndex - 3, colIndex + 3] == 0) && (sockets[rowIndex - 3, colIndex + 2] != 0))
                        {
                            slot = colIndex + 3;
                        }
                    }
                    else if (((sockets[rowIndex - 2, colIndex + 2] == 0) && (sockets[rowIndex - 3, colIndex + 3] == opponentPlayer)) && (sockets[rowIndex - 2, colIndex + 1] != 0))
                    {
                        slot = colIndex + 2;
                    }
                }
                else if (((sockets[rowIndex - 1, colIndex + 1] == 0) && (sockets[rowIndex - 2, colIndex + 2] == opponentPlayer)) && ((sockets[rowIndex - 3, colIndex + 3] == opponentPlayer) && (sockets[rowIndex - 1, colIndex] != 0)))
                {
                    slot = colIndex + 1;
                }
            }
            if (((((rowIndex - 3) < sockets.GetLength(0) && (rowIndex - 3) > 0) && ((colIndex + 3) < sockets.GetLength(1))) && ((sockets[rowIndex, colIndex] == 0) && (sockets[rowIndex - 1, colIndex + 1] == opponentPlayer))) && ((sockets[rowIndex - 2, colIndex + 2] == opponentPlayer) && (sockets[rowIndex - 3, colIndex + 3] == opponentPlayer)))
            {
                slot = colIndex;
            }
            if ((((rowIndex + 3) >= 0 && (rowIndex + 3) < sockets.GetLength(0)) && ((colIndex + 3) < sockets.GetLength(1))) && (sockets[rowIndex, colIndex] == opponentPlayer))
            {
                if (sockets[rowIndex + 1, colIndex + 1] == opponentPlayer)
                {
                    if (sockets[rowIndex + 2, colIndex + 2] == opponentPlayer)
                    {
                        if ((sockets[rowIndex + 3, colIndex + 3] == 0) && (sockets[rowIndex + 3, colIndex + 2] != 0))
                        {
                            slot = colIndex + 3;
                        }
                    }
                    else if (((sockets[rowIndex + 2, colIndex + 2] == 0) && (sockets[rowIndex + 3, colIndex + 3] == opponentPlayer)) && (sockets[rowIndex + 2, colIndex + 1] != 0))
                    {
                        slot = colIndex + 2;
                    }
                }
                else if (((sockets[rowIndex + 1, colIndex + 1] == 0) && (sockets[rowIndex + 2, colIndex + 2] == opponentPlayer)) && ((sockets[rowIndex + 3, colIndex + 3] == opponentPlayer) && (sockets[rowIndex + 1, colIndex] != 0)))
                {
                    slot = colIndex + 1;
                }
            }
            if (((((rowIndex + 3) >= 0 && (rowIndex + 3) < sockets.GetLength(0)) && ((colIndex + 3) < sockets.GetLength(1))) && ((sockets[rowIndex, colIndex] == 0) && (sockets[rowIndex + 1, colIndex + 1] == opponentPlayer))) && ((sockets[rowIndex + 2, colIndex + 2] == opponentPlayer) && (sockets[rowIndex + 3, colIndex + 3] == opponentPlayer)))
            {
                if ((colIndex == 0) && (sockets[rowIndex, colIndex] != 0))
                {
                    slot = colIndex;
                    return slot;
                }
                if ((colIndex > 0) && (sockets[rowIndex, colIndex - 1] != 0))
                {
                    slot = colIndex;
                }
            }
            return slot;
        }

        private static int Horizontal(int[,] sockets, int opponentPlayer, int currentPlayer, int slot, int rowIndex, int colIndex)
        {
            if (sockets[rowIndex, colIndex] == opponentPlayer)
            {
                if (((rowIndex - 1) < sockets.GetLength(0) && (rowIndex - 1) > 0) && (sockets[rowIndex - 1, colIndex] == opponentPlayer))
                {
                    if ((((rowIndex - 3) < sockets.GetLength(0) && (rowIndex - 3) > 0) && (sockets[rowIndex - 2, colIndex] == opponentPlayer)) && (sockets[rowIndex - 3, colIndex] == 0))
                    {
                        if ((colIndex > 0) && (sockets[rowIndex - 3, colIndex - 1] != 0))
                        {
                            slot = colIndex + 3;
                        }
                        else if (colIndex == 0)
                        {
                            slot = colIndex + 3;
                        }
                    }
                    if (((((rowIndex - 2) < sockets.GetLength(0) && (rowIndex - 2) > 0) && (sockets[rowIndex - 2, colIndex] == opponentPlayer)) && (((rowIndex + 1) > 0 && (rowIndex + 1) < sockets.GetLength(0)) && (sockets[rowIndex + 1, colIndex] != currentPlayer))) && (sockets[rowIndex + 1, colIndex] == 0))
                    {
                        if (((rowIndex > 0) && (colIndex > 0)) && (sockets[rowIndex + 1, colIndex - 1] != 0))
                        {
                            slot = colIndex - 1;
                        }
                        else if (colIndex == 0)
                        {
                            slot = colIndex + 1;
                        }
                    }
                }
                if ((((rowIndex - 3) < sockets.GetLength(0) && (rowIndex - 3) > 0) && (sockets[rowIndex - 1, colIndex] == 0)) && ((sockets[rowIndex - 2, colIndex] == opponentPlayer) && (sockets[rowIndex - 3, colIndex] == opponentPlayer)))
                {
                    if ((colIndex > 0) && (sockets[rowIndex - 1, colIndex - 1] != 0))
                    {
                        slot = colIndex + 1;
                    }
                    else if ((colIndex == 0) && (sockets[rowIndex - 1, colIndex] == 0))
                    {
                        slot = colIndex + 1;
                    }
                }
                if ((((rowIndex - 3) < sockets.GetLength(0) && (rowIndex - 3) > 0) && (sockets[rowIndex - 1, colIndex] == opponentPlayer)) && ((sockets[rowIndex - 2, colIndex] == 0) && (sockets[rowIndex - 3, colIndex] == opponentPlayer)))
                {
                    if ((colIndex > 0) && (sockets[rowIndex - 2, colIndex - 1] != 0))
                    {
                        slot = colIndex + 2;
                    }
                    else if ((colIndex == 0) && (sockets[rowIndex - 2, colIndex] == 0))
                    {
                        slot = colIndex + 2;
                    }
                }
            }
            if (((((rowIndex - 3) < sockets.GetLength(0) && (rowIndex - 3) > 0) && ((rowIndex + 1) >= 0 && (rowIndex + 1) < sockets.GetLength(0))) && ((sockets[rowIndex, colIndex] == opponentPlayer) && (sockets[rowIndex - 1, colIndex] == opponentPlayer))) && (((sockets[rowIndex - 2, colIndex] == 0) && (sockets[rowIndex - 3, colIndex] == 0)) && (sockets[rowIndex + 1, colIndex] == 0)))
            {
                slot = colIndex + 2;
            }
            if (
                (rowIndex - 2) < sockets.GetLength(0) && (rowIndex - 2) > 0 &&
                (rowIndex + 2) >= 0 && (rowIndex + 2) < sockets.GetLength(0) &&
                (sockets[rowIndex, colIndex] == opponentPlayer) && (sockets[rowIndex - 1, colIndex] == opponentPlayer) &&
                (sockets[rowIndex - 2, colIndex] == 0 && (sockets[rowIndex + 2, colIndex] == 0) && sockets[rowIndex + 1, colIndex] == 0))
            {
                slot = colIndex - 1;
            }
            return slot;
        }

        private static int Vertikal(int[,] sockets, int opponentPlayer, int currentPlayer, int slot, int rowIndex, int colIndex)
        {
            if ((sockets[rowIndex, colIndex] == opponentPlayer && (colIndex + 1) < sockets.GetLength(1)) &&
                (sockets[rowIndex, colIndex + 1] == opponentPlayer && (colIndex + 2) < sockets.GetLength(1)) &&
                (sockets[rowIndex, colIndex + 2] == opponentPlayer && (colIndex + 3) < sockets.GetLength(1)) &&
                (sockets[rowIndex, colIndex + 3] != currentPlayer && sockets[rowIndex, colIndex + 3] == 0))
            {
                slot = colIndex + 3;
            }
            return slot;
        }

        public string getTypeName()
        {
            return this.GetType().Name;
        }

        public string getFriendlyName()
        {
            return "Schmidt";
        }
    }
}