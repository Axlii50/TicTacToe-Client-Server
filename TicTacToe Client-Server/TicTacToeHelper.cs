using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicTacToe_Client_Server
{
    public static class TicTacToeHelper
    {
        public static char CheckWinner(char[,] board)
        {
            // Sprawdza wiersze
            for (int row = 0; row < 3; row++)
            {
                if (board[row, 0] == board[row, 1] && board[row, 1] == board[row, 2])
                {
                    if (board[row, 0] != '\0') // Znaleziono zwycięzcę
                        return board[row, 0];
                }
            }

            // Sprawdza kolumny
            for (int col = 0; col < 3; col++)
            {
                if (board[0, col] == board[1, col] && board[1, col] == board[2, col])
                {
                    if (board[0, col] != '\0') // Znaleziono zwycięzcę
                        return board[0, col];
                }
            }

            // Sprawdza przekątne
            if (board[0, 0] == board[1, 1] && board[1, 1] == board[2, 2] ||
                board[0, 2] == board[1, 1] && board[1, 1] == board[2, 0])
            {
                if (board[1, 1] != '\0') // Znaleziono zwycięzcę
                    return board[1, 1];
            }

            // Nie znaleziono zwycięzcy
            return '\0';
        }
    }
}
